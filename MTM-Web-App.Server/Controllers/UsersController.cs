using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController(MTM_Web_AppServerContext context, HttpClient httpClient, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IStringLocalizer _localizer = localizer;

        /// <summary>Zwraca wszystkie logi użytkownika</summary>
        /// <returns>Logi</returns>
        /// <response code="404">Brak logów</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // GET: api/user/logs
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("manage/logs")]
        public async Task<IActionResult> GetSignins()
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

            var logs = await _context.Logger.Where(p => p.User == ur.User).Select(p => new { p.Address, Time = p.LogTime.ToString(culture), p.Location, p.LogName }).ToListAsync();
            if (logs.Count == 0)
                return NotFound(); //DODAĆ ODPOWIEDŹ

            //logging
            await Safety.Log(ur.User, "GET logs", _context, _httpClient, HttpContext);
            logs.Reverse();
            return Ok(logs.GetRange(0, logs.Count > 10 ? 10 : logs.Count));
        }

        /// <summary>Aktualizuje dane w bazie danych</summary>
        /// <returns>Nic</returns>
        /// <param name="updatedUser">Obiekt z danymi, które chcemy zaktualizować</param>
        /// <see cref="UpdateUserDto"/>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // PUT: api/user/updateUser
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPut("manage/update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updatedUser)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            //update data
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //logging
                await Safety.Log(ur.User, "PUT user", _context, _httpClient, HttpContext);

                if (updatedUser.Email != null && ur.User.Email != updatedUser.Email.Trim() && !string.IsNullOrEmpty(updatedUser.Email.Trim()))
                    ur.User.Email = updatedUser.Email.Trim();

                if (updatedUser.Name != null && ur.User.Name != updatedUser.Name.Trim())
                    ur.User.Name = updatedUser.Name.Trim();

                if (updatedUser.Password != null)
                {
                    ur.User.PasswordHash = Safety.HashPassword(updatedUser.Password.Trim(), out byte[] salt);
                    ur.User.Salt = salt;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(_localizer.GetString("User Updated.").Value);
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

        /// <summary>Usuwa użytkownika</summary>
        /// <returns>Nic</returns>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="404">Brak restauracji</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="200">Token dostępu</response>
        // DELETE: api/user/deleteUser
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpDelete("manage/delete")]
        public async Task<IActionResult> DeleteUser()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer, false);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            //delete user
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.HotelRes.RemoveRange(_context.HotelRes.Where(r => r.UserId == ur.User.UserId));
                await _context.SaveChangesAsync();

                _context.RestaurantsRes.RemoveRange(_context.RestaurantsRes.Where(r => r.UserId == ur.User.UserId));
                await _context.SaveChangesAsync();

                _context.Logger.RemoveRange(_context.Logger.Where(s => s.UserId == ur.User.UserId));
                await _context.SaveChangesAsync();

                var h = _context.Hotels.Where(u => u.UserId == ur.User.UserId);
                var r = _context.Restaurants.Where(u => u.UserId == ur.User.UserId);

                _context.Addresses.RemoveRange(_context.Addresses.Include(h => h.Hotel).Include(r => r.Restaurant).Where(a => h.Contains(a.Hotel) || r.Contains(a.Restaurant)));
                await _context.SaveChangesAsync();

                _context.Hotels.RemoveRange(h);
                await _context.SaveChangesAsync();

                _context.Restaurants.RemoveRange(r);
                await _context.SaveChangesAsync();

                _context.Users.Remove(ur.User);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(_localizer.GetString("Deleted!").Value);
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
    }
}
