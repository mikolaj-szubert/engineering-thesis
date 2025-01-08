using System.Collections.Immutable;
using System.Net.Http;

using Humanizer;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController(MTM_Web_AppServerContext context, IStringLocalizer<Resource> localizer, HttpClient httpClient) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IStringLocalizer _localizer = localizer;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>Dodaje pokoje</summary>
        /// <param name="room">Obiekt pokoju</param>
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
        [HttpPost]
        public async Task<IActionResult> AddRoom([FromForm] AddRoomDto room)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            if (room.PersonCount == 0) return BadRequest("Cannot post room with no personCount."); //DODAĆ ODPOWIEDŹ

            if (room.Price == 0) return BadRequest("Cannot post free room."); //DODAĆ ODPOWIEDŹ

            if (room.NumberOfGivenRooms == 0) return BadRequest("Cannot post room with no numberOfGivenRooms"); //DODAĆ ODPOWIEDŹ

            //files check
            if (room.Files == null || room.Files.Count == 0)
                return BadRequest(_localizer.GetString("No images added.").Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Hotel? h = await _context.Hotels.FirstOrDefaultAsync(h => h.Name == room.HotelName && h.OwnerUser == ur.User);
                if (h == null)
                    return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);

                if (h.Rooms.Any(r => r.Name == room.Name && r.HotelId == h.HotelId))
                    return BadRequest("Room with given name already exists."); //DODAĆ ODPOWIEDŹ

                Room r = new()
                {
                    Name = room.Name.Trim(),
                    RoomType = room.RoomType,
                    Facilities = room.Facilities,
                    PersonCount = room.PersonCount,
                    Description = room.Description,
                    Price = (decimal)room.Price,
                    Hotel = h,
                    HotelId = h.HotelId,
                };
                //dodaje wybraną liczbę pokoi tego samego typu dla danego hotelu
                await _context.Rooms.AddAsync(r);

                var maxRoomNumber = await _context.RoomEntities.Where(r => r.RoomId == r.RoomId).MaxAsync(r => (int?)r.RoomNumber) ?? 0;
                maxRoomNumber++;
                List<RoomEntity> roomEntities = [];
                for (int i = 0;  i <= maxRoomNumber; i++)
                {
                    var ent = new RoomEntity()
                    {
                        RoomNumber = maxRoomNumber + i,
                        RoomId = r.RoomId,
                        Room = r
                    };
                    roomEntities.Add(ent);
                }
                await _context.RoomEntities.AddRangeAsync(roomEntities);
                await _context.SaveChangesAsync();

                //images adding
                string uploadPath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{h.Name.Dehumanize()}\\";
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                foreach (var f in room.Files)
                {
                    if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);

                        using (FileStream stream = new(filePath, FileMode.Create))
                            await f.CopyToAsync(stream);

                        r.Images.Add(new RoomImage { Room = r, RoomId = r.RoomId, ImageSrc = fileName });
                    }
                    else
                        return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                }
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, "POST room", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok("Room added!"); //DODAĆ ODPOWIEDŹ
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

        /// <summary>Aktualizuje pokoje</summary>
        /// <param name="room">Obiekt pokoju</param>
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
        [HttpPut]
        public async Task<IActionResult> UpdateRoom([FromForm] UpdateRoomDto room)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            if (room.PersonCount < 1) return BadRequest("Cannot post room with no personCount."); //DODAĆ ODPOWIEDŹ

            if (room.Price == 0) return BadRequest("Cannot post free room."); //DODAĆ ODPOWIEDŹ

            if (room.NumberOfGivenRooms < 1) return BadRequest("Cannot post room with no numberOfGivenRooms"); //DODAĆ ODPOWIEDŹ

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Room? r = await _context.Rooms.Include(i => i.Images).FirstOrDefaultAsync(e => e.Name == room.Name && e.Hotel.UserId == ur.User.UserId && e.Hotel.Name == room.HotelName);
                if (r == null)
                    return NotFound("No room found."); //DODAĆ ODPOWIEDŹ

                string? n = null;
                if (room.NewName != null && room.NewName != r.Name && !await _context.Rooms.AnyAsync(e => e.Name == room.NewName && e.HotelId == r.HotelId && e.RoomId != r.RoomId))
                {
                    r.Name = room.NewName.Trim();
                    n = room.NewName.Trim();
                }
                if (room.Description != null && room.Description != r.Description)
                    r.Description = room.Description.Trim();

                if (room.RoomType != null && room.RoomType != r.RoomType)
                    r.RoomType = (RoomTypes)room.RoomType;

                if (room.Facilities != null && room.Facilities != r.Facilities)
                    r.Facilities = room.Facilities;

                if (room.PersonCount != null && room.PersonCount != r.PersonCount)
                    r.PersonCount = (int)room.PersonCount;

                if (room.Price != null && (decimal)room.Price != r.Price)
                    r.Price = (decimal)room.Price;

                if (room.NumberOfGivenRooms != null)
                {
                    _context.RoomEntities.RemoveRange(r.Rooms);
                    var maxRoomNumber = await _context.RoomEntities.Where(r => r.RoomId == r.RoomId).MaxAsync(r => (int?)r.RoomNumber) ?? 0;
                    await _context.RoomEntities.AddRangeAsync(Enumerable.Repeat(new RoomEntity()
                    {
                        RoomNumber = maxRoomNumber + 1,
                        RoomId = r.RoomId,
                        Room = r
                    }, (int)room.NumberOfGivenRooms).ToList());
                }
                await _context.SaveChangesAsync();

                if(room.Files != null)
                {
                    //images adding
                    string uploadPath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{r.Hotel.Name.Dehumanize()}\\";

                    foreach (var img in r.Images)
                    {
                        string path = Path.Combine(uploadPath, img.ImageSrc);
                        if (!r.Images.Remove(img)) throw new Exception("Error removing old images.");
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }


                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    foreach (var f in room.Files)
                    {
                        if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                            string filePath = Path.Combine(uploadPath, fileName);

                            using (FileStream stream = new(filePath, FileMode.Create))
                                await f.CopyToAsync(stream);

                            r.Images.Add(new RoomImage { Room = r, RoomId = r.RoomId, ImageSrc = fileName });
                        }
                        else
                            return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                    }
                    await _context.SaveChangesAsync();
                }

                //logging
                await Safety.Log(ur.User, "PUT room", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok("Room updated."); //DODAĆ ODPOWIEDŹ
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

        [HttpDelete("{hotel}/{room}")]
        public async Task<IActionResult> DeleteRoom(string hotel, string room)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var r = await _context.Rooms.Include(e=>e.Rooms).Include(i => i.Images).Include(h=>h.Hotel).FirstOrDefaultAsync(e=>e.Name == room && e.Hotel.Name == hotel);
                if (r == null)
                    return NotFound("Room not found."); //DODAĆ ODPOWIEDŹ

                string deletePath = $"..\\MTM-Web-App.Server\\Img\\Hotels\\{r.Hotel.Name.Dehumanize()}\\";
                foreach (var img in r.Images.ToList())
                {
                    string path = Path.Combine(deletePath, img.ImageSrc);
                    if (!r.Images.Remove(img)) throw new Exception("Error removing old images.");
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.RoomEntities.RemoveRange(r.Rooms);
                await _context.SaveChangesAsync();
                _context.Rooms.Remove(r);
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, "DELETE room", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok("Room deleted.");
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
