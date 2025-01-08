using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class Logger
    {
        [Key]
        [JsonIgnore]
        public ulong LogId { get; set; }
        [Required]
        public DateTime LogTime { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        public string? LogName { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        [Required]
        public ulong UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }
}
