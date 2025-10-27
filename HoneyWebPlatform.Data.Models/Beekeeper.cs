namespace HoneyWebPlatform.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static Common.EntityValidationConstants.Beekeeper;

    public class Beekeeper
    {
        public Beekeeper()
        {
            Id = Guid.NewGuid();
            OwnedHoney = new HashSet<Honey>();
            OwnedPropolis = new HashSet<Propolis>();
            OwnedBeePollen = new HashSet<BeePollen>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(PhoneNumberMaxLength)]
        public string PhoneNumber { get; set; } = null!;
        
        [MaxLength(255)]
        public string? HiveFarmPicturePaths { get; set; }

        // Temporarily commented out until database migration is applied
        // [MaxLength(500)]
        // public string? Story { get; set; }

        // [MaxLength(100)]
        // public string? Region { get; set; }

        // public int? NumberOfHives { get; set; }

        // [MaxLength(100)]
        // public string? ExperienceYears { get; set; }

        // [MaxLength(500)]
        // public string? Specialties { get; set; }

        public Guid UserId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        // Map coordinates for interactive map feature
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }

        public virtual ICollection<Honey> OwnedHoney { get; set; }

        public virtual ICollection<Propolis> OwnedPropolis { get; set; }

        public virtual ICollection<BeePollen> OwnedBeePollen { get; set; }
    }
}
