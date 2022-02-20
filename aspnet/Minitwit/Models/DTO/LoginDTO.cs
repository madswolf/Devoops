using System.ComponentModel.DataAnnotations;

namespace Minitwit.Models.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "A username is required")]
        public string username { get; set; }

        [Required(ErrorMessage = "A password is required")]
        public string pwd { get; set; }
        public bool rememberMe { get; set; }
    }
}
