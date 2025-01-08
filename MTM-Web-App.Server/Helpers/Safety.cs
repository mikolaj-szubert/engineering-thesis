using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Konscious.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

using MTM_Web_App.Server.Data;
using MTM_Web_App.Server.Models;

using Newtonsoft.Json.Linq;

namespace MTM_Web_App.Server.Helpers
{
    public class EmailSender(IConfiguration configuration) : IEmailSender
    {
        private readonly IConfiguration _configuration = configuration;
        public void SendEmail(string toEmail, string title, string body, byte[]? pdfAttachment = default, string? pdfFileName = default)
        {
            SmtpClient client = new(_configuration.GetValue<string>("EmailSettings:SmtpServer"), _configuration.GetValue<int>("EmailSettings:SmtpPort"))
            {
                EnableSsl = _configuration.GetValue<bool>("EmailSettings:EnableSsl"),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_configuration.GetValue<string>("EmailSettings:SenderEmail"), _configuration.GetValue<string>("EmailSettings:SenderPassword"))
            };

            MailMessage mailMessage = new()
            {
                From = new MailAddress(_configuration.GetValue<string>("EmailSettings:SenderEmail") ?? "", _configuration.GetValue<string>("EmailSettings:SenderName")),
                Subject = title,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            if (pdfAttachment != null)
            {
                var memoryStream = new MemoryStream(pdfAttachment);
                var attachment = new Attachment(memoryStream, pdfFileName, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            StringBuilder mailBody = new();
            mailBody.Append(body);
            mailMessage.Body = mailBody.ToString();

            client.Send(mailMessage);
        }
    }

    public interface IEmailSender
    {
        void SendEmail(string toEmail, string title, string body, byte[]? pdfAttachment = default, string? pdfFileName = default);
    }
    public static class Safety
    {
        public async static Task<List<Address>?> GetLocalisedAddresses(string lat, string lon, HttpClient httpClient, IConfiguration configuration)
        {
            string? key = configuration["Geocoding:Key"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("key for geocoding is not configured");
            List<string> i = ["pl", "en"];
            List<Address> returnCoordinates = [];
            foreach (string s in i)
            {
                string latStr = lat.Replace(",",".");
                string lonStr = lon.Replace(",", ".");
                string url = $"https://eu1.locationiq.com/v1/reverse?key={key}&lat={latStr}&lon={lonStr}&accept-language={s}&format=json&normalizeaddress=1&postaladdress=1";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                //setting headers so the geocoding api could always return the same results
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");
                request.Headers.Add("Accept-Language", s);
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonResponse = await response.Content.ReadAsStreamAsync();

                    //response to JSON
                    using var jsonDoc = await JsonDocument.ParseAsync(jsonResponse);
                    JsonElement root = jsonDoc.RootElement;
                    try
                    {
                        bool arr = root.TryGetProperty("address", out JsonElement addr);
                        if (addr.TryGetProperty("house_number", out var HouseNumber) &&
                            addr.TryGetProperty("road", out var Road) &&
                            addr.TryGetProperty("city", out var City) &&
                            addr.TryGetProperty("state", out var State) &&
                            addr.TryGetProperty("postcode", out var PostalCode) &&
                            addr.TryGetProperty("country", out var Country))
                        {
                            Address address = new()
                            {
                                Locale = s,
                                HouseNumber = HouseNumber.ToString(),
                                Road = Road.ToString(),
                                City = City.ToString(),
                                State = State.ToString(),
                                PostalCode = PostalCode.ToString(),
                                Country = Country.ToString()
                            };

                            returnCoordinates.Add(address);
                        }
                        else return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                    return null;
            }

            if (returnCoordinates.Count == 2)
                return returnCoordinates;
            else
                return null;
        }

        public async static Task Log(User user, string logName, MTM_Web_AppServerContext _context, HttpClient _httpClient, HttpContext httpContext)
        {
            string? location;
            string? ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "::1")
            {
                var response = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=country,city");
                var json = JObject.Parse(response);
                location = $"{json["city"]?.ToString()}, {json["country"]?.ToString()}";
            }
            else
            {
                if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwarded))
                    ipAddress = forwarded;
                else if (httpContext.Request.Headers.TryGetValue("X-Ip-Address", out StringValues ip))
                    ipAddress = ip;

                if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "::1")
                {
                    var response = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=country,city");
                    var json = JObject.Parse(response);
                    location = $"{json["city"]?.ToString()}, {json["country"]?.ToString()}";
                }
                else
                {
                    ipAddress = "localhost";
                    location = "unknown";
                }
            }
            Logger log = new()
            {
                UserId = user.UserId,
                LogTime = DateTime.UtcNow,
                Address = ipAddress,
                Location = location,
                LogName = logName
            };
            await _context.Logger.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public static async Task SendOtp(User user, IEmailSender _emailSender, MTM_Web_AppServerContext _context, IStringLocalizer localizer)
        {
            //generate OTP and save it to db
            Random random = new();
            string code = new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6).Select(s => s[random.Next(s.Length)]).ToArray());
            OTP otp = new()
            {
                Code = code,
                UserId = user.UserId,
                ValidUntil = DateTime.UtcNow.AddMinutes(10)
            };
            await _context.OTPs.AddAsync(otp);
            await _context.SaveChangesAsync();

            //send mail
            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Your One-Time Code</title>
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
                        .otp-code {{
                            font-size: 22px;
                            color: #4CAF50;
                            font-weight: bold;
                            text-align: center;
                            padding: 20px;
                            border-radius: 5px;
                            background-color: #f9f9f9;
                            margin: 20px 0;
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
                        <h1>{localizer.GetString("OTCEmailTitle").Value}</h1>
                    </div>
                    <div class=""content"">
                        <p>{localizer.GetString("HelloUser", user.Name).Value}</p>
                        <p>{localizer.GetString("VerificationMessage").Value}</p>
                        <div class=""otp-code"">{code}</div>
                        <p>{localizer.GetString("This code is valid for the next 10 minutes. Please do not share it with anyone.").Value}</p>
                        <p>{localizer.GetString("IgnoreEmail").Value}</p>
                        <p>{localizer.GetString("BestRegards").Value}</p>
                    </div>
                    <div class=""footer"">
                        <p>{localizer.GetString("FooterMessage").Value}</p>
                    </div>
                </div>
                </body>
                </html>";
            _emailSender.SendEmail(user.Email, localizer.GetString("OTCEmailTitle").Value, body);
        }

        public static async Task<UserResult> GetUserFromRequest(MTM_Web_AppServerContext _context, HttpContext httpContext, IStringLocalizer localizer, bool isVerified = true, CancellationToken ct = default)
        {
            string? refreshToken = httpContext.Request.Cookies["refreshToken"];
            var userResult = new UserResult();

            if (refreshToken == null)
            {
                userResult.Result = new UnauthorizedObjectResult(localizer.GetString("You do not have permission to do that.").Value);
                Console.WriteLine("refreshToken==null");
                return userResult;
            }

            var usr = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
            if (usr == null)
            {
                userResult.Result = new UnauthorizedObjectResult(localizer.GetString("You do not have permission to do that.").Value);
                Console.WriteLine("usr==null");
                return userResult;
            }

            if (isVerified && !usr.IsEmailValid)
            {
                userResult.Result = new BadRequestObjectResult(localizer.GetString("Verify your email address first.").Value);
                return userResult;
            }
            userResult.User = usr;
            userResult.Result = new OkObjectResult(userResult.User);
            return userResult;
        }

        public static async Task<string> GenerateAccessToken(User user, IConfiguration configuration, MTM_Web_AppServerContext context)
        {
            if (user.Email == null || user.Name == null)
                throw new NullReferenceException("User email or name is null at token creation");

            var isHotelOwner = await context.Hotels.AnyAsync(u => u.UserId == user.UserId);
            var isRestaurantOwner = await context.Restaurants.AnyAsync(u => u.UserId == user.UserId);
            string userStr = (isHotelOwner ? "h" : "") + (isRestaurantOwner ? "r" : "");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Picture, user.IsGooglePfp && user.PfpSrc != null ? user.PfpSrc: true.ToString()),
                new Claim("Owner", userStr),
            };
            var secretKey = configuration["jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Secret key for JWT is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, "HS512");

            var token = new JwtSecurityToken
            (
                issuer: "https://localhost:5173",
                audience: "MTM Project",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static async Task SaveRefreshTokenAsync(ulong userId, string refreshToken, MTM_Web_AppServerContext context)
        {
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                await context.SaveChangesAsync();
            }
        }

        public static string HashPassword(string password, out byte[] salt)
        {
            byte[] saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            salt = saltBytes;

            using var hasher = new Argon2i(Encoding.UTF8.GetBytes(password));
            hasher.Salt = saltBytes;
            hasher.DegreeOfParallelism = 8;
            hasher.MemorySize = 65536;
            hasher.Iterations = 4;

            byte[] hash = hasher.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash, byte[] storedSalt)
        {
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            using var hasher = new Argon2i(Encoding.UTF8.GetBytes(password));
            hasher.Salt = storedSalt;
            hasher.DegreeOfParallelism = 8;
            hasher.MemorySize = 65536;
            hasher.Iterations = 4;

            byte[] hash = hasher.GetBytes(32); // 32 bajty hasha
            return hash.SequenceEqual(storedHashBytes);
        }

    }
}
