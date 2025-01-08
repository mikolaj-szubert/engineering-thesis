using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MTM_Web_App.Server.Models
{
    public enum Cusine
    {
        Polish,
        Italian,
        Chinese,
        Indian,
        Mexican,
        French,
        Japanese,
        Thai,
        Spanish,
        Greek,
        American,
        Moroccan,
        Lebanese,
        Brazilian,
        Turkish,
        Russian,
        Indonesian,
        Vietnamese,
        Korean,
        Ethiopian,
        Filipino,
        Malaysian,
        Scandinavian,
        German,
        Dutch,
        British,
        South_African,
        Pakistani,
        Peruvian,
        Australian,
        Argentine,
        Egyptian,
        Hungarian,
        Irish,
        Caribbean,
        Bangladeshi,
        Ukrainian,
        Jewish,
        Basque,
        Finnish,
        Chilean,
        Afghan,
        Tibetan,
        Cambodian,
        Singaporean,
        Sri_Lankan,
        Icelandic,
        Scottish,
        Maltese,
        Belarusian,
        Welsh,
        Zambian,
        Omani,
        Kuwaiti,
        Syrian,
        Albanian,
        Bantu,
        Hawaiian,
        Uzbek,
        Azerbaijani,
        Georgian,
        Estonian,
        Latvian,
        Lithuanian,
        Bhutanese,
        Samoan,
        Tongan,
        Fijian,
        Marshallese,
        Papua_New_Guinean,
        Micronesian,
        Greenlandic,
        Andorran,
        Surinamese,
        Guyanese,
        Trinidadian,
        Mauritian,
        Seychellois,
        Tatar,
        Chuvash,
        Yakut,
        Buryat,
        Breton,
        Corsican,
        Catalan,
        Walloon,
        Aragonese,
        Galician,
        Moldovan,
        Panamanian,
        Salvadoran,
        Nicaraguan,
        Costa_Rican,
        Dominican,
        Paraguayan,
        Uruguayan,
        Ecuadorean,
        Bolivian,
        Venezuelan,
        Haitian,
        Jamaican,
        Northern_Irish,
        Turkish_Cypriot,
        Greek_Cypriot
    }
    public class OpeningHours
    {
        [Key]
        public int OpeningHoursId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }

        [ForeignKey("Restaurant")]
        public ulong RestaurantId { get; set; }
        [JsonIgnore]
        public Restaurant Restaurant { get; set; } = null!;
    }

    public class RRating
    {
        [Key]
        public ulong RatingId { get; set; }
        public ulong UserId { get; set; }
        public ulong RestaurantId { get; set; }
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

    public class RestaurantImage
    {
        public ulong RestaurantImageId { get; set; }
        public ulong RestaurantId { get; set; }
        public string ImageSrc { get; set; } = null!;

        [ForeignKey("RestaurantId")]
        public Restaurant Restaurant { get; set; } = null!;
    }

    public class Restaurant
    {
        [Key]
        public ulong RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string Lat { get; set; } = null!;
        public string Lon { get; set; } = null!;
        public List<Address> Addresses { get; set; } = [];
        public Currency Currency { get; set; }
        public List<Cusine> Cusines { get; set; } = [];
        public string Description { get; set; } = null!;
        public ICollection<RestaurantImage> Images { get; set; } = []; //zdjęcia przestrzeni wspólnych

        public List<OpeningHours> OpenDays { get; set; } = [];

        [Required]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        [JsonIgnore]
        public User OwnerUser { get; set; } = null!;

        public ICollection<Table> Tables { get; } = [];
        public ICollection<RRating> Ratings { get; set; } = [];

        [NotMapped]
        public double? Rating => Ratings.Count != 0 ? Ratings.Average(r => r.Rate) : null;
    }

}