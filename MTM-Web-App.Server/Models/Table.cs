using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public class Table
    {
        public ulong TableId { get; set; }
        public string Name { get; set; } = null!;
        public int PersonCount { get; set; }
        public int TableNumber { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public ICollection<TableEntity> Tables { get; set; } = [];
        public ICollection<TableImage> Images { get; set; } = []; //zdjęcia konretnego stolika/stolików na daną liczbę osób

        [Required]
        [JsonIgnore]
        [ForeignKey("RestaurantId")]
        public ulong RestaurantId { get; set; }
        [JsonIgnore]
        public Restaurant Restaurant { get; set; } = null!;

        // Dodaj konfigurację kaskadowego usuwania
        [JsonIgnore]
        public ICollection<RestaurantRes> RestaurantReservations { get; set; } = [];
    }

    public class TableImage
    {
        public ulong TableImageId { get; set; }
        public ulong TableId { get; set; }
        public string ImageSrc { get; set; } = null!;

        [ForeignKey("TableId")]
        public Table Table { get; set; } = null!;
    }

    //pojedyńcze stoliki i ich dostępność
    public class TableEntity
    {
        public ulong Id { get; set; }
        public int TableNumber { get; set; }
        public ICollection<TableAvailability> Availabilities { get; set; } = [];
        public ulong TableId { get; set; }
        public Table Table { get; set; } = null!;
    }
}
