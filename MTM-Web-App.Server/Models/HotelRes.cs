using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class HotelRes
    {
        [Key]
        public ulong HotelResId { get; set; }
        public string ReservationNumber { get; set; } = null!;
        public string ReservationVerification { get; set; } = null!;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public decimal SummaryCost { get; set; }
        public string? Notes { get; set; }

        [Required]
        [JsonIgnore]
        [ForeignKey("RoomId")]
        public ulong RoomId { get; set; }
        [JsonIgnore]
        public Room Room { get; set; } = null!;

        [Required]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        [JsonIgnore]
        public User ClientUser { get; set; } = null!;
    }
}