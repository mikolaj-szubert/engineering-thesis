using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class RestaurantRes
    {
        [Key]
        public ulong RestaurantResId { get; set; }
        public string ReservationNumber { get; set; } = null!;
        public string ReservationVerification { get; set; } = null!;
        [Required]
        public string TableName { get; set; } = null!;
        [Required]
        public DateOnly Date { get; set; }
        [Required]
        public decimal SummaryCost { get; set; }
        public string? Notes { get; set; }

        [Required]
        [JsonIgnore]
        [ForeignKey("TableId")]
        public ulong TableId { get; set; }
        [JsonIgnore]
        public Table Table { get; set; } = null!;

        [Required]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        [JsonIgnore]
        public User ClientUser { get; set; } = null!;
    }
}