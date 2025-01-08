using Microsoft.AspNetCore.Mvc;

using MTM_Web_App.Server.Models;

namespace MTM_Web_App.Server.Helpers
{
    public class ExchangeRateResponse
    {
        public float Amount { get; set; }
        public string? Base { get; set; }
        public string? Date { get; set; }
        public IDictionary<string, float>? Rates { get; set; }
    }

    public class RatingDto
    {
        public string ObjectName { get; set; } = null!;
        public uint Rating { get; set; }
    }

    public class GoogleDto
    {
        /// <example>4/0AVG7fiSBoDyykZwCRQ5jfqRV2p-jrHkt9F9ZO0blE_1JRz4p9e1QLvoixmN2h5ewCHHx2g</example>
        public string Token { get; set; } = null!;
    }

    public class HReservation
    {
        /// <example>2025-05-13</example>
        public string StartDate { get; set; } = null!;
        /// <example>2025-05-18</example>
        public string EndDate { get; set; } = null!;
        /// <example>California Haze</example>
        public string HotelName { get; set; } = null!;
        /// <example>Pokój dla 2 osób</example>
        public string RoomName { get; set; } = null!;
        /// <example>Uwaga do rezerwacji</example>
        public string? Notes { get; set; }
    }

    public class RReservation
    {
        /// <example>2025-05-18</example>
        public string Date { get; set; } = null!;
        /// <example>Pizza Hut</example>
        public string RestaurantName { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public string? Notes { get; set; }
    }

    public class AddRoomDto
    {
        /// <example>Pokój dwuosobowy</example>
        public string Name { get; set; } = null!;
        /// <example>Standard</example>
        public RoomTypes RoomType { get; set; }
        public List<FacilityRoom> Facilities { get; set; } = [];
        /// <example>2</example>
        public int PersonCount { get; set; }
        /// <example>Opis</example>
        public string? Description { get; set; }
        /// <example>100</example>
        public float Price { get; set; }
        /// <example>California Haze</example>
        public string HotelName { get; set; } = null!;
        /// <example>1</example>
        public int NumberOfGivenRooms { get; set; } = 1;
        public FormFileCollection Files { get; set; } = null!;
    }

    public class UpdateRoomDto
    {
        public string Name { get; set; } = null!;
        public string? NewName { get; set; }
        public RoomTypes? RoomType { get; set; }
        public List<FacilityRoom>? Facilities { get; set; }
        public int? PersonCount { get; set; }
        public string? Description { get; set; }
        public float? Price { get; set; }
        public string HotelName { get; set; } = null!;
        public int? NumberOfGivenRooms { get; set; }
        public FormFileCollection? Files { get; set; } = null!;
    }

    public class AddTableDto
    {
        public string Name { get; set; } = null!;
        public int PersonCount { get; set; }
        public string? Description { get; set; }
        public float Price { get; set; }
        public string RestaurantName { get; set; } = null!;
        public int NumberOfGivenTables { get; set; } = 1;
        public FormFileCollection Files { get; set; } = null!;
    }

    public class UpdateTableDto
    {
        public string Name { get; set; } = null!;
        public string? NewName { get; set; }
        public int? PersonCount { get; set; }
        public string? Description { get; set; }
        public float? Price { get; set; }
        public string RestaurantName { get; set; } = null!;
        public int? NumberOfGivenTables { get; set; }
        public FormFileCollection? Files { get; set; } = null!;
    }

    public class OpeningHoursDto
    {
        /// <example>Monday</example>
        public string DayOfWeek { get; set; } = null!;
        /// <example>10:00</example>
        public string OpeningTime { get; set; } = null!;
        /// <example>23:00</example>
        public string ClosingTime { get; set; } = null!;
    }

    public class HUploadModel
    {
        /// <example>California Haze</example>
        public string HotelName { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public IFormFileCollection Files { get; set; } = null!;
    }

    public class RUploadModel
    {
        /// <example>California Haze</example>
        public string RestaurantName { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public IFormFileCollection Files { get; set; } = null!;
    }

    public class RReturnCoordinatesDto
    {
        /// <example>Pizza Hut Delivery, 4350, South Pulaski Road, Archer Heights, Chicago, Cook County, Illinois, 60632, USA</example>
        public string DisplayName { get; set; } = null!;
        /// <example>41.8138261</example>
        public double Lat { get; set; }
        /// <example>-87.7242681</example>
        public double Lon { get; set; }
        /// <example>4350</example>
        public string HouseNumber { get; set; } = null!;
        /// <example>South Pulaski Road</example>
        public string Road { get; set; } = null!;
        /// <example>Chicago</example>
        public string City { get; set; } = null!;
        /// <example>Illinois</example>
        public string State { get; set; } = null!;
        /// <example>il</example>
        public string? StateCode { get; set; } = null!;
        /// <example>60632</example>
        public string PostalCode { get; set; } = null!;
        /// <example>United States of America</example>
        public string Country { get; set; } = null!;
    }

    public class UpdateHotelDto
    {
        /// <example>California Haze</example>
        public string Name { get; set; } = null!;
        /// <example>Californian Haze</example>
        public string? NewName { get; set; }
        public string? Lat { get; set; }
        public string? Lon { get; set; }
        /// <example>15:00</example>
        public string? CheckIn { get; set; }
        ///<example>12:00</example>
        public string? CheckOut { get; set; }
        public ObjType? Type { get; set; }

        public List<FacilityHotel>? Facilities { get; set; }
        ///<example>PLN</example>
        public Currency? HotelCurrency { get; set; }
        /// <example>Located 160 metres’ walk from the beach, this Californian Haze hotel features an outdoor swimming pool and sun terrace. The hotel is 9 minutes’ drive from the shops and restaurants of Lincoln Road and offers free WiFi to guests.</example>
        public string? Description { get; set; }
        public IFormFileCollection? Files { get; set; } = null!;
    }
    public class PostHotelDto
    {
        /// <example>California Haze</example>
        public string Name { get; set; } = null!;
        public string Lat { get; set; } = null!;
        public string Lon { get; set; } = null!;
        /// <example>14:00</example>
        public string CheckIn { get; set; } = null!;
        /// <example>11:00</example>
        public string CheckOut { get; set; } = null!;
        /// <example>Hotel</example>
        public ObjType Type { get; set; }
        public List<FacilityHotel>? Facilities { get; set; }
        public Currency HotelCurrency { get; set; }
        /// <example>Description</example>
        public string Description { get; set; } = null!;
        public IFormFileCollection Files { get; set; } = null!;
    }
    public class UpdateRestaurantDto
    {
        /// <example>Pizza Hut</example>
        public string Name { get; set; } = null!;
        /// <example>Pizza Hut Express</example>
        public string? NewName { get; set; }
        public string? Lat { get; set; }
        public string? Lon { get; set; }
        /// <example>
        /// [
        ///     "Polish",
        ///     "Italian"
        /// ]
        /// </example>
        public List<Cusine>? Cusines { get; set; }
        public List<OpeningHoursDto>? OpenDays { get; set; }
        /// <example>USD</example>
        public Currency? RestaurantCurrency { get; set; }
        /// <example>Description</example>
        public string? Description { get; set; }
        public IFormFileCollection? Files { get; set; } = null!;
    }
    public class PostRestaurantDto
    {
        /// <example>Pizza Hut</example>
        public string Name { get; set; } = null!;
        public string Lat { get; set; } = null!;
        public string Lon { get; set; } = null!;
        /// <example>
        /// [
        ///     "Polish",
        ///     "Italian"
        /// ]
        /// </example>
        public List<Cusine> Cusines { get; set; } = null!;
        /// <example>
        /// [
        ///     "Monday",
        ///     "Friday",
        ///     "Sunday"
        /// ]
        /// </example>
        public List<string> OpenDays { get; set; } = null!;
        /// <example>
        /// [
        ///     "10:00",
        ///     "10:00",
        ///     "11:00"
        /// ]
        /// </example>
        public List<string> StartHours { get; set; } = null!;
        /// <example>
        /// [
        ///     "23:00",
        ///     "23:00",
        ///     "22:00"
        /// ]
        /// </example>
        public List<string> EndHours { get; set; } = null!;
        /// <example>USD</example>
        public Currency RestaurantCurrency { get; set; }
        /// <example>Description</example>
        public string Description { get; set; } = null!;
        public IFormFileCollection Files { get; set; } = null!;
    }

    public class UserResult
    {
        public IActionResult Result { get; set; } = null!;
        public User? User { get; set; }
    }

    public class UpdateUserDto
    {
        /// <example>name@example.com</example>
        public string? Email { get; set; }
        /// <example>John Doe</example>
        public string? Name { get; set; }
        /// <example>P@ssw0rd</example>
        public string? Password { get; set; }
    }

    public class RegisterUserDto
    {
        /// <example>P@ssw0rd</example>
        public string Password { get; set; } = null!;
    }

    public class SendOTP
    {
        /// <example>name@example.com</example>
        public string Email { get; set; } = null!;
        /// <example>John Doe</example>
        public string? Name { get; set; }
    }

    public class LoginDto
    {
        /// <example>name@example.com</example>
        public string Email { get; set; } = null!;
        /// <example>P@ssw0rd</example>
        public string Password { get; set; } = null!;
    }

    public class OtpDto
    {
        /// <example>name@example.com</example>
        public string Email { get; set; } = null!;
        /// <example>0F3B3A</example>
        public string Code { get; set; } = null!;
    }
}
