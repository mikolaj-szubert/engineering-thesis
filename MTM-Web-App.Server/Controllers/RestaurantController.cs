using System.Globalization;

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
    [Route("api/restaurants")]
    [ApiController]
    public class RestaurantController(IConfiguration configuration, MTM_Web_AppServerContext context, HttpClient httpClient, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IStringLocalizer _localizer = localizer;

        /// <summary>Zwraca wszystkie restauracje</summary>
        /// <returns>Lista restauracji</returns>
        /// <response code="404">Brak restauracji</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/restaurants/get
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<IEnumerable<Restaurant>>(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult> GetRestaurants(
            [FromQuery] string? city,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? cusines,
            [FromQuery] int? minRating,
            [FromQuery] int? maxRating,
            [FromQuery] DateOnly? date,
            [FromQuery] Currency? currency,
            CancellationToken ct)
        {
            List<Cusine>? facilitiesHotelList = null;

            if (!string.IsNullOrEmpty(cusines))
            {
                var facilityStrings = cusines.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();

                facilitiesHotelList = facilityStrings
                    .Select(facilityString => Enum.TryParse<Cusine>(facilityString, true, out var facility) ? facility : (Cusine?)null)
                    .Where(facility => facility.HasValue)
                    .Select(facility => facility!.Value)
                    .ToList();
            }

            var restaurantQuery = _context.Restaurants
                .Include(r => r.Ratings)
                .Include(a => a.Addresses)
                .Include(h => h.Tables)
                    .ThenInclude(r => r.Tables)
                        .ThenInclude(re => re.Availabilities)
                .Include(i => i.Images)
                .Include(r => r.Tables)
                    .ThenInclude(i => i.Images)
                .Where(h => h.Tables.Count > 0 && h.Tables.SelectMany(r => r.Images).Count() + h.Images.Count >= 3)
                .AsSplitQuery()
                .AsQueryable();

            //fitr miast
            if (!string.IsNullOrWhiteSpace(city))
            {
                string[] cityPart = city.Trim().ToLower().Split(" ");
                restaurantQuery = restaurantQuery.Where(h =>
                    h.Addresses.Any(a => cityPart.Any(c => c.Contains(a.City) || c.Contains(a.Country) || c.Contains(a.State) || c.Contains(a.Road))));
            }

            //filtr udogodnień
            if ((facilitiesHotelList != null && facilitiesHotelList.Count > 0))
            {
                restaurantQuery = restaurantQuery.Where(h => facilitiesHotelList == null || facilitiesHotelList.All(f => h.Cusines.Contains(f)));
            }

            //kowersja na listę
            var restaurants = await restaurantQuery.ToListAsync(ct);

            //filtry oceny
            if (minRating.HasValue && minRating.Value > 1)
            {
                restaurants = restaurants.Where(h => h.Rating >= (double)minRating.Value).ToList();
            }
            Console.WriteLine(maxRating);
            if (maxRating.HasValue && maxRating.Value < 5)
            {
                Console.WriteLine(true);
                restaurants = restaurants.Where(h => h.Rating <= (double)maxRating.Value).ToList();
            }

            //filtr dostępności
            if (date.HasValue)
            {
                restaurants = restaurants.Where(hotel =>
                    hotel.Tables.Any(room =>
                        room.Tables.Any(roomEntity => 
                            !roomEntity.Availabilities.Any(ra => 
                                ra.Date == date && ra.IsReserved
                            )
                        )
                    )
                ).ToList();
            }
            if (currency == null && (minPrice != null || maxPrice != null))
                return BadRequest(); //DODAĆ ODPOWIEDŹ - NIE MOŻNA SPRAWDZIĆ CEN DLA RÓŻNYCH WALUT

            if (currency != null)
            {
                Currency cur = (Currency)currency;
                // Przeliczenie walut dla wszystkich hoteli
                using var httpClient = new HttpClient();
                foreach (var hotel in restaurants)
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
                                    foreach (var room in hotel.Tables)
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
                    restaurants = restaurants.Where(h =>
                        h.Tables.Any(r => (minPrice.HasValue && r.Price >= minPrice.Value) && (maxPrice.HasValue && r.Price <= maxPrice.Value))
                    ).ToList();
                }
            }

            if (restaurants.Count == 0)
                return NotFound( new { Result = _localizer.GetString("No restaurants with specified filters.").Value });
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
            var result = restaurants.Select(h => new
            {
                h.Name,
                h.Addresses.First(a => a.Locale == locale).City,
                h.Addresses.First(a => a.Locale == locale).Country,
                h.Description,
                HotelCurrency = h.Currency,
                MinPrice = h.Tables.Count != 0 ? h.Tables.Min(room => room.Price).ToString(currencyCode != "" ? "C2" : "D2", currencyCode != null ? CultureInfo.CreateSpecificCulture(currencyCode) : null) : null,
                Image = h.Images.Select(i => i.ImageSrc).FirstOrDefault() ?? h.Tables.SelectMany(r => r.Images.Select(i => i.ImageSrc)).FirstOrDefault(),
                h.Rating,
                h.Cusines,
                Coordinates = new[] { h.Lat, h.Lon } //dla mapy
            }).ToList();
            var maxPrices = restaurants.Select(h => h.Tables.Max(t => t.Price));
            return Ok(new { result, MaxPrice = maxPrices.Max() });
        }

        /// <summary>Zwraca dane konkretnej restauracji</summary>
        /// <returns>Dane restauracji</returns>
        /// <param name="name" example="Pizza Hut">Nazwa restauracji</param>
        /// <param name="currency">Waluta</param>
        /// <response code="404">Brak restauracji</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/restaurants/get/name
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<Restaurant>(StatusCodes.Status200OK)]
        [HttpGet("{name}")]
        public async Task<ActionResult> GetRestaurant(string name, [FromQuery] Currency currency = Currency.USD)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Ratings)
                .Include(u => u.OwnerUser)
                .Include(i=>i.Images)
                .Include(i=>i.OpenDays)
                .Include(a=>a.Addresses)
                .Include(h => h.Tables)
                    .ThenInclude(r => r.Tables)
                    .ThenInclude(re => re.Availabilities)
                .Include(r => r.Tables)
                    .ThenInclude(i => i.Images)
                .Where(h => h.Name == name)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return NotFound(_localizer.GetString("Specified restaraurant does not exist.").Value);
            }

            //przeliczenie walut dla wszystkich hoteli
            using var httpClient = new HttpClient();
            if (restaurant.Currency != currency)
            {
                try
                {
                    string endpoint = $"https://api.frankfurter.app/latest?base={restaurant.Currency}&symbols={currency}";
                    var response = await httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        ExchangeRateResponse? exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(content);

                        if (exchangeRateResponse?.Rates != null && exchangeRateResponse.Rates.TryGetValue(currency.ToString().ToUpperInvariant(), out float rate))
                        {
                            foreach (var table in restaurant.Tables)
                            {
                                table.Price *= (decimal)rate;
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

            // Extract the first valid culture from the Accept-Language header
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
            string? locale = Request.Headers.AcceptLanguage.ToString()[..2] == "pl" ? "pl" : "en";

            object result = new
            {
                restaurant.Name,
                Images = restaurant.Images.Select(i => i.ImageSrc),
                Address = restaurant.Addresses.Where(a => a.Locale == locale).Select(a => new {
                    a.Road, 
                    a.HouseNumber, 
                    a.PostalCode, 
                    a.City, 
                    a.Country, 
                    a.State
                }).FirstOrDefault(),
                restaurant.Description,
                OpenDays = restaurant.OpenDays.Select(a => new { a.DayOfWeek, OpeningTime = a.OpeningTime.ToShortTimeString().ToString(culture), ClosingTime = a.ClosingTime.ToShortTimeString().ToString(culture) }),
                restaurant.Lat,
                restaurant.Lon,
                restaurant.Cusines,
                restaurant.Currency,
                Tables = restaurant.Tables.Select(table => new { 
                    table.Name,
                    Price = table.Price.ToString(currencyCode != "" ? "C2" : "D2", currencyCode != null ? CultureInfo.CreateSpecificCulture(currencyCode) : null),
                    Images = table.Images.Select(t => t.ImageSrc)
                }),
                Coordinates = new[] { restaurant.Lat, restaurant.Lon },
                Owner = restaurant.OwnerUser.Name,
                OwnerEmail = restaurant.OwnerUser.Email,
                restaurant.Rating
            };
            return Ok(result);
        }

        /// <summary>Zwraca dane pokojów hotelu</summary>
        /// <returns>Pokoje</returns>
        /// <param name="name" example="California Haze">Nazwa hotelu</param>
        /// <param name="currency" example="USD">Waluta</param>
        /// <param name="date">początkowa data rezerwacji</param>
        /// <response code="404">Brak hotelu</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/hotels/name/rooms
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Hotel>(StatusCodes.Status200OK)]
        [HttpGet("{name}/tables")]
        public async Task<ActionResult> GetHotelRooms(string name, [FromQuery] DateOnly? date, [FromQuery] Currency currency = Currency.USD)
        {
            var restaurant = await _context.Restaurants
                .Include(u => u.OwnerUser)
                .Include(r => r.Images)
                .Include(a => a.Addresses)
                .Include(h => h.Tables)
                    .ThenInclude(r => r.Tables)
                    .ThenInclude(re => re.Availabilities)
                .Include(h => h.Tables)
                    .ThenInclude(r => r.Images)
                .Where(h => h.Name == name)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);
            }

            //przeliczenie walut dla wszystkich hoteli
            using var httpClient = new HttpClient();
            if (restaurant.Currency != currency)
            {
                try
                {
                    string endpoint = $"https://api.frankfurter.app/latest?base={restaurant.Currency}&symbols={currency}";
                    var response = await httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        ExchangeRateResponse? exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(content);

                        if (exchangeRateResponse?.Rates != null && exchangeRateResponse.Rates.TryGetValue(currency.ToString().ToUpperInvariant(), out float rate))
                        {
                            foreach (var room in restaurant.Tables)
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

            List<Table>? availableRooms;
            if (date.HasValue) {
                availableRooms = restaurant.Tables.Where(room => 
                    room.Restaurant.Name == name && room.Tables.Any(re => 
                        !re.Availabilities.Any(av => 
                            av.Date == date && av.IsReserved
                        )
                    )
                ).ToList();
                availableRooms = availableRooms.Count == 0 ? null : availableRooms;
            }
            else
            {
                availableRooms = [.. restaurant.Tables];
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
                room.PersonCount
            });

            return Ok(res);
        }

        /// <summary>Zwraca posiadane restauracje</summary>
        /// <returns>Restauracje</returns>
        /// <response code="404">Brak posiadanych restauracji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/restaurants/get
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<Restaurant>>(StatusCodes.Status200OK)]
        [HttpGet("owned")]
        public async Task<IActionResult> GetOwnedRestaurants()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            string? locale = Request.Headers.AcceptLanguage.ToString()[..2] == "pl" ? "pl" : "en";

            var restaurants = await _context.Restaurants.Where(u => u.UserId == ur.User.UserId).Select(e => new
            {
                e.Name,
                e.Cusines,
                OpenDays = e.OpenDays.Select(i => new { i.OpeningTime, i.ClosingTime, i.DayOfWeek }).ToList(),
                e.Addresses.First(a => a.Locale == locale).City,
                e.Addresses.First(a => a.Locale == locale).Country,
                e.Description,
                e.Lat,
                e.Lon,
                Images = e.Images.Select(r => r.ImageSrc),
                Owner = e.OwnerUser.Name,
                OwnerEmail = e.OwnerUser.Email
            }).ToListAsync();
            if (restaurants.Count == 0)
                return NotFound(_localizer.GetString("No owned restaurants.").Value);

            //logging
            await Safety.Log(ur.User, "GET ownedRestaurants", _context, _httpClient, HttpContext);

            return Ok(restaurants);
        }

        /// <summary>Aktualizuje dane w bazie danych</summary>
        /// <returns>Nic</returns>
        /// <param name="restaurantDto">Obiekt z danymi, które chcemy zaktualizować.</param>
        /// <see cref="UpdateRestaurantDto"/>
        /// <seealso cref="Currency"/>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // PUT: api/restaurants/put/name
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPut]
        public async Task<IActionResult> UpdateRestaurant([FromForm] UpdateRestaurantDto restaurantDto)
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

            if (restaurantDto.Name == null)
                return BadRequest(_localizer.GetString("Current restaurant name cannot be empty.").Value);

            if (restaurantDto.NewName != null && await _context.Restaurants.AnyAsync(e => e.Name == restaurantDto.NewName.Trim() && e.UserId != ur.User.UserId))
                return BadRequest(_localizer.GetString("Given name is already taken.").Value);

            var r = await _context.Restaurants.Include(a => a.Addresses).Include(i => i.Images).FirstOrDefaultAsync(h => h.Name == restaurantDto.Name && h.UserId == ur.User.UserId);
            if (r == null)
                return BadRequest(_localizer.GetString("You do not own restaurant with given name.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? oldName = r.Name;
                if (restaurantDto.NewName != null && restaurantDto.NewName != r.Name && !await _context.Restaurants.AnyAsync(e => e.Name == restaurantDto.NewName && e.RestaurantId != r.RestaurantId))
                {
                    r.Name = restaurantDto.NewName.Trim();
                }
                if (restaurantDto.Description != null && restaurantDto.Description != r.Description)
                    r.Description = restaurantDto.Description.Trim();

                if (restaurantDto.RestaurantCurrency != null && restaurantDto.RestaurantCurrency != r.Currency)
                {
                    if (!Enum.IsDefined(typeof(Currency), restaurantDto.RestaurantCurrency)) return BadRequest(_localizer.GetString("Given currency is not yet supported.").Value);
                    r.Currency = (Currency)restaurantDto.RestaurantCurrency;
                }
                if (restaurantDto.Cusines != null && restaurantDto.Cusines != r.Cusines)
                {
                    foreach (Cusine c in restaurantDto.Cusines)
                        if (!Enum.IsDefined(typeof(Cusine), c))
                            return BadRequest(_localizer.GetString("Given currency is not yet supported.").Value);

                    r.Cusines = restaurantDto.Cusines;
                }
                if (restaurantDto.OpenDays != null)
                {
                    List<OpeningHours> list = [];
                    foreach (OpeningHoursDto d in restaurantDto.OpenDays)
                    {
                        if (!Enum.TryParse(d.DayOfWeek, true, out DayOfWeek dayOfWeek))
                            return BadRequest(_localizer.GetString("Invalid closing time format.").Value);

                        Console.WriteLine("dayOfWeek.ToString() " + dayOfWeek.ToString());

                        if (!TimeOnly.TryParse(d.OpeningTime, culture, out TimeOnly openingTime))
                            return BadRequest(_localizer.GetString("Invalid opening time format.").Value);

                        if (!TimeOnly.TryParse(d.ClosingTime, culture, out TimeOnly closingTime))
                            return BadRequest(_localizer.GetString("Invalid closing time format.").Value);

                        list.Add(new OpeningHours
                        {
                            DayOfWeek = dayOfWeek,
                            OpeningTime = openingTime,
                            ClosingTime = closingTime
                        });
                    }
                    r.OpenDays = list;
                }
                if (restaurantDto.Lat != null && restaurantDto.Lon != null)
                {
                    var addresses = await Safety.GetLocalisedAddresses(restaurantDto.Lat, restaurantDto.Lon, _httpClient, _configuration);
                    if (addresses == null)
                        return BadRequest("Podano błędny adres."); //DODAĆ ODPOWIEDŹ

                    var oldAddresses = r.Addresses.ToList();
                    foreach (var address in oldAddresses)
                        _context.Addresses.Remove(address);
                    await _context.SaveChangesAsync();

                    r.Addresses = addresses;
                    r.Lon = restaurantDto.Lon.Replace(",", ".");
                    r.Lat = restaurantDto.Lat.Replace(",", ".");
                }
                await _context.SaveChangesAsync();

                //images adding
                if (restaurantDto.Files != null)
                {
                    string oldUploadPath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{oldName.Dehumanize()}\\";
                    foreach (var img in r.Images)
                    {
                        string path = Path.Combine(oldUploadPath, img.ImageSrc);
                        if (!r.Images.Remove(img)) throw new Exception("Error removing old images.");
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }
                    if(!Directory.EnumerateFiles(oldUploadPath).Any()) Directory.Delete(oldUploadPath);

                    string newUploadPath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{(restaurantDto.NewName ?? r.Name).Dehumanize()}\\";
                    if (!Directory.Exists(newUploadPath)) Directory.CreateDirectory(newUploadPath);
                    foreach (var f in restaurantDto.Files)
                    {
                        if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                            string filePath = Path.Combine(newUploadPath, fileName);

                            using (FileStream stream = new(filePath, FileMode.Create))
                                await f.CopyToAsync(stream);

                            r.Images.Add(new RestaurantImage { Restaurant = r, RestaurantId = r.RestaurantId, ImageSrc = fileName });
                        }
                        else
                            return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                    }
                    await _context.SaveChangesAsync();
                }
                else if (restaurantDto.Files == null && restaurantDto.NewName != null)
                    Directory.Move($"..\\MTM-Web-App.Server\\Img\\Restaurants\\{oldName.Dehumanize()}\\", $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{restaurantDto.NewName.Dehumanize()}\\");

                //logging
                await Safety.Log(ur.User, $"PUT restaurant", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Restaurant updated!").Value);
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

        /// <summary>Dodaje restaurację</summary>
        /// <param name="restaurantDto">Obiekt z danymi restauracji</param>
        /// <see cref="PostRestaurantDto"/>
        /// <seealso cref="Currency"/>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email/Błąd w <c>restaurantDto</c></response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/restaurants/add
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> AddRestaurant([FromForm] PostRestaurantDto restaurantDto)
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

            if (await _context.Restaurants.AnyAsync(e => e.Name.ToLower() == restaurantDto.Name.ToLower()))
                return BadRequest(_localizer.GetString("Given name is already taken.").Value);

            if (restaurantDto.Files == null || restaurantDto.Files.Count == 0)
                return BadRequest(_localizer.GetString("No images added.").Value);

            if (restaurantDto.OpenDays == null || restaurantDto.OpenDays.Count == 0 || restaurantDto.OpenDays.Count != restaurantDto.StartHours.Count || restaurantDto.OpenDays.Count != restaurantDto.EndHours.Count)
                return BadRequest(_localizer.GetString("InvalidOpenTimes").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<OpeningHours> list = [];
                for (int i = 0; i < restaurantDto.OpenDays.Count; i++)
                {
                    if (!Enum.TryParse(restaurantDto.OpenDays[i], true, out DayOfWeek dayOfWeek))
                        return BadRequest(_localizer.GetString("Invalid closing time format.").Value);

                    Console.WriteLine("dayOfWeek.ToString() " + dayOfWeek.ToString());

                    if (!TimeOnly.TryParse(restaurantDto.StartHours[i], culture, out TimeOnly openingTime))
                        return BadRequest(_localizer.GetString("Invalid opening time format.").Value);

                    if (!TimeOnly.TryParse(restaurantDto.EndHours[i], culture, out TimeOnly closingTime))
                        return BadRequest(_localizer.GetString("Invalid closing time format.").Value);

                    list.Add(new OpeningHours
                    {
                        DayOfWeek = dayOfWeek,
                        OpeningTime = openingTime,
                        ClosingTime = closingTime
                    });
                }
                var addresses = await Safety.GetLocalisedAddresses(restaurantDto.Lat, restaurantDto.Lon, _httpClient, _configuration);
                if (addresses == null)
                    return BadRequest(_localizer.GetString("Given address is invalid."));

                var restaurant = new Restaurant
                {
                    Name = restaurantDto.Name.Trim(),
                    Currency = restaurantDto.RestaurantCurrency,
                    OpenDays = list,
                    Cusines = restaurantDto.Cusines,
                    Addresses = addresses,
                    Lat = restaurantDto.Lat.Replace(",","."),
                    Lon = restaurantDto.Lon.Replace(",", "."),
                    Description = restaurantDto.Description,
                    UserId = ur.User.UserId,
                    OwnerUser = ur.User
                };
                await _context.Restaurants.AddAsync(restaurant);
                await _context.SaveChangesAsync();

                string uploadPath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{restaurant.Name.Dehumanize()}\\";

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                foreach (var f in restaurantDto.Files)
                {
                    if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);

                        using (FileStream stream = new(filePath, FileMode.Create))
                            await f.CopyToAsync(stream);

                        restaurant.Images.Add(new RestaurantImage { Restaurant = restaurant, RestaurantId = restaurant.RestaurantId, ImageSrc = fileName });
                    }
                    else
                        return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                }
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, $"POST restaurant {restaurant.Name}", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Restaurant added!").Value);
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

        /// <summary>Usuwa restaurację</summary>
        /// <param name="name" example="Pizza Hut">Nazwa restauracji</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji o podanej nazwie</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // DELETE: api/restaurants/name
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteRestaurant(string name)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var restaurant = await _context.Restaurants.Include(a=>a.Addresses).Include(i=>i.Images).Include(t=>t.Tables).ThenInclude(i=>i.Images).AsSplitQuery().FirstOrDefaultAsync(e => e.Name == name && e.UserId == ur.User.UserId);
            if (restaurant == null)
                return NotFound(_localizer.GetString("No restaurant with given name.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string deletePath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{restaurant.Name.Dehumanize()}\\";
                foreach (var img in restaurant.Images.ToList())
                    if (!restaurant.Images.Remove(img))
                        throw new Exception("Error removing old images.");

                foreach (var r in restaurant.Tables.ToList())
                    foreach (var img in r.Images.ToList())
                        if (!r.Images.Remove(img))
                            throw new Exception("Error removing old images.");

                Directory.Delete(deletePath, true);

                _context.Addresses.RemoveRange(restaurant.Addresses);
                await _context.SaveChangesAsync();
                _context.Tables.RemoveRange(restaurant.Tables);
                await _context.SaveChangesAsync();
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, $"DELETE restaurant", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Restaurant deleted").Value);
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

        /// <summary>Sprawdza czy podana nazwa restauracji jest wolna</summary>
        /// <param name="name" example="Pizza Hut">Nazwa restauracji</param>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik <c>true</c>/<c>false</c></response>
        // GET: api/restaurants/isNameFree/name
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("isNameFree/{name}")]
        public async Task<IActionResult> RestaurantExists(string name)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            //logging
            await Safety.Log(ur.User, "GET freeName", _context, _httpClient, HttpContext);

            return Ok(!await _context.Restaurants.AnyAsync(e => e.Name == name.Trim()));
        }
    }
}
