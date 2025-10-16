using System.ComponentModel.DataAnnotations;

namespace HoneyWebPlatform.Web.ViewModels.Home
{
    public class OrderFormViewModel
    {
        [Required(ErrorMessage = "Моля, въведете вашето пълно име.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Името трябва да е между 2 и 100 символа.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Моля, въведете вашия имейл.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Моля, въведете телефонния си номер.")]
        [RegularExpression(@"^(\+359|0)[0-9]{8,9}$", ErrorMessage = "Телефонният номер трябва да започва с +359 или 0 и да съдържа 8-9 цифри след това.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Моля, въведете адрес за доставка.")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Моля, изберете вид мед.")]
        public int HoneyTypeId { get; set; }

        public Guid? BeekeeperId { get; set; }

        [Required(ErrorMessage = "Моля, въведете количество.")]
        [Range(1, 10, ErrorMessage = "Количеството трябва да е между 1 и 10.")]
        public int Quantity { get; set; } = 1;

        public string? Notes { get; set; }

        // For dropdown population
        public IEnumerable<HoneyTypeViewModel> AvailableHoneyTypes { get; set; } = new List<HoneyTypeViewModel>();
        public IEnumerable<BeekeeperViewModel> AvailableBeekeepers { get; set; } = new List<BeekeeperViewModel>();
    }

    public class HoneyTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;
    }

    public class BeekeeperViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
