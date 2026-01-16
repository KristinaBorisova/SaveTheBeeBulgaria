namespace HoneyWebPlatform.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class FortuneAccess
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(45)] // IPv6 max length
        public string IpAddress { get; set; } = null!;

        [Required]
        public DateTime LastAccessDate { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(500)] // Store the fortune text
        public string? FortuneText { get; set; }
    }
}

