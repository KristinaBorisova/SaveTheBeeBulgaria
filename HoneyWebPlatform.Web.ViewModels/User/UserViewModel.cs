namespace HoneyWebPlatform.Web.ViewModels.User
{
    using System.ComponentModel.DataAnnotations;

    public class UserViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Моля, въведете вашия имейл.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Моля, въведете вашето пълно име.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Името трябва да е между 2 и 100 символа.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Моля, въведете телефонния си номер.")]
        [RegularExpression(@"^(\+359|0)[0-9]{9}$", ErrorMessage = "Телефонният номер трябва да започва с +359 или 0 и да съдържа 9 цифри след това.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Моля, въведете адрес за доставка.")]
        public string Address { get; set; } = null!;

        public bool IsSubscribed { get; set; } = false;
    }
}
