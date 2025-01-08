using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    //Waluty jakimi w danym hotelu można płacić
    public enum Currency
    {
        PLN, //Polish Zloty New
        GBP, //Great Britain Pound
        EUR, //Euro
        USD, //United States Dollar
        CAD, //Canadian dollar
        AUD, //Australian Dollar
        JPY, //Japanese Yen
        INR, //Indian Rupee
        NZD, //New Zealand Dollar
        CHF, //Swiss Franc
    }

    //Udogodnienia
    public enum FacilityHotel
    {
        FreeParking,
        PaidParking,
        Elevator,
        WheelchairAccessible,

        // Rekreacyjne
        Pool,
        HotTub,
        Sauna,
        FitnessCenter,
        Spa,
        MassageService,
        GameRoom,
        Playground,
        Library,
        Garden,
        Terrace,
        BarbecueFacilities,

        // Sportowe i aktywności
        TennisCourt,
        GolfCourse,
        WaterSportsFacilities,
        SkiStorage,
        SkiInSkiOutAccess,
        HikingTrails,
        BicycleRental,
        HorseRiding,
        BowlingAlley,

        // Transport i mobilność
        AirportShuttle,
        CarRental,
        ElectricVehicleCharging,

        // Jedzenie i napoje
        Restaurant,
        Bar,
        BreakfastIncluded,
        Kitchenette,
        VendingMachine,

        // Usługi dla gości
        ConciergeService,
        Housekeeping,
        LaundryService,
        BabysittingService,
        LuggageStorage,
        CurrencyExchange,
        ATMOnSite,
        TourDesk,
        TicketService,

        // Udogodnienia biznesowe
        BusinessCenter,
        MeetingRooms,

        // Wielojęzyczna kadra
        MultilingualStaff,

        // Inne
        Fireplace,
        SelfCheckIn,
        EcoFriendly
    }

    public enum ObjType
    {
        Hotel,
        Hostel,
        Apartment,
        Penthouse,
        House,
        Willa,
        Pension,
    }

    public class Address
    {
        public int Id { get; set; }
        public string Locale { get; set; } = null!;
        public string HouseNumber { get; set; } = null!;
        public string Road { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!; 
        
        public ulong? HotelId { get; set; }
        [JsonIgnore]
        public Hotel? Hotel { get; set; }

        public ulong? RestaurantId { get; set; }
        [JsonIgnore]
        public Restaurant? Restaurant { get; set; }
    }

    public class HotelImage
    {
        public ulong HotelImageId { get; set; }
        public ulong HotelId { get; set; }
        public string ImageSrc { get; set; } = null!;

        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; } = null!;
    }

    public class HRating
    {
        [Key]
        public ulong RatingId { get; set; }
        public ulong UserId { get; set; }
        public ulong HotelId { get; set; }
        private uint _rate;
        public uint Rate
        {
            get { return _rate; }
            set
            {
                if (value < 1)
                    _rate = 5;
                else if (value > 5)
                    _rate = 5;
                else
                    _rate = value;
            }
        }
    }

    //Obiekt hotelu
    public class Hotel
    {
        [Key]
        public ulong HotelId { get; set; }
        public string Name { get; set; } = null!;
        public string Lat { get; set; } = null!;
        public string Lon { get; set; } = null!;
        public List<Address> Addresses { get; set; } = [];
        public Currency Currency { get; set; }
        public TimeOnly CheckIn { get; set; }
        public TimeOnly CheckOut { get; set; }
        public ObjType Type { get; set; }
        public string Description { get; set; } = null!;
        public List<FacilityHotel> Facilities { get; set; } = [];
        public ICollection<HotelImage> Images { get; set; } = []; //zdjęcia przestrzeni wspólnych

        [Required]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        [JsonIgnore]
        public User OwnerUser { get; set; } = null!;

        public ICollection<Room> Rooms { get; } = [];
        public ICollection<HRating> Ratings { get; set; } = [];

        [NotMapped]
        public double? Rating => Ratings.Count != 0 ? Ratings.Average(r => r.Rate) : null;
    }
}