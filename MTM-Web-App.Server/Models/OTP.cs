using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class OTP
    {
        [Key]
        [JsonIgnore]
        public ulong OtpId { get; set; }
        [Required]
        public string Code { get; set; } = null!;
        [Required]
        public DateTime ValidUntil { get; set; }

        [Required]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; } = null!;
    }
}
