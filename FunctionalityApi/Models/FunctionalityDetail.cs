using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FunctionalityApi.Models
{
    public class FunctionalityDetail
    {
        public int Id { get; set; }
        public string Channel { get; set; }
        public AvailabilityDetail Availability { get; set; }
        public Config Config { get; set; }
    }

    public class AvailabilityDetail
    {
        [JsonPropertyName("business_hours")]
        public BusinessHours BusinessHours { get; set; }

        [JsonPropertyName("out_of_service_list")]
        public ICollection<string> OutOfServiceList { get; set; }
    }

    public class BusinessHours
    {
        [JsonPropertyName("includes_holidays")]
        public bool IncludeHoliday { get; set; }
        public IList<WeekdayDetail> Includes { get; set; }
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Title { get; set; }
        public string Detail { get; set; }
    }

    public class WeekdayDetail
    {
        public string Weekday { get; set; }

        [JsonPropertyName("from_hour")]
        public string FromHour { get; set; }

        [JsonPropertyName("to_hour")]
        public string ToHour { get; set; }
    }

    public class Config
    {
        public Security Security { get; set; }
    }

    public class Security
    {
        [JsonPropertyName("scope_level")]
        public string ScopeLevel { get; set; }
    }
}
