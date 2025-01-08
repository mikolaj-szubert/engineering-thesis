using System.Net.Http;

using Humanizer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/tables")]
    public class TableController(MTM_Web_AppServerContext context, HttpClient httpClient, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IStringLocalizer _localizer = localizer;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>Dodaje stoliki</summary>
        /// <param name="table">Obiekt stolika</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji o podanej nazwie</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email lub stolik o podanej nazwie już istnieje</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/rooms
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> AddTable([FromForm] AddTableDto table)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Restaurant? r = await _context.Restaurants.FirstOrDefaultAsync(h => h.Name == table.RestaurantName && h.OwnerUser == ur.User);
                if (r == null)
                    return NotFound(_localizer.GetString("Specified hotel does not exist.").Value);

                if (r.Tables.Any(r => r.Name == table.Name && r.RestaurantId == r.RestaurantId))
                    return BadRequest(); //DODAĆ ODPOWIEDŹ

                Table t = new()
                {
                    Name = table.Name,
                    PersonCount = table.PersonCount,
                    Description = table.Description,
                    Price = (decimal)table.Price,
                    Restaurant = r,
                    RestaurantId = r.RestaurantId,
                };
                //dodaje wybraną liczbę stołów tego samego typu dla danej restauracji
                await _context.Tables.AddAsync(t);

                var maxTableNumber = await _context.TableEntities.Where(r => r.TableId == r.TableId).MaxAsync(r => (int?)r.TableNumber) ?? 0;
                maxTableNumber++;
                List<TableEntity> tableEntities = [];
                for (int i = 0; i <= maxTableNumber; i++)
                {
                    var ent = new TableEntity()
                    {
                        TableNumber = maxTableNumber + i,
                        TableId = t.TableId,
                        Table = t
                    };
                    tableEntities.Add(ent);
                }
                await _context.TableEntities.AddRangeAsync(tableEntities);
                await _context.SaveChangesAsync();

                //images adding
                string uploadPath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{r.Name.Dehumanize()}\\";
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                foreach (var f in table.Files)
                {
                    if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);

                        using (FileStream stream = new(filePath, FileMode.Create))
                            await f.CopyToAsync(stream);

                        t.Images.Add(new TableImage { Table = t, TableId = t.TableId, ImageSrc = fileName });
                    }
                    else
                        return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                }
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, "POST table", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(); //DODAĆ ODPOWIEDŹ
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

        /// <summary>Aktualizuje stoliki</summary>
        /// <param name="table">Obiekt stolika</param>
        /// <response code="500">Błąd serwera</response>
        /// <response code="404">Brak restauracji o podanej nazwie</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Niezweryfikowany email</response>
        /// <response code="200">Poprawny wynik</response>
        // POST: api/rooms
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPut]
        public async Task<IActionResult> UpdateTable([FromForm] UpdateTableDto table)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Table? t = await _context.Tables.FirstOrDefaultAsync(e => e.Name == table.Name && e.Restaurant.UserId == ur.User.UserId && e.Restaurant.Name == table.RestaurantName);
                if (t == null)
                    return NotFound(); //DODAĆ ODPOWIEDŹ

                string? n = null;
                if (table.NewName != null && table.NewName != t.Name && !await _context.Tables.AnyAsync(e => e.Name == table.NewName && e.RestaurantId == t.RestaurantId && e.TableId != t.TableId))
                {
                    t.Name = table.NewName.Trim();
                    n = table.NewName.Trim();
                }
                if (table.Description != null && table.Description != t.Description)
                    t.Description = table.Description.Trim();

                if (table.PersonCount != null && table.PersonCount != t.PersonCount)
                    t.PersonCount = (int)table.PersonCount;

                if (table.Price != null && (decimal)table.Price != t.Price)
                    t.Price = (decimal)table.Price;

                if (table.NumberOfGivenTables != null)
                {
                    _context.TableEntities.RemoveRange(t.Tables);
                    var maxTableNumber = await _context.RoomEntities.Where(r => r.RoomId == r.RoomId).MaxAsync(r => (int?)r.RoomNumber) ?? 0;
                    await _context.TableEntities.AddRangeAsync(Enumerable.Repeat(new TableEntity()
                    {
                        TableNumber = maxTableNumber + 1,
                        TableId = t.TableId,
                        Table = t
                    }, (int)table.NumberOfGivenTables).ToList());
                }
                await _context.SaveChangesAsync();

                if(table.Files != null)
                {
                    //images adding
                    string uploadPath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{t.Restaurant.Name.Dehumanize()}\\";

                    foreach (var img in t.Images)
                    {
                        string path = Path.Combine(uploadPath, img.ImageSrc);
                        if (!t.Images.Remove(img)) throw new Exception("Error removing old images.");
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    foreach (var f in table.Files)
                    {
                        if ((f.ContentType.StartsWith("image/png", StringComparison.InvariantCultureIgnoreCase) || f.ContentType.StartsWith("image/jpeg", StringComparison.InvariantCultureIgnoreCase)) && (Path.GetExtension(f.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(f.FileName);
                            string filePath = Path.Combine(uploadPath, fileName);

                            using (FileStream stream = new(filePath, FileMode.Create))
                                await f.CopyToAsync(stream);

                            t.Images.Add(new TableImage { Table = t, TableId = t.TableId, ImageSrc = fileName });
                        }
                        else
                            return BadRequest(_localizer.GetString("Invalid image extension. Accepted file formats are .jpg and .png.").Value);
                    }
                    await _context.SaveChangesAsync();
                }

                //logging
                await Safety.Log(ur.User, "PUT table", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(); //DODAĆ ODPOWIEDŹ
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

        [HttpDelete("{restaurant}/{table}")]
        public async Task<IActionResult> DeleteRoom(string restaurant, string table)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var t = await _context.Tables.Include(e => e.Tables).Include(i => i.Images).Include(r=>r.Restaurant).FirstOrDefaultAsync(e => e.Name == table && e.Restaurant.Name == restaurant);
                if (t == null)
                    return NotFound(); //DODAĆ ODPOWIEDŹ

                string deletePath = $"..\\MTM-Web-App.Server\\Img\\Restaurants\\{t.Restaurant.Name.Dehumanize()}\\";
                foreach (var img in t.Images.ToList())
                {
                    string path = Path.Combine(deletePath, img.ImageSrc);
                    if (!t.Images.Remove(img)) throw new Exception("Error removing old images.");
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.TableEntities.RemoveRange(t.Tables);
                await _context.SaveChangesAsync();
                _context.Tables.Remove(t);
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(ur.User, "DELETE table", _context, _httpClient, HttpContext);

                await transaction.CommitAsync();
                return Ok(); //DODAĆ ODPOWIEDŹ
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
