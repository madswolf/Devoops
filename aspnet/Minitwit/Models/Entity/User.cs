using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Minitwit.Models.Entity
{
    public class User : IdentityUser<int>
    {
        public List<User> Follows { get; set; }
        public List<User> FollowedBy { get; set; }
        public List<Message> Messages { get; set; }
    }
}
