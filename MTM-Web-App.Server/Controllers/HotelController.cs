using System.Collections.Immutable;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text.Json;
using System.Web;

using Humanizer;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Models;

using Newtonsoft.Json;

namespace MTM_Web_App.Server.Controllers
{
    [Route("api/hotels")]
    [ApiController]
    public class HotelController(IConfiguration configuration, MTM_Web_AppServerContext context, HttpClient httpClient, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IStringLocalizer _localizer = localizer;

        /// <summary>Zwraca wszystkie hotele</summary>
        /// <returns>Lista restauracji</returns>
        /// <response code="404">Brak hoteli</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/hotels/get
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<Hotel>>(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult> GetHotels(
            [FromQuery] string? city, 
            [FromQuery] decimal? minPrice, 
            [FromQuery] decimal? maxPrice, 
            [FromQuery] string? facilities, 
            [FromQuery] int? minRating, 
            [FromQuery] int? maxRating, 
            [FromQuery] DateOnly? startDate, 
            [FromQuery] DateOnly? endDate, 
            [FromQuery] string? types,
            [FromQuery] Currency? currency,
            CancellationToken ct )
        {
            List<FacilityHotel>? facilitiesHotelList = null;
            List<FacilityRoom>? facilitiesRoomList = null;
            List<ObjType>? objTypes = null;

            if (!string.IsNullOrEmpty(facilities))
            {
                var facilityStrings = facilities.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();

                facilitiesHotelList = facilityStrings
                    .Select(facilityString => Enum.TryParse<FacilityHotel>(facilityString, true, out var facility) ? facility : (FacilityHotel?)null)
                    .Where(facility => facility.HasValue)
                    .Select(facility => facility!.Value)
                    .ToList();

                facilitiesRoomList = facilityStrings
                    .Select(facilityString => Enum.TryParse<FacilityRoom>(facilityString, true, out var facility) ? facility : (FacilityRoom?)null)
                    .Where(facility => facility.HasValue)
                    .Select(facility => facility!.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(types))
            {
                var typesStrings = types.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
                objTypes = typesStrings
                    .Select(typesStrings => Enum.TryParse<ObjType>(typesStrings, true, out var type) ? type : (ObjType?)null)
                    .Where(type => type.HasValue)
                    .Select(type => type!.Value)
                    .ToList();
            }

            var hotelsQuery = _context.Hotels
                .Include(r => r.Ratings)
                .Include(a => a.Addresses)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Rooms)
                        .ThenInclude(re => re.Availabilities)
                .Include(r => r.Images)
                .Include(h => h.Rooms)
                    .ThenInclude(i => i.Images)
                .Where(h => h.Rooms.Count > 0 && h.Rooms.SelectMany(r => r.Images).Count() + h.Images.Count >= 3)
                .AsSplitQuery()
                .AsQueryable();

            //fitr miast
            if (!string.IsNullOrWhiteSpace(city))
            {
                string[] cityPart = city.Trim().ToLower().Split(" ");
                hotelsQuery = hotelsQuery.Where(h => 
                    h.Addresses.Any(a => cityPart.Any(c => c.Contains(a.City) || c.Contains(a.Country) || c.Contains(a.State) || c.Contains(a.Road))));
            }

            //fitr obiektów
            if (objTypes != null && objTypes.Count > 0)
                hotelsQuery = hotelsQuery.Where(h => objTypes.Contains(h.Type));

            //filtr udogodnień
            if ((facilitiesHotelList != null && facilitiesHotelList.Count > 0) ||
                (facilitiesRoomList != null && facilitiesRoomList.Count > 0))
            {
                hotelsQuery = hotelsQuery.Where(h =>
                    (facilitiesHotelList == null || facilitiesHotelList.All(f => h.Facilities.Contains(f))) &&
                    (facilitiesRoomList == null || h.Rooms.Any(room => facilitiesRoomList.All(f => room.Facilities.Contains(f))))
                );
            }

            //kowersja na listę
            var hotels = await hotelsQuery.ToListAsync(ct);

            //filtry oceny
            if (minRating.HasValue && minRating.Value > 1)
                hotels = hotels.Where(h => h.Rating >= minRating.Value).ToList();

            if (maxRating.HasValue && maxRating.Value < 5)
                hotels = hotels.Where(h => h.Rating <= maxRating.Value).ToList();

            //filtr dostępności
            if (startDate.HasValue && endDate.HasValue)
            {
                if ((endDate.Value.ToDateTime(new TimeOnly()) - startDate.Value.ToDateTime(new TimeOnly())).Days > 0)
                    hotels = hotels.Where(hotel =>
                        hotel.Rooms.Any(room =>
                            room.Rooms.Any(re =>
                            Enumerable
                                .Range(0, (endDate.Value.ToDateTime(new TimeOnly()) - startDate.Value.ToDateTime(new TimeOnly())).Days + 1)
                                .Select(offset => startDate.Value.AddDays(offset))
                                .All(date =>
                                    !re.Availabilities.Any(av => av.Date == date && av.IsReserved)
                                )
                            )
                        )
                    ).ToList();
                else return NotFound("Cannot search for stay shorter than one day.");
            }

            if (currency == null && (minPrice != null || maxPrice != null))
                return BadRequest(); //DODAĆ ODPOWIEDŹ - NIE MOŻNA SPRAWDZIĆ CEN DLA RÓŻNYCH WALUT

            if (currency != null)
            {
                Currency cur = (Currency)currency;
                // Przeliczenie walut dla wszystkich hoteli
                using var httpClient = new HttpClient();
                foreach (var hotel in hotels)
                {
                    if (hotel.Currency != cur)
                    {
                        try
                        {
                            string endpoint = $"https://api.frankfurter.app/latest?base={hotel.Currency}&symbols={cur}";
                            var response = await httpClient.GetAsync(endpoint, ct);

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync(ct);
                                ExchangeRateResponse? exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(content);

                                if (exchangeRateResponse?.Rates != null && exchangeRateResponse.Rates.TryGetValue(cur.ToString().ToUpperInvariant(), out float rate))
                                {
                                    foreach (var room in hotel.Rooms)
                                    {
                                        room.Price *= (decimal)rate;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Błąd podczas konwersji waluty: {ex.Message}");
                        }
                    }
                }

                //filtr ceny (obowiązkowo po konwersji waluty)
                if (minPrice.HasValue || maxPrice.HasValue)
                {
                    hotels = hotels.Where(h => 
                        h.Rooms.Any(r => (minPrice.HasValue && r.Price >= minPrice.Value) && (maxPrice.HasValue && r.Price <= maxPrice.Value))
                    ).ToList();
                }
            }

            if (hotels.Count == 0)
                return NotFound(new { Result = _localizer.GetString("No hotels with specified filters.").Value });
            string locale = Request.Headers.AcceptLanguage.ToString()[..2] == "pl" ? "pl" : "en";
            string? currencyCode = currency switch
            {
                Currency.PLN => "pl-PL",
                Currency.GBP => "en-GB",
                Currency.NZD => "mi-NZ",
                Currency.JPY => "ja-JP",
                Currency.CAD => "en-CA",
                Currency.AUD => "en-AU",
                Currency.CHF => "de-CH",
                Currency.EUR => "fr-FR",
                Currency.INR => "en-IN",
                Currency.USD => "en-US",
                _ => null,
            };
            var result = hotels.Select(h => new
            {
                h.Name,
                h.Addresses.First(a => a.Locale == locale).City,
                h.Addresses.First(a => a.Locale == locale).Country,
                h.Description,
                HotelCurrency = h.Currency,
                MinPrice = h.Rooms.Count != 0 ? h.Rooms.Min(room => room.Price).ToString(currencyCode != "" ? "C2" : "D2", currencyCode != null ? CultureInfo.CreateSpecificCulture(currencyCode) : null) : null,
                MaxPrice = (decimal?)h.Rooms.Count != 0 ? h.Rooms.Max(room => room.Price) : (decimal?)null,
                Image = h.Images.Select(i => i.ImageSrc).FirstOrDefault() ?? h.Rooms.SelectMany(r => r.Images.Select(i => i.ImageSrc)).FirstOrDefault(),
                h.Rating,
                h.Facilities,
                Coordinates = new[] { h.Lat, h.Lon }, //na potrzeby mapy
            }).ToList();
            var maxPrices = hotels.Select(h => h.Rooms.Max(t => t.Price));
            return Ok(new { result, MaxPrice = maxPrices.Max() });
        }

        /// <summary>Zwraca dane konkretnego hotelu</summary>
        /// <returns>Dane hotelu</returns>
        /// <param name="name" example="California Haze">Nazwa hotelu</param>
        /// <param name="currency">Waluta</param>
        /// <response code="404">Brak hotelu</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/hotels/get/name
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Hotel>(StatusCodes.Status200OK)]
        [HttpGet("{name}")]
        public async Task<ActionResult> GetHotel(string name, [FromQuery] Currency currency = Currency.USD)
        {
            var hotel = await _context.Hotels
                .Include(r => r.Ratings)
                .Include(u => u.OwnerUser)
                .Include(r => r.Images)
                .Include(a => a.Addresses)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Rooms)
                    .ThenInclude(re => re.Availabilities)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Images)
                .Where(h => h.Name == name)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (hotel == null)
            { 
                return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);
            }

            using var httpClient = new HttpClient();
            if (hotel.Currency != currency)
            {
                try
                {
                    string endpoint = $"https://api.frankfurter.app/latest?base={hotel.Currency}&symbols={currency}";
                    var response = await httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        ExchangeRateResponse? exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(content);

                        if (exchangeRateResponse?.Rates != null && exchangeRateResponse.Rates.TryGetValue(currency.ToString().ToUpperInvariant(), out float rate))
                        {
                            foreach (var room in hotel.Rooms)
                            {
                                room.Price *= (decimal)rate;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas konwersji waluty: {ex.Message}");
                }
            }

            string? currencyCode = currency switch
            {
                Currency.PLN => "pl-PL",
                Currency.GBP => "en-GB",
                Currency.NZD => "mi-NZ",
                Currency.JPY => "ja-JP",
                Currency.CAD => "en-CA",
                Currency.AUD => "en-AU",
                Currency.CHF => "de-CH",
                Currency.EUR => "fr-FR",
                Currency.INR => "en-IN",
                Currency.USD => "en-US",
                _ => null,
            };

            string? acceptLanguageHeader = Request.Headers.AcceptLanguage.FirstOrDefault();

            string? cultureName = acceptLanguageHeader?.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault();

            CultureInfo culture;
            try
            {
                culture = !string.IsNullOrWhiteSpace(cultureName) ? new CultureInfo(cultureName) : CultureInfo.InvariantCulture;
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.InvariantCulture;
            }

            // Use the resolved culture for formatting
            string checkInTime = hotel.CheckIn.ToShortTimeString().ToString(culture);
            string checkOutTime = hotel.CheckOut.ToShortTimeString().ToString(culture);
            string locale = Request.Headers.AcceptLanguage.ToString()[..2] == "pl" ? "pl" : "en";

            object result = new
            {
                hotel.Name,
                CheckIn = checkInTime,
                CheckOut = checkOutTime,
                Images = hotel.Images.Select(i => i.ImageSrc),
                Address = hotel.Addresses.Where(a => a.Locale == locale).Select(a => new { 
                    a.Road, 
                    a.HouseNumber, 
                    a.PostalCode, 
                    a.City, 
                    a.Country, 
                    a.State 
                }).FirstOrDefault(),
                hotel.Description,
                hotel.Lat,
                hotel.Lon,
                hotel.Facilities,
                hotel.Currency,
                Rooms = hotel.Rooms.Select(r => new { 
                    r.Name, 
                    Price = r.Price.ToString(currencyCode != "" ? "C2" : "D2", currencyCode != null ? CultureInfo.CreateSpecificCulture(currencyCode) : null), 
                    Images = r.Images.Select(i => i.ImageSrc) 
                }),
                Coordinates = new[] { hotel.Lat, hotel.Lon },
                Owner = hotel.OwnerUser.Name,
                OwnerEmail = hotel.OwnerUser.Email,
                hotel.Rating
            };
            return Ok(result);
        }

        /// <summary>Zwraca dane pokojów hotelu</summary>
        /// <returns>Pokoje</returns>
        /// <param name="name" example="California Haze">Nazwa hotelu</param>
        /// <param name="currency" example="USD">Waluta</param>
        /// <param name="start">początkowa data rezerwacji</param>
        /// <param name="end">końcowa data rezerwacji</param>
        /// <response code="404">Brak hotelu</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/hotels/name/rooms
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Hotel>(StatusCodes.Status200OK)]
        [HttpGet("{name}/rooms")]
        public async Task<ActionResult> GetHotelRooms(string name, [FromQuery] DateOnly? start, [FromQuery] DateOnly? end, [FromQuery] Currency currency = Currency.USD)
        {
            var hotel = await _context.Hotels
                .Include(u => u.OwnerUser)
                .Include(r => r.Images)
                .Include(a => a.Addresses)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Rooms)
                    .ThenInclude(re => re.Availabilities)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Images)
                .Where(h => h.Name == name)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (hotel == null)
            { 
                return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);
            }

            //przeliczenie walut dla wszystkich hoteli
            using var httpClient = new HttpClient();
            if (hotel.Currency != currency)
            {
                try
                {
                    string endpoint = $"https://api.frankfurter.app/latest?base={hotel.Currency}&symbols={currency}";
                    var response = await httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        ExchangeRateResponse? exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(content);

                        if (exchangeRateResponse?.Rates != null && exchangeRateResponse.Rates.TryGetValue(currency.ToString().ToUpperInvariant(), out float rate))
                        {
                            foreach (var room in hotel.Rooms)
                            {
                                room.Price *= (decimal)rate;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas konwersji waluty: {ex.Message}");
                }
            }

            string? acceptLanguageHeader = Request.Headers.AcceptLanguage.FirstOrDefault();

            string? cultureName = acceptLanguageHeader?.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault();

            CultureInfo culture;
            try
            {
                culture = !string.IsNullOrWhiteSpace(cultureName) ? new CultureInfo(cultureName) : CultureInfo.InvariantCulture;
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.InvariantCulture;
            }

            List<Room>? availableRooms;
            if (start.HasValue && end.HasValue)
            {
                availableRooms = hotel.Rooms
                    .Where(room =>
                        room.Hotel.Name == name &&
                        room.Rooms.Any(re =>
                            Enumerable
                                .Range(0, (end.Value.ToDateTime(new TimeOnly()) - start.Value.ToDateTime(new TimeOnly())).Days + 1)
                                .Select(offset => start.Value.AddDays(offset))
                                .All(date =>
                                    !re.Availabilities.Any(av => av.Date == date && av.IsReserved)
                                )
                        )
                    )
                    .ToList();

                availableRooms = availableRooms.Count == 0 ? null : availableRooms;
            }
            else
            {
                availableRooms = [.. hotel.Rooms];
            }

            string? currencyCode = currency switch
            {
                Currency.PLN => "pl-PL",
                Currency.GBP => "en-GB",
                Currency.NZD => "mi-NZ",
                Currency.JPY => "ja-JP",
                Currency.CAD => "en-CA",
                Currency.AUD => "en-AU",
                Currency.CHF => "de-CH",
                Currency.EUR => "fr-FR",
                Currency.INR => "en-IN",
                Currency.USD => "en-US",
                _ => null,
            };

            var res = availableRooms?.Select(room => new
            {
                room.Name,
                room.Description,
                Price = room.Price.ToString(currencyCode != "" ? "C2" : "D2", currencyCode != null ? CultureInfo.CreateSpecificCulture(currencyCode) : null),
                Images = room.Images.Select(i => i.ImageSrc),
                room.Facilities,
                room.PersonCount
            });

            return Ok(res);
        }

        /// <summary>Zwraca wszystkie posiadane hotele</summary>
        /// <returns>Hotele</returns>
        /// <response code="404">Brak posiadanych hoteli</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/hotels/get
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<Hotel>>(StatusCodes.Status200OK)]
        [HttpGet("owned")]
        public async Task<IActionResult> GetOwnedHotels(CancellationToken ct)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer, ct: ct);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            string? locale = Request.Headers.AcceptLanguage.ToString()[..2] == "pl" ? "pl" : "en";

            var hotels = await _context.Hotels.Include(a=>a.Addresses).Include(r=>r.Rooms).ThenInclude(i=>i.Images).Where(u => u.UserId == ur.User.UserId).Select(e => new
            {
                e.Name,
                e.Addresses.First(a => a.Locale == locale).City,
                e.Addresses.First(a => a.Locale == locale).Country,
                e.Description,
                Price = e.Rooms.Count != 0 ? e.Rooms.Min(room => room.Price) : (decimal?)null,
                Images = e.Rooms.SelectMany(room => room.Images.Select(r => r.ImageSrc)).ToList(),
                e.Rating
            }).ToListAsync(ct);
            if (hotels.Count == 0)
                return NotFound(_localizer.GetString("No owned hotels.").Value);

            //logging
            await Safety.Log(ur.User, $"GET ownedHotels", _context, _httpClient, HttpContext);

            return Ok(hotels);
        }

        /// <summary>Aktualizuje dane w bazie danych</summary>
        /// <returns>Nic</returns>
        /// <param name="hotelDto">Obiekt z danymi, które chcemy zaktualizować.</param>
        /// <see cref="UpdateHotelDto"/>
        /// <seealso cref="Currency"/>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // PUT: api/hotels/put/name
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPut]
        public async Task<IActionResult> UpdateHotel([FromForm] UpdateHotelDto hotelDto)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            if (hotelDto.Name == null)
                return BadRequest(_localizer.GetString("Current hotel name cannot be empty.").Value);

            if (hotelDto.NewName != null && await _context.Hotels.AnyAsync(e => e.Name == hotelDto.NewName.Trim() && e.UserId != ur.User.UserId))
                return BadRequest(_localizer.GetString("Given name is already taken.").Value);

            var h = await _context.Hotels.Include(a=>a.Addresses).Include(i=>i.Images).FirstOrDefaultAsync(h => h.Name == hotelDto.Name && h.UserId == ur.User.UserId);
            if (h == null)
                return NotFound(_localizer.GetString("You do not own hotel with given name.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? acceptLanguageHeader = Request.Headers.AcceptLanguage.FirstOrDefault();
                string? cultureName = acceptLanguageHeader?.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault();
                CultureInfo culture;
                try
                {
                    culture = !string.IsNullOrWhiteSpace(cultureName) ? new CultureInfo(cultureName) : CultureInfo.InvariantCulture;
                }
                catch (CultureNotFoundException)
                {
                    culture = CultureInfo.InvariantCulture; // Fallback to invariant culture if parsing fails
                }

                string? oldName = h.Name;
                if (hotelDto.NewName != null && hotelDto.NewName != h.Name && !await _context.Hotels.AnyAsync(e => e.Name == hotelDto.NewName && e.HotelId != h.HotelId))
                {
                    h.Name = hotelDto.NewName.Trim();
                }

                if (hotelDto.Description != null && hotelDto.Description != h.Description)
                    h.Description = hotelDto.Description.Trim();

                if (hotelDto.HotelCurrency != null && hotelDto.HotelCurrency != h.Currency)
                    h.Currency = (Currency)hotelDto.HotelCurrency;

                if (!string.IsNullOrEmpty(hotelDto.CheckIn))
                {
                    if (!TimeOnly.TryParse(hotelDto.CheckIn, culture, out TimeOnly checkIn)) return BadRequest(_localizer.GetString("Invalid check-in time format.").Value);
                    if (h.CheckIn != checkIn) h.CheckIn = checkIn;
                }

                if (!string.IsNullOrEmpty(hotelDto.CheckOut))
                {
                    if (!TimeOnly.TryParse(hotelDto.CheckOut, culture, out TimeOnly checkOut)) return BadRequest(_localizer.GetString("Invalid check-out time format.").Value);
                    if (h.CheckOut != checkOut) h.CheckOut = checkOut;
                }

                if (hotelDto.Type != null && hotelDto.Type != h.Type)
                    h.Type = (ObjType)hotelDto.Type;

                if (hotelDto.Facilities != null && hotelDto.Facilities != h.Facilities)
                    h.Facilities = hotelDto.Facilities;

                if (hotelDto.Lat != null && hotelDto.Lon != null)
                {
                    var addresses = await Safety.GetLocalisedAddresses(hotelDto.Lat, hotelDto.Lon, _httpClient, _configuration);
                    if (addresses == null)
                        return BadRequest("Podano błędny adres."); //DODAĆ ODPOWIEDŹ

                    var oldAddresses = h.Addresses.ToList();
                    foreach (var address in oldAddresses)
                        _context.Addresses.Remove(address);
                    await _context.SaveChangesAsync();

                    h.Addresses = addresses;
                    h.Lon = hotelDto.Lon.Replace(",", ".");
                    h.Lat = hotelDto.Lat.Replace(",", ".");
                }
                await _context.SaveChangesAsync();

                //images adding
                if (hotelDto.Files != null)
                {
                    string oldUploadPath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{oldName.Dehumanize()}\\";
                    foreach (var img in h.Images)
                    {
                        string path = Path.Combine(oldUploadPath, img.ImageSrc);
                        if (!h.Images.Remove(img)) throw new Exception("Error removing old images.");
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }
                    if (!Directory.EnumerateFiles(oldUploadPath).Any()) Directory.Delete(oldUploadPath);

                    string newUploadPath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{(hotelDto.NewName ?? oldName).Dehumanize()}\\";
                    if (!Directory.Exists(newUploadPath)) Directory.CreateDirectory(newUploadPath);
                    foreach (var f in hotelDto.Files)
                    {
                        if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                            string filePath = Path.Combine(newUploadPath, fileName);

                            using (FileStream stream = new(filePath, FileMode.Create))
                                await f.CopyToAsync(stream);

                            h.Images.Add(new HotelImage { Hotel = h, HotelId = h.HotelId, ImageSrc = fileName });
                        }
                        else
                            return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                    }
                    await _context.SaveChangesAsync();
                }
                else if (hotelDto.Files == null && hotelDto.NewName != null)
                    Directory.Move($"..\\MTM-Web-App.Server\\Img\\Hotels\\{oldName.Dehumanize()}\\", $"..\\MTM-Web-App.Server\\Img\\Hotels\\{hotelDto.NewName.Dehumanize()}\\");

                //logging
                await Safety.Log(ur.User, $"PUT hotel", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Hotel updated!").Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return Problem(
                    title: _localizer.GetString("Error occurred. Please try again later.").Value,
                    detail: _localizer.GetString("If the problem persists, please contact us.").Value,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        /// <summary>Dodaje hotel</summary>
        /// <param name="hotelDto">Obiekt z danymi hotelu</param>
        /// <see cref="PostHotelDto"/>
        /// <seealso cref="Currency"/>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email/Błąd w <c>hotelDto</c></response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/hotels/add
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> AddHotel([FromForm] PostHotelDto hotelDto)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            string? acceptLanguageHeader = Request.Headers.AcceptLanguage.FirstOrDefault();
            string? cultureName = acceptLanguageHeader?.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault();
            CultureInfo culture;
            try
            {
                culture = !string.IsNullOrWhiteSpace(cultureName) ? new CultureInfo(cultureName) : CultureInfo.InvariantCulture;
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.InvariantCulture; // Fallback to invariant culture if parsing fails
            }

            if (!TimeOnly.TryParse(hotelDto.CheckIn, culture, out TimeOnly checkIn))
                return BadRequest(_localizer.GetString("Invalid check-in time format.").Value);

            if (!TimeOnly.TryParse(hotelDto.CheckOut, culture, out TimeOnly checkOut))
                return BadRequest(_localizer.GetString("Invalid check-out time format.").Value);

            if (!Enum.IsDefined(typeof(Currency), hotelDto.HotelCurrency))
                return BadRequest(_localizer.GetString("Given currency is not yet supported.").Value);

            if(hotelDto.Facilities == null)
                return BadRequest(_localizer.GetString("Incorrect facilities.").Value);

            if (await _context.Hotels.AnyAsync(e => e.Name.ToLower() == hotelDto.Name.ToLower()))
                return BadRequest(_localizer.GetString("Given name is already taken.").Value);

            if (hotelDto.Files == null || hotelDto.Files.Count == 0)
                return BadRequest(_localizer.GetString("No images added.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var addresses = await Safety.GetLocalisedAddresses(hotelDto.Lat, hotelDto.Lon, _httpClient, _configuration);
                if (addresses == null) 
                    return BadRequest(_localizer.GetString("Given address is invalid."));

                var hotelData = new Hotel
                {
                    Name = hotelDto.Name.Trim(),
                    Currency = hotelDto.HotelCurrency,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Type = hotelDto.Type,
                    Facilities = hotelDto.Facilities,
                    Addresses = addresses,
                    Lat = hotelDto.Lat.Replace(",","."),
                    Lon = hotelDto.Lon.Replace(",", "."),
                    Description = hotelDto.Description,
                    UserId = ur.User.UserId,
                    OwnerUser = ur.User
                };
                await _context.Hotels.AddAsync(hotelData);
                await _context.SaveChangesAsync();

                string uploadPath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{hotelData.Name.Dehumanize()}\\";

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                foreach (var f in hotelDto.Files)
                {
                    if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);

                        using (FileStream stream = new(filePath, FileMode.Create))
                            await f.CopyToAsync(stream);

                        hotelData.Images.Add(new HotelImage { Hotel = hotelData, HotelId = hotelData.HotelId, ImageSrc = fileName });
                    }
                    else
                        return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                }
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, $"POST hotel", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Hotel added!").Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return Problem(
                    title: _localizer.GetString("Error occurred. Please try again later.").Value,
                    detail: _localizer.GetString("If the problem persists, please contact us.").Value,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        /// <summary>Usuwa hotel</summary>
        /// <param name="name" example="California Haze">Nazwa hotelu</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak hotelu o podanej nazwie</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // DELETE: api/hotels/name
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteHotel(string name)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var hotelData = await _context.Hotels.Include(a => a.Addresses).Include(i => i.Images).Include(t => t.Rooms).ThenInclude(i => i.Images).AsSplitQuery().FirstOrDefaultAsync(e => e.Name == name && e.UserId == ur.User.UserId);
            if (hotelData == null)
                return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                hotelData.Addresses.RemoveRange(0, hotelData.Addresses.Count);
                await _context.SaveChangesAsync();

                string deletePath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{hotelData.Name.Dehumanize()}\\";
                foreach (var img in hotelData.Images.ToList())
                    if (!hotelData.Images.Remove(img))
                        throw new Exception("Error removing old images.");

                foreach(var r in hotelData.Rooms.ToList())
                    foreach(var img in r.Images.ToList())
                        if (!r.Images.Remove(img)) 
                            throw new Exception("Error removing old images.");

                Directory.Delete(deletePath, true);

                _context.RemoveRange(hotelData.Rooms);
                await _context.SaveChangesAsync();
                _context.Hotels.Remove(hotelData);
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, $"DELETE hotel", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Hotel deleted").Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return Problem(
                    title: _localizer.GetString("Error occurred. Please try again later.").Value,
                    detail: _localizer.GetString("If the problem persists, please contact us.").Value,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        /// <summary>Sprawdza czy podana nazwa hotelu jest wolna</summary>
        /// <param name="name" example="California Haze">Nazwa hotelu</param>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik <c>true</c>/<c>false</c></response>
        // GET: api/hotels/doesHotelExist/name
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("is-name-free/{name}")]
        public async Task<IActionResult> HotelDataExists(string name)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            //logging
            await Safety.Log(ur.User, $"GET freeName", _context, _httpClient, HttpContext);

            return Ok(!await _context.Hotels.AnyAsync(e => e.Name == name.Trim()));
        }
    }
}
