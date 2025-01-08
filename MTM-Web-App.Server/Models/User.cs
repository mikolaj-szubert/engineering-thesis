using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class User
    {
        [Key]
        [JsonIgnore]
        public ulong UserId { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public byte[]? Salt { get; set; }
        public string? RefreshToken { get; set; }
        public bool IsEmailValid { get; set; } = false;
        public bool IsUserValid { get; set; } = false;
        public string? PfpSrc { get; set; }

        public bool IsGooglePfp { get; set; } = false;
        //Google login
        public string? GoogleSub { get; set; }

        public ICollection<Logger> Logger { get; } = [];
        public ICollection<Restaurant> OwnedRestaurants { get; } = [];
        public ICollection<Hotel> OwnedHotels { get; } = [];
        public ICollection<HotelRes> HotelRes { get; } = [];
        public ICollection<RestaurantRes> RestaurantRes { get; } = [];
    }
}
