namespace HoneyWebPlatform.Web.ViewModels.Beekeeper
{
    public class BeekeepersMapViewModel
    {
        public IEnumerable<BeekeeperMapMarker> Beekeepers { get; set; } = new List<BeekeeperMapMarker>();
        
        public string GoogleMapsApiKey { get; set; } = string.Empty;

        public RecommendBeekeeperFormModel RecommendationForm { get; set; } = new RecommendBeekeeperFormModel();
    }

    public class BeekeeperMapMarker
    {
        public string Id { get; set; } = null!;
        
        public string FullName { get; set; } = null!;
        
        public string? Region { get; set; }
        
        public string? ShopLocation { get; set; }
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
        
        public IEnumerable<string> HoneyTypes { get; set; } = new List<string>();
        
        public string ProfileUrl { get; set; } = string.Empty;
    }
}

