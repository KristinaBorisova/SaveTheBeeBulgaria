namespace HoneyWebPlatform.Web.ViewModels.Beekeeper
{
    public class AllBeekeepersViewModel
    {
        public IEnumerable<BeekeeperCardViewModel> Beekeepers { get; set; } = new List<BeekeeperCardViewModel>();
    }

    public class BeekeeperCardViewModel
    {
        public string Id { get; set; } = null!;
        
        public string FullName { get; set; } = null!;
        
        public string Email { get; set; } = null!;
        
        public string PhoneNumber { get; set; } = null!;
        
        public string? HivePicturePath { get; set; }
        
        public int HoneyCount { get; set; }
        
        public int PropolisCount { get; set; }
        
        public DateTime JoinedDate { get; set; }
        
        public string? Location { get; set; }
        
        public string? Bio { get; set; }
        
        public double AverageRating { get; set; }
        
        public int TotalOrders { get; set; }
        
        // Map coordinates for interactive map
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
    }
}
