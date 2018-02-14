namespace DemoBlog.Mvc.Models.AccountViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
