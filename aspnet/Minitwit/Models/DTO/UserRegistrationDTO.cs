using System.ComponentModel.DataAnnotations;

namespace Minitwit.Models.DTO
{

        public class UserRegistrationDTO
        {
            [Required(ErrorMessage = "A username is required")]
            public string username { get; set; }

            [Required(ErrorMessage = "A password is required")]
            public string pwd{ get; set; }
            [Compare("pwd", ErrorMessage = "The password and confirmation password do not match.")]
            public string pwdrepeat { get; set; }

            [Display(Name = "Email address")]
            [Required(ErrorMessage = "The email address is required")]
            [EmailAddress(ErrorMessage = "Invalid Email Address")]
            public string email { get; set; }
            public bool rememberMe { get; set; }
    }

}
