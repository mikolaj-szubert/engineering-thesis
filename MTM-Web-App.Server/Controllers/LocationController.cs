using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Web;

using Microsoft.AspNetCore.Mvc;

using MTM_Web_App.Server.Helpers;
using Microsoft.Extensions.Localization;
using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/address")]
    public class LocationController(IConfiguration configuration, MTM_Web_AppServerContext context, HttpClient httpClient, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IStringLocalizer _localizer = localizer;

        /// <summary>Potencjalne adresy oraz współrzędne z zapytania</summary>
        /// <returns>Adresy</returns>
        /// <param name="addr" example="120 E Delaware Pl, Chicago, IL 60611, USA">Adres</param>
        /// <param name="ct">Cancellation token</param>
        /// <response code="404">Brak adresów z zapytania</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/hotels/listAddresses/address
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<HReturnCoordinates>>(StatusCodes.Status200OK)]
        [HttpGet("list/{addr}")]
        public async Task<IActionResult> ListAddresses(string addr, CancellationToken ct)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer, ct: ct);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            string? key = _configuration["Geocoding:Key"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("key for geocoding is not configured");

            string url = $"https://eu1.locationiq.com/v1/search?q={HttpUtility.UrlEncode(addr)}&limit=50&format=json&addressdetails=1&statecode=1&accept-language={Request.Headers.AcceptLanguage}&normalizeaddress=1&normalizecity=1&postaladdress=1&dedupe=1&key={key}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            //setting headers so the geocoding api could always return the same results
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");
            request.Headers.Add("Accept-Language", Request.Headers.AcceptLanguage.ToString());
            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonResponse = await response.Content.ReadAsStreamAsync(ct);
                //response to JSON
                using var jsonDoc = await JsonDocument.ParseAsync(jsonResponse, cancellationToken: ct);
                JsonElement root = jsonDoc.RootElement;
                List<object> returnCoordinates = [];
                for (int i = 0; i < root.GetArrayLength(); i++)
                {
                    try
                    {
                        string? latStr = root[i].GetProperty("lat").GetString();
                        string? lonStr = root[i].GetProperty("lon").GetString();
                        var latBool = double.TryParse(latStr, CultureInfo.InvariantCulture, out double lat);
                        var lonBool = double.TryParse(lonStr, CultureInfo.InvariantCulture, out double lon);
                        string? name = root[i].GetProperty("display_name").GetString();
                        bool isAddress = root[i].TryGetProperty("address",out var address);
                        isAddress = isAddress && address.TryGetProperty("house_number", out _);

                        if (latBool && lonBool && name != null && isAddress)
                            returnCoordinates.Add(new { name, lat, lon });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("BŁAD: " + ex.Message);
                    }

                }

                //logging
                await Safety.Log(ur.User, $"GET addresses", _context, _httpClient, HttpContext);

                if (returnCoordinates.Count > 0)
                    return Ok(returnCoordinates);
                else
                    return NotFound(_localizer.GetString("No results for given address.").Value);
            }
            else
            {
                Console.WriteLine("Response Not OK");
                return NotFound(_localizer.GetString("Invalid request.").Value); }
            }
    }
}
