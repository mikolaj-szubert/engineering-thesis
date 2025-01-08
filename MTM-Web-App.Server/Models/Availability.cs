using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class RoomAvailability
    {
        [Key]
        public ulong AvailabilityId { get; set; }

        public DateOnly Date { get; set; }
        public bool IsReserved { get; set; } = true;

        [Required]
        [ForeignKey("RoomId")]
        public ulong RoomId { get; set; }
        public Room Room { get; set; } = null!;
    }
    public class TableAvailability
    {
        [Key]
        public ulong TableAvailabilityId { get; set; }

        public DateOnly Date { get; set; }
        public bool IsReserved { get; set; } = true;

        [Required]
        [ForeignKey("TableId")]
        public ulong TableId { get; set; }
        public Table Table { get; set; } = null!;
    }
}
