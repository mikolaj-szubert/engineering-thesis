using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration configuration, MTM_Web_AppServerContext context, HttpClient httpClient, IEmailSender emailSender, IStringLocalizer<Resource> localizer) : ControllerBase
    {
        private readonly MTM_Web_AppServerContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IStringLocalizer _localizer = localizer;

        // POST: api/auth/login
        /// <summary>Przeprowadza logowanie użytkownika</summary>
        /// <returns>Token dotępu</returns>
        /// <param name="loginDto">Dane logowania</param>
        /// <see cref="LoginDto"/>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="401">Niepoprawne dane logowania</response>
        /// <response code="400">Niepoprawne dane wejściowe</response>
        /// <response code="200">Poprawny wynik</response>
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Token>(StatusCodes.Status200OK)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            //received data check
            if (loginDto == null || loginDto.Email == null || loginDto.Password == null)
                return BadRequest(_localizer.GetString("Invalid data.").Value);

            var user = _context.Users.FirstOrDefault(u => u.Email == loginDto.Email.Trim());
            if (user == null)
                return Unauthorized(_localizer.GetString("Invalid credentials.").Value);

            if (user.PasswordHash == null || user.Salt == null)
                return BadRequest(_localizer.GetString("Login using Google and then set your password.").Value);

            if (Safety.VerifyPassword(loginDto.Password.Trim(), user.PasswordHash, user.Salt))
            {
                //return refresh and access token
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    //logging
                    await Safety.Log(user, "Login", _context, _httpClient, HttpContext);

                    var accessToken = await Safety.GenerateAccessToken(user, _configuration, _context);
                    string refreshToken;
                    if (user.RefreshToken == null)
                    {
                        refreshToken = Safety.GenerateRefreshToken();
                        await Safety.SaveRefreshTokenAsync(user.UserId, refreshToken, _context);
                    }
                    else refreshToken = user.RefreshToken;
                    await transaction.CommitAsync();
                    var refreshTokenCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(7)
                    };

                    Response.Cookies.Append("refreshToken", refreshToken, refreshTokenCookieOptions);

                    Console.WriteLine("Zalogowano z refreshToken: " + refreshToken + ", oraz accessToken: " + accessToken);
                    return Ok(accessToken);
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
            return Unauthorized(_localizer.GetString("Invalid credentials.").Value);
        }

        private async Task<bool> DownloadImage(string imageUrl, ulong userId)
        {
            try
            {
                using var response = await _httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                    return false;

                var imageData = await response.Content.ReadAsByteArrayAsync();
                string filename = Guid.NewGuid().ToString() + ".png";
                string filePath = Path.Combine("..\\MTM-Web-App.Server\\Img\\Users\\", filename);
                await System.IO.File.WriteAllBytesAsync(filePath, imageData);

                var usr = await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
                if (usr == null)
                {
                    System.IO.File.Delete(filePath);
                    return false;
                }
                usr.PfpSrc = filename;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // POST: api/auth/google-login
        /// <summary>Przeprowadza logowanie użytkownika za pomocą Google</summary>
        /// <returns>Token dotępu</returns>
        /// <param name="googleDto">Token</param>
        /// <see cref="GoogleDto"/>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="401">Niepoprawne dane logowania</response>
        /// <response code="400">Niepoprawne dane wejściowe</response>
        /// <response code="200">Poprawny wynik</response>
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Token>(StatusCodes.Status200OK)]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleDto googleDto)
        {
            var referer = Request.Headers.Referer.ToString();
            var origin = Request.Headers.Origin.ToString();
            string redirectUri = referer ?? origin;
            string[] redirectUriArr = redirectUri.Split('/');

            if (googleDto == null || string.IsNullOrEmpty(googleDto.Token) || redirectUriArr.Length < 3)
                return BadRequest(_localizer.GetString("Invalid data.").Value);

            string? ClientId = _configuration["Google:ClientId"];
            string? ClientSecret = _configuration["Google:ClientSecret"];

            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
                throw new Exception("Google ClientId or ClientSecret is invalid");

            redirectUri = redirectUriArr[0] + "//" + redirectUriArr[2];

            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var postData = new Dictionary<string, string>
            {
                { "code", googleDto.Token },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "redirect_uri", redirectUri.TrimEnd('/') }, //zmienić https/http/port w razie potrzeby (bez / na końcu adresu)
                { "grant_type", "authorization_code" }
            };

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            //if (!response.IsSuccessStatusCode) return BadRequest(_localizer.GetString("Error retrieving OAuth token.").Value);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(response.Content);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            //jeśli w odpowiedzi jest id_token
            if (root.TryGetProperty("id_token", out JsonElement idTokenElement))
            {
                var idToken = idTokenElement.GetString();
                if (idToken != null)
                {
                    List<Claim> claims = DecodeIdToken(idToken);
                    if (claims.Count > 0)
                    {
                        string? email = claims.FirstOrDefault(e => e.Type == "email")?.Value;
                        string? sub = claims.FirstOrDefault(e => e.Type == "sub")?.Value;
                        string? name = claims.FirstOrDefault(e => e.Type == "name")?.Value;
                        string? picture = claims.FirstOrDefault(e => e.Type == "picture")?.Value;

                        if (email != null && sub != null && name != null && picture != null)
                        {
                            using var transaction = await _context.Database.BeginTransactionAsync();
                            try
                            {
                                string accessToken, refreshToken;
                                var usr = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                                if (usr != null)
                                {
                                    if (usr.GoogleSub != null && usr.GoogleSub != sub)
                                        return Unauthorized("You can't log in using Google credentials. Try logging in classical way.");

                                    usr.IsEmailValid = true;
                                    usr.IsUserValid = true;
                                    if (usr.GoogleSub == null) //użytkownik ma konto lecz nie logował się z google
                                    {
                                        usr.GoogleSub = sub;
                                        if (usr.PfpSrc == null)
                                        {
                                            await DownloadImage(picture, usr.UserId);
                                        }
                                    }
                                    await _context.SaveChangesAsync();

                                    //logging
                                    await Safety.Log(usr, "googleLogin", _context, _httpClient, HttpContext);

                                    accessToken = await Safety.GenerateAccessToken(usr, _configuration, _context);
                                    if (usr.RefreshToken == null)
                                    {
                                        refreshToken = Safety.GenerateRefreshToken();
                                        await Safety.SaveRefreshTokenAsync(usr.UserId, refreshToken, _context);
                                    }
                                    else refreshToken = usr.RefreshToken;
                                    await transaction.CommitAsync();
                                }
                                //użytkownik nie ma konta
                                else
                                {
                                    var u = new User
                                    {
                                        Email = email,
                                        Name = name,
                                        IsEmailValid = true,
                                        IsUserValid = true,
                                        GoogleSub = sub
                                    };
                                    await _context.Users.AddAsync(u);
                                    await _context.SaveChangesAsync();

                                    await DownloadImage(picture, u.UserId);

                                    //logging
                                    await Safety.Log(u, "googleRegister", _context, _httpClient, HttpContext);

                                    accessToken = await Safety.GenerateAccessToken(u, _configuration, _context);
                                    if (u.RefreshToken == null)
                                    {
                                        refreshToken = Safety.GenerateRefreshToken();
                                        await Safety.SaveRefreshTokenAsync(u.UserId, refreshToken, _context);
                                    }
                                    else refreshToken = u.RefreshToken;
                                    await transaction.CommitAsync();
                                }
                                var refreshTokenCookieOptions = new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.None,
                                    Expires = DateTime.UtcNow.AddDays(7)
                                };

                                Response.Cookies.Append("refreshToken", refreshToken, refreshTokenCookieOptions);
                                return Ok(accessToken);
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
            }
            return BadRequest(_localizer.GetString("Something went wrong. Try again later.").Value);
        }

        private static List<Claim> DecodeIdToken(string idToken)
        {
            List<Claim> claims = [];
            var handler = new JwtSecurityTokenHandler();
            if (handler.ReadToken(idToken) is JwtSecurityToken jsonToken)
                foreach (Claim claim in jsonToken.Claims)
                    claims.Add(claim);

            return claims;
        }

        // POST: api/auth/register
        /// <summary>Rejestruje użytkownika i wyysła maila weryfikacyjnego na jego adres email</summary>
        /// <returns>Token dotępu</returns>
        /// <param name="userDto">Obiekt z danymi użytkownika, którego chcemy dodać</param>
        /// <see cref="RegisterUserDto"/>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="409">Email jest już zarejestrowany</response>
        /// <response code="400">Niepoprawny lub pusty adres email</response>
        /// <response code="200">Token dostępu</response>
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String409>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Token>(StatusCodes.Status200OK)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto userDto)
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer, false);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            //create user
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                User? user = await _context.Users.FirstOrDefaultAsync(e => e.UserId == ur.User.UserId);
                if (user == null)
                    return BadRequest(_localizer.GetString("Invalid data.").Value);

                if (user.IsUserValid)
                    return BadRequest(_localizer.GetString("Invalid data.").Value);

                user.PasswordHash = Safety.HashPassword(userDto.Password, out byte[] salt);
                user.Salt = salt;
                user.IsUserValid = true;
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(user, "Register", _context, _httpClient, HttpContext);

                var accessToken = await Safety.GenerateAccessToken(user, _configuration, _context);
                var refreshToken = Safety.GenerateRefreshToken();
                await Safety.SaveRefreshTokenAsync(user.UserId, refreshToken, _context);

                await transaction.CommitAsync();

                var refreshTokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("refreshToken", refreshToken, refreshTokenCookieOptions);

                return Ok(accessToken);
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

        // GET: api/auth/logout
        /// <summary>Wylogowywuje użytkownika, zmieniając jego refreshToken na 0</summary>
        /// <returns>Nic</returns>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="200">Poprawna odpowiedź</response>
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer, false);
            if (ur.Result is not OkObjectResult || ur.User == null)
                return ur.Result;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddSeconds(5)
            };
            Response.Cookies.Append("refreshToken", "", cookieOptions);
            ur.User.RefreshToken = null;
            await _context.SaveChangesAsync();

            return Ok(_localizer.GetString("Logged out successfully.").Value);
        }

        // POST: api/auth/check-otp
        /// <summary>Sprawdza kod weryfikacyjny</summary>
        /// <returns>Nic</returns>
        /// <param name="otpDto">Obiekt z mailem oraz kodem.</param>
        /// <see cref="OtpDto"/>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="404">Błędny kod</response>
        /// <response code="400">Użytkownik o podanym emailu prawdopodobnie nie prosił o kod</response>
        /// <response code="200">Poprawny wynik</response>
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String404>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("check-otp")]
        public async Task<IActionResult> CheckOtpCode([FromBody] OtpDto otpDto)
        {
            var usr = await _context.Users.FirstOrDefaultAsync(u => u.Email == otpDto.Email.Trim());
            if (usr == null)
                return BadRequest(_localizer.GetString("Invalid data.").Value);

            //update user data and remove unused otps
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool isOtpValid = await _context.OTPs.AnyAsync(e => e.UserId == usr.UserId && e.Code == otpDto.Code);
                if (!isOtpValid)
                    return NotFound(_localizer.GetString("Your One-Time Code is invalid.").Value);

                _context.OTPs.RemoveRange(_context.OTPs.Where(e => e.UserId == usr.UserId));
                await _context.SaveChangesAsync();
                usr.IsEmailValid = true;
                await _context.SaveChangesAsync();

                //logging
                await Safety.Log(usr, "OTPchecked", _context, _httpClient, HttpContext);
                var accessToken = await Safety.GenerateAccessToken(usr, _configuration, _context);
                var refreshToken = Safety.GenerateRefreshToken();
                await Safety.SaveRefreshTokenAsync(usr.UserId, refreshToken, _context);

                await transaction.CommitAsync();

                var refreshTokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                Response.Cookies.Append("refreshToken", refreshToken, refreshTokenCookieOptions);

                return Ok(_localizer.GetString("Your email address has been verified!").Value);
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

        //  GET: api/auth/is-name-free
        /// <summary>Sprawdza czy użytkownik istnieje</summary>
        /// <returns>Nic</returns>
        /// <response code="404">Użytkownik nie istnieje</response>
        /// <response code="200">Użytkownik istnieje</response>
        [HttpGet("is-name-free/{email}")]
        [ProducesResponseType<String404>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        public async Task<IActionResult> IsEmailFree(string email) => await _context.Users.AnyAsync(u => u.Email == email.Trim()) ? Conflict(_localizer.GetString("Email already exist. Try logging in.").Value) : Ok();

        // POST: api/auth/send-otp
        /// <summary>Wysyła ponownie email weryfikacyjny</summary>
        /// <returns>Nic</returns>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="400">Email został już zweryfikowany</response>
        /// <response code="200">Poprawnie wysłano</response>
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<String400>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<String200>(StatusCodes.Status200OK)]
        [HttpPost("send-otp")]
        public async Task<IActionResult> PostSendOtp(SendOTP so)
        {
            var usr = await _context.Users.FirstOrDefaultAsync(u => u.Email == so.Email.Trim());
            if (usr != null && !usr.IsEmailValid && !usr.IsUserValid)
            {
                _context.Users.Remove(usr);
                usr = null;
            }
            if (usr == null)
            {
                if (string.IsNullOrEmpty(so.Name))
                    return BadRequest(_localizer.GetString("Invalid data.").Value);

                var newUsr = new User
                {
                    Email = so.Email,
                    Name = so.Name
                };
                await _context.Users.AddAsync(newUsr);
                await _context.SaveChangesAsync();
                await Safety.SendOtp(newUsr, _emailSender, _context, _localizer);
                return Ok(_localizer.GetString("One-Time Code has been sent.").Value);
            }
            else
            {
                if (!string.IsNullOrEmpty(so.Name))
                    return BadRequest(_localizer.GetString("Invalid data.").Value);

                await Safety.SendOtp(usr, _emailSender, _context, _localizer);
                return Ok(_localizer.GetString("One-Time Code has been sent.").Value);
            }
        }

        // POST: api/auth/refresh-token
        /// <summary>Aktualizuje i zwraca token dostępu</summary>
        /// <returns>Token dostępu</returns>
        /// <response code="500">Błąd serwera. Sprawdź błąd w konsoli</response>
        /// <response code="401">Brak dostępu - niepoprawny token</response>
        /// <response code="200">Token dostępu</response>
        [ProducesResponseType<String500>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<String401>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<Token>(StatusCodes.Status200OK)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            //authentication
            UserResult ur = await Safety.GetUserFromRequest(_context, HttpContext, _localizer, false);
            if (ur.Result is not OkObjectResult || ur.User == null) return ur.Result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //generate new access and refresh token
                var newAccessToken = await Safety.GenerateAccessToken(ur.User, _configuration, _context);
                string newRefreshToken;
                if (ur.User.RefreshToken == null)
                {
                    newRefreshToken = Safety.GenerateRefreshToken();
                    await Safety.SaveRefreshTokenAsync(ur.User.UserId, newRefreshToken, _context);
                }
                else newRefreshToken = ur.User.RefreshToken;
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

                await transaction.CommitAsync();
                return Ok(newAccessToken);
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
