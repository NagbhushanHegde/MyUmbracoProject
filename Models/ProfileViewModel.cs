using System.ComponentModel.DataAnnotations;

namespace MyUmbracoProject.Models
{
    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}