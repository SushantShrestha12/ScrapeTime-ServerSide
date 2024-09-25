using Newtonsoft.Json;
using ScrapeTime.Presentation.Services;

namespace ScrapeTime.Presentation.Contracts;

public class InstagramCreate
{
        public string? Url { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public string? Age { get; set; }
}

public class LocationInfo
{
        public string? CountryName { get; set; }
        public List<CityInfo>? Cities { get; set; }
}


public class CityInfo
{
        public string? CityName { get; set; }
        public List<LocationDetail>? Locations { get; set; }
}

public class LocationDetail
{
        public string? LocationName { get; set; }
        public string? LocationId { get; set; }
}

public class NamSorGenderResponse
{
        [JsonProperty("likelyGender")]
        public string? LikelyGender { get; set; }
}
