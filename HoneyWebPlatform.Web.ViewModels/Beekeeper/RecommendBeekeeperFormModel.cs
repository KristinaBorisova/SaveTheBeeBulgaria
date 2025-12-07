namespace HoneyWebPlatform.Web.ViewModels.Beekeeper
{
    using System.ComponentModel.DataAnnotations;

    using static Common.EntityValidationConstants.Beekeeper;

    public class RecommendBeekeeperFormModel
    {
        [Required(ErrorMessage = "Името на пчелара е задължително")]
        [Display(Name = "Име на пчелар")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Името трябва да бъде между 2 и 100 символа")]
        public string BeekeeperName { get; set; } = null!;

        [Required(ErrorMessage = "Номерът за връзка е задължителен")]
        [Display(Name = "Номер за връзка")]
        [StringLength(PhoneNumberMaxLength, MinimumLength = 5, ErrorMessage = "Невалиден телефонен номер")]
        public string ContactNumber { get; set; } = null!;

        [Required(ErrorMessage = "Регионът е задължителен")]
        [Display(Name = "Регион")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Регионът трябва да бъде между 2 и 100 символа")]
        public string Region { get; set; } = null!;

        [Required(ErrorMessage = "Моля, обяснете защо препоръчвате този пчелар")]
        [Display(Name = "Защо ни препоръчвате този медопроизводител?")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Описанието трябва да бъде между 10 и 1000 символа")]
        public string RecommendationReason { get; set; } = null!;

        [Required(ErrorMessage = "Вашето име е задължително")]
        [Display(Name = "Вашето име")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Името трябва да бъде между 2 и 50 символа")]
        public string RecommenderName { get; set; } = null!;

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [Display(Name = "Имейл за обратна връзка")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        [StringLength(100, ErrorMessage = "Имейлът не може да бъде повече от 100 символа")]
        public string RecommenderEmail { get; set; } = null!;

        [Display(Name = "Брой пчелни семейства")]
        public string? NumberOfColonies { get; set; }
    }
}

