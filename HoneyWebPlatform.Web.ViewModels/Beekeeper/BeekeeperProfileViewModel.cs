namespace HoneyWebPlatform.Web.ViewModels.Beekeeper
{
    using Honey;

    public class BeekeeperProfileViewModel
    {
        public string Id { get; set; } = null!;
        
        public string FullName { get; set; } = null!;
        
        public string Email { get; set; } = null!;
        
        public string PhoneNumber { get; set; } = null!;
        
        public string? Story { get; set; }
        
        public string? Region { get; set; }
        
        public int? NumberOfHives { get; set; }
        
        public string? ExperienceYears { get; set; }
        
        public string? Specialties { get; set; }
        
        public string? HiveFarmPicturePaths { get; set; }
        
        public IEnumerable<HoneyAllViewModel> OwnedHoneys { get; set; } = new List<HoneyAllViewModel>();
        
        public int TotalHoneys { get; set; }
        
        public DateTime JoinedDate { get; set; }
    }
}
