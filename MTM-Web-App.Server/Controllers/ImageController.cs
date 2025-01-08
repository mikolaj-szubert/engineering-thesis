using Humanizer;

using Microsoft.AspNetCore.Mvc;

using MTM_Web_App.Server.Helpers;

using MTM_Web_App.Server.Models;
using Microsoft.Extensions.Localization;
using MTM_Web_App.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController(MTM_Web_AppServerContext context, HttpClient httpClient, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IStringLocalizer _localizer = localizer;

        /// <summary>Zwraca zdjęcie profilowe użytkownika</summary>
        /// <returns>Zdjęcie</returns>
        /// <response code="400">Błąd zapytania</response>
        /// <response code="404">Brak hotelu</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/images/user
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserImage()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            if (ur.User.PfpSrc == null)
                return NotFound(_localizer.GetString("NoPfp").Value);

            if (ur.User.IsGooglePfp == true)
                return BadRequest(_localizer.GetString("GooglePfp").Value);

            string source = ur.User.PfpSrc;
            string basePath = "..\\MTM-Web-App.Server\\Img\\Users\\";
            string path = Path.Combine(basePath, source);
            if (!System.IO.File.Exists(path))
                return NotFound(_localizer.GetString("NoPfp").Value);

            byte[] b = System.IO.File.ReadAllBytes(path);
            return Path.GetExtension(source) == "png" ? File(b, "image/png") : File(b, "image/jpg");
        }

        /// <summary>Dodaje zdjęcie profilowe</summary>
        /// <param name="file">Zdjęcie z formularza</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak użytkownika</response>
        /// <response code="401">Brak dostępu - niepoprawny/brak tokenu</response>
        /// <response code="400">Niezweryfikowany email/Błąd zapytania</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/images/user
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("user")]
        public async Task<IActionResult> UploadUserImage(IFormFile? file)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            //files check
            if (file == null)
                return BadRequest(_localizer.GetString("NoPfpAdded").Value);

            string uploadPath = "..\\MTM-Web-App.Server\\Img\\Users\\";
            if (Path.GetExtension(file.FileName) != ".png" && Path.GetExtension(file.FileName) != ".jpg" && Path.GetExtension(file.FileName) != ".jpeg")
                return BadRequest(_localizer.GetString("PfpInvalidExtension").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                if (ur.User.PfpSrc != null && System.IO.File.Exists(Path.Combine(uploadPath, ur.User.PfpSrc))) 
                    System.IO.File.Delete(Path.Combine(uploadPath, ur.User.PfpSrc));

                ur.User.PfpSrc = fileName;
                ur.User.IsGooglePfp = false;
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, $"POST userImage", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("ImageAdded").Value);
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

        /// <summary>Usuwa zdjęcie</summary>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak zdjęcia</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // DELETE: api/images/user
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [HttpDelete("user")]
        public async Task<IActionResult> DeleteUserImage()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            if (ur.User.PfpSrc == null)
                return NotFound(_localizer.GetString("You do not have profile picture.").Value);

            string dir = $"..\\MTM-Web-App.Server\\Img\\Users\\";
            string path = Path.Combine(dir, ur.User.PfpSrc);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    if (!Directory.EnumerateFiles(dir).Any())
                        Directory.Delete(dir);
                }

                ur.User.PfpSrc = null;
                ur.User.IsGooglePfp = false;
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, $"DELETE userImage", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Image Deleted!").Value);
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

        /// <summary>Zwraca zdjęcie hotelu</summary>
        /// <returns>Zdjęcie</returns>
        /// <param name="hotelName" example="California Haze">Nazwa hotelu</param>
        /// <param name="name" example="1b304de9-6c61-4e20-b63a-53d3e1600a3f.jpg">Nazwa pliku ze zdjęciem</param>
        /// <response code="400">Błąd zapytania</response>
        /// <response code="404">Brak hotelu</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/images/hotel/hotelName/roomName/name
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("hotel/{hotelName}/{name}")]
        public IActionResult GetHotelImages(string hotelName, string name)
        {
            hotelName = hotelName.Trim();
            name = name.Trim();
            string path = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{hotelName.Dehumanize()}\\{name}";

            if (!System.IO.File.Exists(path))
                return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);

            if (!name.EndsWith(".png") && !name.EndsWith(".jpg") && !name.EndsWith(".jpeg"))
                return BadRequest(_localizer.GetString("Invalid file extension.").Value);

            byte[] b = System.IO.File.ReadAllBytes(path);
            return File(b, $"image/{name.Split(".")[^1]}");
        }

        /// <summary>Zwraca zdjęcie restauracji</summary>
        /// <returns>Zdjęcie</returns>
        /// <param name="restaurantName" example="Pizza Hut">Nazwa restauracji</param>
        /// <param name="name" example="1b304de9-6c61-4e20-b63a-53d3e1600a3f.jpg">Nazwa pliku ze zdjęciem</param>
        /// <response code="400">Błąd zapytania</response>
        /// <response code="404">Brak restauracji</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/images/restaurant/restaurantName/tableName/name
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("restaurant/{restaurantName}/{name}")]
        public IActionResult GetRestaurantImages(string restaurantName, string name)
        {
            restaurantName = restaurantName.Trim();
            name = name.Trim();
            string path = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{restaurantName.Dehumanize()}\\{name}";

            if (!System.IO.File.Exists(path))
                return NotFound(_localizer.GetString("File does not exist.").Value);

            if (!name.EndsWith(".png") && !name.EndsWith(".jpg") && !name.EndsWith(".jpeg"))
                return BadRequest(_localizer.GetString("Invalid image extension. Accepted formats are .png and .jpg.").Value);

            byte[] b = System.IO.File.ReadAllBytes(path);
            return File(b, $"image/{name.Split(".")[^1]}");
        }
    }
}
