using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;

namespace Minitwit.Models.Entity
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string Salt { get; set; }
        [Required]
        public string Email { get; set; }
        public List<User> Follows { get; set; }
        public List<User> FollowedBy { get; set; }
        public List<Message> Messages { get; set; }
    }
}
