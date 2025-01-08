using MTM_Web_App.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Text;
using MTM_Web_App.Server.Helpers;
using MTM_Web_App.Server.Data;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace MTM_Web_App.Server.Services
{
    public class NotificationService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IStringLocalizer<Resource> localizer) : IHostedService, IDisposable, IEmailSender
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private Timer? _timer;
        private readonly IConfiguration _configuration = configuration;
        private readonly IStringLocalizer _localizer = localizer;

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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckSubscriptions, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void CheckSubscriptions(object? state)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MTM_Web_AppServerContext>();

            var currentTime = DateTime.UtcNow;
            List<HotelRes> hDueTommorow = await dbContext.HotelRes
                .Include(r=>r.Room)
                .ThenInclude(h=>h.Hotel)
                .Where(s => s.StartDate.Date <= currentTime.AddDays(1).Date && s.StartDate.Date > currentTime.Date)
                .ToListAsync();

            List<RestaurantRes> rDueTommorow = await dbContext.RestaurantsRes
                .Include(t => t.Table)
                .ThenInclude(r => r.Restaurant)
                .Where(s => s.Date >= DateOnly.FromDateTime(currentTime.Date) && s.Date < DateOnly.FromDateTime(currentTime.AddDays(1).Date))
                .ToListAsync();

            List<User> users = await dbContext.Users.ToListAsync();

            foreach (HotelRes subscription in hDueTommorow)
            {
                var currentUser = users.Where(e => e.UserId == subscription.UserId).FirstOrDefault() ?? throw new Exception("User is null in Scheduled Task Service at Hotel Reservations");
                if (currentUser != null)
                {
                    string reminderBody = $@"<!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>{_localizer.GetString("Reservation Reminder").Value}</title>
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
                                background-color: #FFA500;
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
                            <h1>{_localizer.GetString("Reservation Reminder").Value}</h1>
                        </div>
                        <div class=""content"">
                            <p>{_localizer.GetString("HelloUser", currentUser.Name)},</p>
                            <p>{_localizer.GetString("This is a friendly reminder about your upcoming reservation at").Value} <strong>{subscription.Room.Hotel.Name}</strong> {localizer.GetString("starting tomorrow").Value}.</p>
                            <p>{_localizer.GetString("Here are the details of your reservation:").Value}</p>
                            <div class=""reservation-details"">
                                <p><strong>{_localizer.GetString("Reservation number:").Value}</strong> {subscription.ReservationNumber}</p>
                                <p><strong>{_localizer.GetString("Check in").Value}:</strong> {subscription.StartDate.ToString("g", CultureInfo.CurrentCulture)}</p>
                                <p><strong>{_localizer.GetString("Check out").Value}:</strong> {subscription.EndDate.ToString("g", CultureInfo.CurrentCulture)}</p>
                                <p><strong>{_localizer.GetString("Price").Value}:</strong> {subscription.SummaryCost} {subscription.Room.Hotel.Currency}</p>
                            </div>
                            <p>{_localizer.GetString("If you have any questions or need to update your reservation, feel free to reach out to us.").Value}</p>
                            <p>{_localizer.GetString("Safe travels, and we look forward to welcoming you tomorrow!").Value}</p>
                            <p>{_localizer.GetString("BestRegards").Value}</p>
                        </div>
                        <div class=""footer"">
                            <p>{_localizer.GetString("FooterMessage").Value}</p>
                        </div>
                    </div>

                    </body>
                    </html>";
                    SendEmail(currentUser.Email, _localizer.GetString("Reservation Reminder").Value, reminderBody);
                }
            }
            foreach (RestaurantRes subscription in rDueTommorow)
            {
                var currentUser = users.Where(e => e.UserId == subscription.UserId).FirstOrDefault() ?? throw new Exception("User is null in Scheduled Task Service at Restaurant Reservations");
                if (currentUser != null)
                {
                    string reminderBody = $@"<!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>{_localizer.GetString("Reservation Reminder").Value}</title>
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
                                background-color: #FFA500;
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
                            <h1>{_localizer.GetString("Reservation Reminder").Value}</h1>
                        </div>
                        <div class=""content"">
                            <p>{_localizer.GetString("HelloUser", currentUser.Name)},</p>
                            <p>{_localizer.GetString("This is a reminder about your reservation at").Value} <strong>{subscription.Table.Restaurant.Name}</strong>, {localizer.GetString("scheduled for tomorrow").Value}.</p>
                            <p>{_localizer.GetString("Here are the details of your reservation:").Value}</p>
                            <div class=""reservation-details"">
                                <p><strong>{_localizer.GetString("Reservation number:").Value}</strong> {subscription.ReservationNumber}</p>
                                <p><strong>{_localizer.GetString("Date").Value}:</strong> {subscription.Date.ToString("d", CultureInfo.CurrentCulture)}</p>
                                <p><strong>{_localizer.GetString("Price").Value}:</strong> {subscription.SummaryCost} {subscription.Table.Restaurant.Currency}</p>
                            </div>
                            <p>{_localizer.GetString("If you have any questions or need to make changes to your reservation, feel free to contact us.").Value}</p>
                            <p>{_localizer.GetString("We look forward to welcoming you!").Value}</p>
                            <p>{localizer.GetString("BestRegards").Value}</p>
                        </div>
                        <div class=""footer"">
                            <p>{localizer.GetString("FooterMessage").Value}</p>
                        </div>
                    </div>

                    </body>
                    </html>";
                    SendEmail(currentUser.Email, _localizer.GetString("Reservation Reminder").Value, reminderBody);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}