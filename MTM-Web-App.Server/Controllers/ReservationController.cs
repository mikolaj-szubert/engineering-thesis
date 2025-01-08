using iText.Barcodes;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;

using MTM_Web_App.Server.Models;
using Microsoft.Extensions.Localization;
using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using Microsoft.EntityFrameworkCore;
using iText.Layout;

namespace MTM_Web_App.Server.Controllers
{
    [Keyless]
    public class HotelResProcessed
    {
        public string? ReservationNumber { get; set; }
        public int FirstDigit { get; set; }
        public int SecondDigit { get; set; }
        public int ThirdDigit { get; set; }
        public int FourthDigit { get; set; }
    }
    [Keyless]
    public class RestaurantResProcessed
    {
        public string? ReservationNumber { get; set; }
        public int FirstDigit { get; set; }
        public int SecondDigit { get; set; }
        public int ThirdDigit { get; set; }
        public int FourthDigit { get; set; }
    }
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController(MTM_Web_AppServerContext context, IStringLocalizer<Resource> localizer, IEmailSender emailSender, HttpClient httpClient) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IStringLocalizer _localizer = localizer;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>Tworzy plik PDF</summary>
        /// <returns>Plik PDF</returns>
        /// <param name="reservationNum">Identyfikator rezerwacji</param>
        /// <see cref="HReservation"/>
        /// <response code="400">Niepoprawne dane wejściowe</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/reservations/hotel/pdf
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("hotel/pdf")]
        public async Task<IActionResult> GenerateHotelPDF([FromQuery] string reservationNum)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            HotelRes? hrr = await _context.HotelRes.Include(h=>h.Room).ThenInclude(h=>h.Hotel).Where(r => r.ReservationNumber == reservationNum && r.UserId == ur.User.UserId).FirstOrDefaultAsync();
            if (hrr == null) return BadRequest(); //DODAĆ ODPOWIEDŹ

            string pdfFileName = $"{_localizer.GetString("Reservation").Value}_{hrr.ReservationNumber}.pdf";
            byte[] pdf = GenerateHotelPDF(hrr, pdfFileName);
            
            Response.Headers.Append("Content-Disposition", $"inline; filename={pdfFileName}");
            return File(pdf, "application/pdf");
        }

        private string GenerateHotelResNum()
        {
            var t = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()[^3..];
            var (szósta, dziewiąta, ósma) = (t[0], t[1], t[2]);
            var (druga, czwarta) = (new Random().Next(0, 9).ToString()[0], new Random().Next(0, 9).ToString()[0]);

            char pierwsza, trzecia, piąta, siódma;
            IQueryable<int> values = _context.HotelResProcessed.Select(x => x.FirstDigit * 1000 + x.SecondDigit * 100 + x.ThirdDigit * 10 + x.FourthDigit);
            if (values.Any())
            {
                int minValue = values.Min();
                int maxValue = values.Max();
                if (minValue == 0)
                {
                    (pierwsza, trzecia, piąta, siódma) = ((minValue + 1).ToString("D4")[0], (minValue + 1).ToString("D4")[1], (minValue + 1).ToString("D4")[2], (minValue + 1).ToString("D4")[3]);
                }
                else (pierwsza, trzecia, piąta, siódma) = ((minValue - 1).ToString("D4")[0], (minValue - 1).ToString("D4")[1], (minValue - 1).ToString("D4")[2], (minValue - 1).ToString("D4")[3]);
            }
            else (pierwsza, trzecia, piąta, siódma) = ('0', '0', '0', '0');

            return "" + pierwsza + druga + trzecia + czwarta + piąta + szósta + siódma + ósma + dziewiąta;
        }

        /// <summary>Tworzy rezerwację hotelu</summary>
        /// <returns>Nic</returns>
        /// <param name="hr">Obiekt z danymi rezerwacji, którą chcemy dodać</param>
        /// <see cref="HReservation"/>
        /// <response code="400">Niepoprawne dane wejściowe</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/user/make-hotel-reservation
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("hotel")]
        public async Task<IActionResult> PostHotelRes(HReservation hr)
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

            if (!DateOnly.TryParse(hr.EndDate, culture, out DateOnly end) || !DateOnly.TryParse(hr.StartDate, culture, out DateOnly start) || end < start)
                return BadRequest(_localizer.GetString("Inavalid date.").Value);

            int stayLength = (end.Day - start.Day) + 1;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Room? r = await _context.Rooms.Include(h=>h.Hotel).Include(r => r.Rooms).ThenInclude(r => r.Availabilities).FirstOrDefaultAsync(e => e.Name == hr.RoomName && e.Hotel.Name == hr.HotelName);
                if (r == null)
                    return NotFound(_localizer.GetString("Specified room does not exist.").Value);
                List<RoomEntity> roomEntities = [];
                //sprawdza czy wszystkie pokoje danego typu w podanym przedziale czasowym są dostępne
                foreach (RoomEntity? roomItem in r.Rooms.AsQueryable())
                {
                    //wszystkie daty w podanym zakresie nie mają elementów w tabeli = można rezerwować
                    bool x = !roomItem.Availabilities.Any(ra => ra.RoomId == r.RoomId && ra.Date >= start && ra.Date <= end);

                    int y = roomItem.Availabilities.Where(ra => ra.RoomId == r.RoomId && ra.Date >= start && ra.Date <= end && !ra.IsReserved).Count();

                    //wszystkie wyniki w podanym zakresie nie są zarezerwowane = można rezerwować
                    bool z = y == stayLength;

                    //suma pustych dat w bazie i anulowanych rezerwacji jest równa liczbie rezerwowanych dni = można rezerwować
                    bool l = false;
                    if (y > 0 && y < stayLength)
                    {
                        int emptyDates = Enumerable.Range(0, stayLength).Count(offset => {
                            DateOnly date = start.AddDays(offset);
                            return !roomItem.Availabilities.Any(ra => ra.RoomId == r.RoomId && ra.Date == date);
                        });
                        l = emptyDates + y == stayLength;
                    }

                    if (x || z || l)
                        roomEntities.Add(roomItem);
                }

                //liczba pokoi jest mniejsza niz wymagana do rezerwacji
                if (roomEntities.Count == 0)
                    return BadRequest("NIEWYSTARCZAJĄCA LICZBA POKOI DO WYKONANIA REZERWACJI"); //DODAĆ ODPOWIEDŹ - BRAK POKOI W TERMINIE

                RoomEntity? status = roomEntities.FirstOrDefault() ?? throw new Exception("Pomimo dostępności pokojów tego typu, nie można zarezerwować jednego z nich.");
                int days = 0;
                for (DateOnly x = start; x <= end; x = x.AddDays(1))
                {
                    RoomAvailability? res = status.Availabilities.FirstOrDefault(y => y.Date == x);
                    if (res == null) status.Availabilities.Add(new() { Date = x, RoomId = r.RoomId, Room = r, IsReserved = true });
                    else res.IsReserved = true;
                    days++;
                }
                await _context.SaveChangesAsync();

                string resNum = GenerateHotelResNum();

                var hrr = new HotelRes
                {
                    ReservationNumber = resNum,
                    ReservationVerification = new Random().Next(0, 99999).ToString("D5"),
                    StartDate = start.ToDateTime(r.Hotel.CheckIn),
                    EndDate = end.ToDateTime(r.Hotel.CheckOut),
                    SummaryCost = r.Price*days,
                    Room = r,
                    RoomId = r.RoomId,
                    UserId = ur.User.UserId,
                    ClientUser = ur.User,
                    Notes = hr.Notes,
                };
                await _context.HotelRes.AddAsync(hrr);
                await _context.SaveChangesAsync();

                await _context.Entry(hrr).Reference(h => h.Room).LoadAsync();

                var date = hrr.StartDate.ToShortDateString().ToString(culture);
                string body = $@"<!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>{_localizer.GetString("Reservation Details").Value}</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                color: #333;
                                margin: 0;
                                padding: 0;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 20px auto;
                                background-color: #ffffff;
                                border-radius: 8px;
                                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                overflow: hidden;
                            }}
                            .header {{
                                background-color: #4CAF50;
                                padding: 20px;
                                text-align: center;
                                color: white;
                            }}
                            .header h1 {{
                                margin: 0;
                                font-size: 24px;
                            }}
                            .content {{
                                padding: 20px;
                            }}
                            .reservation-details {{
                                margin: 20px 0;
                                padding: 10px;
                                background-color: #f9f9f9;
                                border: 1px solid #e0e0e0;
                                border-radius: 5px;
                            }}
                            .footer {{
                                text-align: center;
                                padding: 20px;
                                background-color: #f4f4f4;
                                color: #999;
                            }}
                        </style>
                    </head>
                    <body>

                    <div class=""container"">
                        <div class=""header"">
                            <h1>{_localizer.GetString("Reservation Details").Value}</h1>
                        </div>
                        <div class=""content"">
                            <p>{_localizer.GetString("HelloUser", ur.User.Name)},</p>
                            <p>{_localizer.GetString("Thank you for your reservation at").Value} <strong>{hrr.Room.Hotel.Name}</strong>!</p>
                            <p>{_localizer.GetString("Your reservation details are as follows:").Value}</p>
                            <div class=""reservation-details"">
                                <p><strong>{_localizer.GetString("Reservation number:").Value}</strong> {hrr.ReservationNumber}</p>
                                <p><strong>{_localizer.GetString("Check in").Value}:</strong> {hrr.StartDate.ToString("g", CultureInfo.CurrentCulture)}</p>
                                <p><strong>{_localizer.GetString("Check out").Value}:</strong> {hrr.EndDate.ToString("g", CultureInfo.CurrentCulture)}</p>
                                <p><strong>{_localizer.GetString("Price").Value}:</strong> {hrr.SummaryCost} {r.Hotel.Currency}</p>
                            </div>
                            <p>{_localizer.GetString("If you have any questions or need to make changes to your reservation, please do not hesitate to contact us.").Value}</p>
                            <p>{_localizer.GetString("We look forward to welcoming you!").Value}</p>
                            <p>{localizer.GetString("BestRegards").Value}</p>
                        </div>
                        <div class=""footer"">
                            <p>{localizer.GetString("FooterMessage").Value}</p>
                        </div>
                    </div>

                    </body>
                    </html>";

                string pdfFileName = $"{_localizer.GetString("Reservation").Value}_{hrr.ReservationNumber}.pdf";
                byte[] pdf = GenerateHotelPDF(hrr, pdfFileName);

                _emailSender.SendEmail(ur.User.Email, _localizer.GetString("Reservation Details").Value, body, pdf, pdfFileName);

                //logging
                await Safety.Log(ur.User, "POST hotelRes", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(new { Number = hrr.ReservationNumber });
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

        /// <summary>Zwraca wszystkie rezerwacje hoteli użytkownika</summary>
        /// <returns>Rezerwacje hoteli</returns>
        /// <response code="404">Brak rezerwacji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Rezerwacje hoteli</response>
        // GET: api/user/make-hotel-reservation
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<HotelRes>>(StatusCodes.Status200OK)]
        [HttpGet("hotel")]
        public async Task<IActionResult> GetHotelRes()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

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

            var hr = await _context.HotelRes.Include(r=>r.Room).ThenInclude(h=>h.Hotel).Where(r => r.UserId == ur.User.UserId).Select(r => new
            {
                r.ReservationNumber,
                From = r.StartDate.ToShortDateString().ToString(culture),
                To = r.EndDate.ToShortDateString().ToString(culture),
                HotelName = r.Room.Hotel.Name,
                r.SummaryCost,
                Room = r.Room.Name,
                r.Room.Hotel.Currency
            }).ToListAsync();
            if (hr == null || hr.Count == 0)
                return NotFound(_localizer.GetString("No reservations.").Value);

            //logging
            await Safety.Log(ur.User, "GET hotelRes", _context, _httpClient, HttpContext);

            return Ok(hr);
        }

        /// <summary>Usuwa rezerwacje hotelu użytkownika</summary>
        /// <returns>Status</returns>
        /// <param name="name">Numer rezerwacji</param>
        /// <response code="404">Brak rezerwacji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Rezerwacje restauracji</response>
        // GET: api/user/make-hotel-reservation
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<RestaurantRes>>(StatusCodes.Status200OK)]
        [HttpDelete("hotel/{name}")]
        public async Task<IActionResult> DeleteHotelRes(string name)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var rr = await _context.HotelRes.Include(t => t.Room).ThenInclude(a => a.Rooms).Include(t => t.Room).ThenInclude(r => r.Hotel).Where(r => r.UserId == ur.User.UserId || r.Room.Hotel.UserId == ur.User.UserId && r.ReservationNumber == name).FirstOrDefaultAsync();
            if (rr == null)
                return NotFound(_localizer.GetString("No reservations.").Value);

            _context.HotelRes.Remove(rr);
            await _context.SaveChangesAsync();

            //logging
            await Safety.Log(ur.User, "DELETE hotelRes", _context, _httpClient, HttpContext);

            return Ok();
        }

        private byte[] GenerateHotelPDF(HotelRes hrr, string title)
        {
            if (hrr == null)
                throw new ArgumentNullException(nameof(hrr), "Reservation data is null.");

            if (hrr.Room.Hotel == null)
                throw new InvalidOperationException("Hotel data is null in reservation.");

            if (hrr.ClientUser == null)
                throw new InvalidOperationException("Client user data is null in reservation.");

            //creating pdf with qr code
            using var stream = new MemoryStream();
            using (var pdfWriter = new PdfWriter(stream))
            {
                using var pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.GetDocumentInfo().SetTitle(title);
                pdfDocument.GetDocumentInfo().SetSubject(title);
                pdfDocument.GetDocumentInfo().RemoveCreationDate();
                pdfDocument.GetDocumentInfo().SetAuthor("MTM Project");
                pdfDocument.GetDocumentInfo().SetCreator("MTM Project");
                pdfDocument.GetDocumentInfo().SetProducer("MTM Project");
                var document = new Document(pdfDocument);

                PdfFont font = PdfFontFactory.CreateFont(@"..\MTM-Web-App.Server\Resources\Poppins\Poppins-Regular.ttf", PdfEncodings.IDENTITY_H);

                document.SetFont(font);
                document
                    .Add(new Paragraph($"{_localizer.GetString("Your reservation")}{hrr.Room.Hotel.Name}")
                    .SetFontSize(30)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER));

                var barcodeQRCode = new BarcodeQRCode($"H|{hrr.ReservationNumber}");
                var qrCodeObject = barcodeQRCode.CreateFormXObject(pdfDocument);
                var qrImage = new Image(qrCodeObject)
                    .SetPadding(0)
                    .SetMargins(0, 0, 0, 0)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetWidth(UnitValue.CreatePercentValue(60));

                document.Add(qrImage);

                document
                    .Add(new Paragraph($"{_localizer.GetString("Reservation number:")} {hrr.ReservationNumber}")
                    .SetFontSize(15)
                    .SetTextAlignment(TextAlignment.CENTER));

                iText.Layout.Element.Table table = new(UnitValue.CreatePercentArray([1, 2]));
                table.SetWidth(UnitValue.CreatePercentValue(100));

                int height = 25, padding = 5;

                Cell headerCell1 = new Cell()
                    .Add(new Paragraph(_localizer.GetString("Parameter")).SetBold())
                    .SetBackgroundColor(ColorConstants.BLACK)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height);

                Cell headerCell2 = new Cell()
                    .Add(new Paragraph(_localizer.GetString("Value"))
                    .SetBold())
                    .SetBackgroundColor(ColorConstants.BLACK)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height);

                table.AddHeaderCell(headerCell1);
                table.AddHeaderCell(headerCell2);

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Name")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(hrr.ClientUser.Name))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Check in")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(hrr.StartDate.ToString("g", CultureInfo.CurrentCulture)))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Check out")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(hrr.EndDate.ToString("g", CultureInfo.CurrentCulture)))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Price")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph($"{hrr.SummaryCost} {hrr.Room.Hotel.Currency}"))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                document.Add(table);

                document.Close();
            }

            return stream.ToArray();
        }

        /// <summary>Tworzy plik PDF</summary>
        /// <returns>Plik PDF</returns>
        /// <param name="reservationNum">Identyfikator rezerwacji</param>
        /// <see cref="HReservation"/>
        /// <response code="400">Niepoprawne dane wejściowe</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/reservations/hotel/pdf
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("restaurant/pdf")]
        public async Task<IActionResult> GenerateRestaurantPDF([FromQuery] string reservationNum)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            RestaurantRes? rrr = await _context.RestaurantsRes.Include(h => h.Table).ThenInclude(r => r.Restaurant).Where(r => r.ReservationNumber == reservationNum && r.UserId == ur.User.UserId).FirstOrDefaultAsync();
            if (rrr == null) return BadRequest(); //DODAĆ ODPOWIEDŹ

            string pdfFileName = $"{_localizer.GetString("Reservation").Value}_{rrr.ReservationNumber}.pdf";
            byte[] pdf = GenerateRestaurantPDF(rrr, pdfFileName);

            Response.Headers.Append("Content-Disposition", $"inline; filename={pdfFileName}");
            return File(pdf, "application/pdf");
        }

        private string GenerateRestaurantResNum()
        {
            var t = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()[^3..];
            var (szósta, dziewiąta, ósma) = (t[0], t[1], t[2]);
            var (druga, czwarta) = (new Random().Next(0, 9).ToString()[0], new Random().Next(0, 9).ToString()[0]);

            char pierwsza, trzecia, piąta, siódma;
            IQueryable<int> values = _context.RestaurantResProcessed.Select(x => x.FirstDigit * 1000 + x.SecondDigit * 100 + x.ThirdDigit * 10 + x.FourthDigit);
            if (values.Any())
            {
                int minValue = values.Min();
                int maxValue = values.Max();
                if (minValue == 0)
                {
                    (pierwsza, trzecia, piąta, siódma) = ((minValue + 1).ToString("D4")[0], (minValue + 1).ToString("D4")[1], (minValue + 1).ToString("D4")[2], (minValue + 1).ToString("D4")[3]);
                }
                else (pierwsza, trzecia, piąta, siódma) = ((minValue - 1).ToString("D4")[0], (minValue - 1).ToString("D4")[1], (minValue - 1).ToString("D4")[2], (minValue - 1).ToString("D4")[3]);
            }
            else (pierwsza, trzecia, piąta, siódma) = ('0', '0', '0', '0');

            return "" + pierwsza + druga + trzecia + czwarta + piąta + szósta + siódma + ósma + dziewiąta;
        }

        /// <summary>Tworzy rezerwację restauracji</summary>
        /// <returns>Nic</returns>
        /// <param name="rr">Obiekt z danymi rezerwacji, którą chcemy dodać</param>
        /// <see cref="RReservation"/>
        /// <response code="400">Niepoprawne dane wejściowe</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/user/make-restaurant-reservation
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("restaurant")]
        public async Task<IActionResult> PostRestaurantRes(RReservation rr)
        {
            //auth
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

            if (!DateOnly.TryParse(rr.Date, culture, out DateOnly dtoDate))
                return BadRequest(_localizer.GetString("Inavalid date.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Models.Table? t = await _context.Tables.Include(r=>r.Restaurant).Include(r => r.Tables).FirstOrDefaultAsync(e => e.Name == rr.TableName && e.Restaurant.Name == rr.RestaurantName);
                if (t == null)
                    return NotFound(_localizer.GetString("Specified room does not exist.").Value);

                List<TableEntity> tableEntities = [];
                //sprawdza czy wszystkie pokoje danego typu w podanym przedziale czasowym są dostępne
                foreach (TableEntity? tableItem in t.Tables.AsQueryable())
                    if (!tableItem.Availabilities.Any(ra => ra.TableId == t.TableId && ra.Date == dtoDate) || tableItem.Availabilities.Any(ra => ra.TableId == t.TableId && ra.Date == dtoDate && !ra.IsReserved))
                        tableEntities.Add(tableItem);

                //liczba pokoi jest mniejsza niz wymagana do rezerwacji
                if (tableEntities.Count == 0)
                    return BadRequest("NIEWYSTARCZAJĄCA LICZBA POKOI DO WYKONANIA REZERWACJI"); //DODAĆ ODPOWIEDŹ - BRAK POKOI W TERMINIE

                TableEntity? status = tableEntities.FirstOrDefault() ?? throw new Exception("Pomimo dostępności pokojów tego typu, nie można zarezerwować jednego z nich.");

                TableAvailability? res = status.Availabilities.FirstOrDefault(y => y.Date == dtoDate);
                if (res == null) status.Availabilities.Add(new() { Date = dtoDate, TableId = t.TableId, Table = t, IsReserved = true });
                else res.IsReserved = true;

                await _context.SaveChangesAsync();

                string resNum = GenerateRestaurantResNum();

                var hrr = new RestaurantRes
                {
                    ReservationNumber = resNum,
                    ReservationVerification = new Random().Next(0, 99999).ToString("D5"),
                    Date = dtoDate,
                    SummaryCost = t.Price,
                    TableName = status.Table.Name,
                    Table = t,
                    TableId = t.TableId,
                    UserId = ur.User.UserId,
                    ClientUser = ur.User,
                    Notes = rr.Notes,
                };
                await _context.RestaurantsRes.AddAsync(hrr);
                await _context.SaveChangesAsync();

                await _context.Entry(hrr).Reference(h => h.Table).LoadAsync();

                var date = hrr.Date.ToShortDateString().ToString(culture);
                string body = $@"<!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>{_localizer.GetString("Reservation Details").Value}</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            color: #333;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 8px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }}
                        .header {{
                            background-color: #4CAF50;
                            padding: 20px;
                            text-align: center;
                            color: white;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 24px;
                        }}
                        .content {{
                            padding: 20px;
                        }}
                        .reservation-details {{
                            margin: 20px 0;
                            padding: 10px;
                            background-color: #f9f9f9;
                            border: 1px solid #e0e0e0;
                            border-radius: 5px;
                        }}
                        .footer {{
                            text-align: center;
                            padding: 20px;
                            background-color: #f4f4f4;
                            color: #999;
                        }}
                    </style>
                </head>
                <body>

                <div class=""container"">
                    <div class=""header"">
                        <h1>{_localizer.GetString("Reservation Details").Value}</h1>
                    </div>
                    <div class=""content"">
                        <p>{_localizer.GetString("HelloUser", ur.User.Name)},</p>
                        <p>{_localizer.GetString("Thank you for your reservation at").Value} <strong>{hrr.Table.Restaurant.Name}</strong>!</p>
                        <p>{_localizer.GetString("Your reservation details are as follows:").Value}</p>
                        <div class=""reservation-details"">
                            <p><strong>{_localizer.GetString("Reservation number:").Value}</strong> {hrr.ReservationNumber}</p>
                            <p><strong>{_localizer.GetString("Date").Value}:</strong> {hrr.Date.ToString("d", CultureInfo.CurrentCulture)}</p>
                            <p><strong>{_localizer.GetString("Price").Value}:</strong> {hrr.SummaryCost} {hrr.Table.Restaurant.Currency}</p>
                        </div>
                        <p>{_localizer.GetString("If you have any questions or need to make changes to your reservation, please do not hesitate to contact us.").Value}</p>
                        <p>{_localizer.GetString("We look forward to welcoming you!").Value}</p>
                        <p>{localizer.GetString("BestRegards").Value}</p>
                    </div>
                    <div class=""footer"">
                        <p>{localizer.GetString("FooterMessage").Value}</p>
                    </div>
                </div>

                </body>
                </html>";

                string pdfFileName = $"{_localizer.GetString("Reservation").Value}_{hrr.ReservationNumber}.pdf";
                byte[] pdf = GenerateRestaurantPDF(hrr, pdfFileName);

                _emailSender.SendEmail(ur.User.Email, _localizer.GetString("Reservation Details").Value, body, pdf, pdfFileName);

                //logging
                await Safety.Log(ur.User, "POST restaurantRes", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(new { Number = hrr.ReservationNumber });
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

        /// <summary>Zwraca wszystkie rezerwacje restauracji użytkownika</summary>
        /// <returns>Rezerwacje restauracji</returns>
        /// <response code="404">Brak rezerwacji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Rezerwacje restauracji</response>
        // GET: api/user/make-hotel-reservation
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<RestaurantRes>>(StatusCodes.Status200OK)]
        [HttpGet("restaurant")]
        public async Task<IActionResult> GetRestaurantRes()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var rr = await _context.RestaurantsRes.Include(t=>t.Table).ThenInclude(a=>a.Tables).Where(r => r.UserId == ur.User.UserId).Select(r => new
            {
                r.ReservationNumber,
                Date = r.Date.ToShortDateString(),
                RestaurantName = r.Table.Restaurant.Name,
                r.SummaryCost,
                r.Table.Restaurant.Currency
            }).ToListAsync();
            if (rr == null || rr.Count == 0)
                return NotFound(_localizer.GetString("No reservations.").Value);

            //logging
            await Safety.Log(ur.User, "GET restaurantRes", _context, _httpClient, HttpContext);

            return Ok(rr);
        }

        /// <summary>Usuwa rezerwacje restauracji użytkownika</summary>
        /// <returns>Status</returns>
        /// <param name="name">Numer rezerwacji</param>
        /// <response code="404">Brak rezerwacji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Rezerwacje restauracji</response>
        // GET: api/user/make-hotel-reservation
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<IEnumerable<RestaurantRes>>(StatusCodes.Status200OK)]
        [HttpDelete("restaurant/{name}")]
        public async Task<IActionResult> DeleteRestaurantRes(string name)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var rr = await _context.RestaurantsRes.Include(t => t.Table).ThenInclude(a => a.Tables).Include(t => t.Table).ThenInclude(r => r.Restaurant).Where(r => r.UserId == ur.User.UserId || r.Table.Restaurant.UserId == ur.User.UserId && r.ReservationNumber == name).FirstOrDefaultAsync();
            if (rr == null)
                return NotFound(_localizer.GetString("No reservations.").Value);

            _context.RestaurantsRes.Remove(rr);
            await _context.SaveChangesAsync();

            //logging
            await Safety.Log(ur.User, "DELETE restaurantRes", _context, _httpClient, HttpContext);

            return Ok();
        }

        private byte[] GenerateRestaurantPDF(RestaurantRes rr, string title)
        {
            //creating pdf with qr code
            using var stream = new MemoryStream();
            using (var pdfWriter = new PdfWriter(stream))
            {
                using var pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.GetDocumentInfo().SetTitle(title);
                pdfDocument.GetDocumentInfo().SetSubject(title);
                pdfDocument.GetDocumentInfo().RemoveCreationDate();
                pdfDocument.GetDocumentInfo().SetAuthor("MTM Project");
                pdfDocument.GetDocumentInfo().SetCreator("MTM Project");
                pdfDocument.GetDocumentInfo().SetProducer("MTM Project");
                var document = new Document(pdfDocument);

                PdfFont font = PdfFontFactory.CreateFont(@"..\MTM-Web-App.Server\Resources\Poppins\Poppins-Regular.ttf", PdfEncodings.IDENTITY_H);

                document.SetFont(font);
                document
                    .Add(new Paragraph($"{_localizer.GetString("Your reservation")}{rr.Table.Restaurant.Name}")
                    .SetFontSize(30)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER));

                var barcodeQRCode = new BarcodeQRCode($"R|{rr.ReservationNumber}");

                var qrCodeObject = barcodeQRCode.CreateFormXObject(pdfDocument);
                var qrImage = new Image(qrCodeObject)
                    .SetPadding(0)
                    .SetMargins(0, 0, 0, 0)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetWidth(UnitValue.CreatePercentValue(60));
                document.Add(qrImage);

                document
                    .Add(new Paragraph($"{_localizer.GetString("Reservation number:")} {rr.ReservationNumber}")
                    .SetFontSize(15)
                    .SetTextAlignment(TextAlignment.CENTER));

                iText.Layout.Element.Table table = new(UnitValue.CreatePercentArray([1, 2]));
                table.SetWidth(UnitValue.CreatePercentValue(100));

                int height = 25, padding = 5;

                Cell headerCell1 = new Cell()
                    .Add(new Paragraph(_localizer.GetString("Parameter")).SetBold())
                    .SetBackgroundColor(ColorConstants.BLACK)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height);

                Cell headerCell2 = new Cell()
                    .Add(new Paragraph(_localizer.GetString("Value"))
                    .SetBold())
                    .SetBackgroundColor(ColorConstants.BLACK)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height);

                table.AddHeaderCell(headerCell1);
                table.AddHeaderCell(headerCell2);

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Name")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(rr.ClientUser.Name))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Date")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(rr.Date.ToString("d", CultureInfo.CurrentCulture)))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                table.AddCell(
                    new Cell()
                    .Add(new Paragraph(_localizer.GetString("Price")))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );
                table.AddCell(
                    new Cell()
                    .Add(new Paragraph($"{rr.SummaryCost} {rr.Table.Restaurant.Currency}"))
                    .SetPadding(0)
                    .SetPaddingLeft(padding)
                    .SetHeight(height)
                );

                document.Add(table);

                document.Close();
            }

            return stream.ToArray();
        }

        [HttpGet("owned/hotel")]
        public async Task<IActionResult> GetMyHotelRes()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;
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
            var reservations = await _context.HotelRes
                .AsSplitQuery()
                .Include(r => r.Room)
                    .ThenInclude(h => h.Hotel)
                        .ThenInclude(u => u.OwnerUser)
                .Where(r => r.Room.Hotel.OwnerUser == ur.User)
                .Select(r => new {
                    hotelName = r.Room.Hotel.Name,
                    r.ReservationNumber, 
                    r.Room.Name,
                    StartDate = r.StartDate.ToShortDateString().ToString(culture),
                    EndDate = r.EndDate.ToShortDateString().ToString(culture), 
                    r.Notes 
                }).ToListAsync();

            return Ok(reservations.Count != 0 ? reservations : null);
        }

        [HttpGet("owned/restaurant")]
        public async Task<IActionResult> GetMyRestaurantRes()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var reservations = await _context.RestaurantsRes
                .AsSplitQuery()
                .Include(r => r.Table)
                    .ThenInclude(h => h.Restaurant)
                        .ThenInclude(u => u.OwnerUser)
                .Where(r => r.Table.Restaurant.OwnerUser == ur.User)
                .Select(r => new { 
                    restaurantName = r.Table.Restaurant.Name,
                    r.ReservationNumber, 
                    r.Table.Name, 
                    Date = r.Date.ToShortDateString(), 
                    r.Notes
                }).ToListAsync();

            return Ok(reservations.Count != 0 ? reservations : null);
        }
    }
}
