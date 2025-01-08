using System.Net.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/rating")]
    public class RatingController(MTM_Web_AppServerContext context, IStringLocalizer<Resource> localizer, HttpClient httpClient) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IStringLocalizer _localizer = localizer;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>Dodaje ocenę restauracji</summary>
        /// <param name="rating">Obiekt pokoju</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak hotelu o podanej nazwie</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email lub pokój o podanej nazwie już istnieje</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/rooms
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("restaurant")]
        public async Task<IActionResult> AddRestaurantRating(RatingDto rating)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var r = await _context.Restaurants.Include(r => r.Ratings).Where(r => r.Name == rating.ObjectName).FirstOrDefaultAsync();
                if (r == null) return NotFound("Restaurant not found");
                var rat = await _context.RestaurantRating.Where(r => r.RestaurantId == r.RestaurantId && r.UserId == ur.User.UserId).FirstOrDefaultAsync();
                if (rat != null)
                {
                    rat.Rate = rating.Rating;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    RRating rate = new()
                    {
                        UserId = ur.User.UserId,
                        RestaurantId = r.RestaurantId,
                        Rate = rating.Rating
                    };
                    await _context.RestaurantRating.AddAsync(rate);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok("Rating posted!"); //DODAĆ ODPOWIEDŹ
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

        [HttpDelete("restaurant/{restaurantName}")]
        public async Task<IActionResult> DeleteRoom(string restaurantName)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var restaurant = await _context.Restaurants.Where(r => r.Name == restaurantName).FirstOrDefaultAsync();
                if (restaurant == null) return NotFound("Restaurant not found."); //DODAĆ ODPOWIEDŹ

                var r = await _context.RestaurantRating.Where(r => r.RestaurantId == restaurant.RestaurantId && r.UserId == ur.User.UserId).FirstOrDefaultAsync();
                if (r == null) return NotFound("Rating not found."); //DODAĆ ODPOWIEDŹ

                _context.RestaurantRating.Remove(r);
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, "DELETE restaurantRating", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok("Rating deleted."); //DODAĆ ODPOWIEDŹ
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

        /// <summary>Dodaje ocenę hotelu</summary>
        /// <param name="rating">Obiekt pokoju</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak hotelu o podanej nazwie</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email lub pokój o podanej nazwie już istnieje</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/rooms
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("hotel")]
        public async Task<IActionResult> AddHotelRating(RatingDto rating)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var h = await _context.Hotels.Include(r => r.Ratings).Where(h => h.Name == rating.ObjectName).FirstOrDefaultAsync();
                if (h == null) return NotFound("Hotel not found"); //DODAĆ ODPOWIEDŹ
                var r = await _context.HotelRatings.Where(r => r.HotelId == h.HotelId && r.UserId == ur.User.UserId).FirstOrDefaultAsync();
                if (r != null)
                {
                    r.Rate = rating.Rating;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    HRating rate = new()
                    {
                        UserId = ur.User.UserId,
                        HotelId = h.HotelId,
                        Rate = rating.Rating
                    };
                    await _context.HotelRatings.AddAsync(rate);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok("Rating posted!"); //DODAĆ ODPOWIEDŹ
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

        [HttpDelete("hotel/{hotelName}")]
        public async Task<IActionResult> DeleteHotelRating(string hotelName)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hotel = await _context.Hotels.Where(r => r.Name == hotelName).FirstOrDefaultAsync();
                if (hotel == null) return NotFound("Hotel not found."); //DODAĆ ODPOWIEDŹ

                var r = await _context.HotelRatings.Where(r => r.HotelId == hotel.HotelId && r.UserId == ur.User.UserId).FirstOrDefaultAsync();
                if (r == null) return NotFound("Rating not found."); //DODAĆ ODPOWIEDŹ

                _context.HotelRatings.Remove(r);
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, "DELETE hotelRating", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok("Rating deleted."); //DODAĆ ODPOWIEDŹ
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
