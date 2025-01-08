using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public enum FacilityRoom
    {
        FreeWiFi,
        AirConditioning,
        Heating,
        NonSmoking,
        Smoking,
        PetFriendly,
        Kitchen,
        PrivateBathroom,
        WashingMachine,
        Dryer,
        Iron,
        HairDryer,

        // Udogodnienia w pokoju
        RoomService,
        MiniBar,
        CoffeeMaker,
        InRoomSafe,
        FlatScreenTV,
        Balcony,
        OceanView,
        MountainView,
        BlackoutCurtains,
        PremiumBedding,

        // Dostępne technologie
        FreeHighSpeedInternet,
        SatelliteTV,
        StreamingServiceAccess,
        SmartHomeFeatures,
        USBChargingPorts
    }

    //typy pokoi
    public enum RoomTypes
    {
        Standard,
        Double,
        Twin,
        Family,
        Studio,
        Superior,
        Deluxe,
        Suite,
        JuniorSuite,
        Penthouse,
        Loft,
        Dormitory,
        JacuzziRoom,
        PoolSuite,
        Accessible,
        Connecting,
        Executive,
        Business,
        Villa
    }

    public class RoomImage
    {
        public ulong RoomImageId { get; set; }
        public ulong RoomId { get; set; }
        public string ImageSrc { get; set; } = null!;

        [ForeignKey("RoomId")]
        public Room Room { get; set; } = null!;
    }

    //pokoje pokazywane do wyboru klientowi według typu
    public class Room
    {
        public ulong RoomId { get; set; }
        public string Name { get; set; } = null!;
        public RoomTypes RoomType { get; set; }
        public List<FacilityRoom> Facilities { get; set; } = [];
        public int PersonCount { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public ICollection<RoomEntity> Rooms { get; set; } = [];
        public ICollection<RoomImage> Images { get; set; } = [];

        [Required]
        [JsonIgnore]
        [ForeignKey("HotelId")]
        public ulong HotelId { get; set; }
        [JsonIgnore]
        public Hotel Hotel { get; set; } = null!;

        // Dodaj konfigurację kaskadowego usuwania
        [JsonIgnore]
        public ICollection<HotelRes> HotelReservations { get; set; } = [];
    }

    //pojedyńcze pokoje i ich dostępność
    public class RoomEntity
    {
        public ulong Id { get; set; }
        public int RoomNumber { get; set; }
        public ICollection<RoomAvailability> Availabilities { get; set; } = [];
        public ulong RoomId { get; set; }
        public Room Room { get; set; } = null!;
    }
}
