using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minitwit.Models.Entity
{
    public class User
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Username { get; set; }
        public string PassWordHash { get; set; }
        public string Email { get; set; }
        public List<User> Follows { get; set; }
        public List<User> FollowedBy { get; set; }
        public List<Message> Messages { get; set; }
    }
}
