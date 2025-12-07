namespace HoneyWebPlatform.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static Common.EntityValidationConstants.Post;

    public class Post
    {
        public Post()
        {
            Id = Guid.NewGuid();
            Comments = new HashSet<Comment>();
            IsActive = true; //default value
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(TitleMaxLength)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(ContentMaxLength)]
        public string Content { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsActive { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public Guid AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; } = null!;
    }
}