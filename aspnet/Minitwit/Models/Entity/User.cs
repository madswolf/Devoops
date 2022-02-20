using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Minitwit.Models.Entity
{
    public class User : IdentityUser<int>
    {
        public User() 
        {
            this.Follows = new List<User>();
            this.FollowedBy = new List<User>();
            this.Messages = new List<Message>();
        }
        public List<User> Follows { get; set; }
        public List<User> FollowedBy { get; set; }
        public List<Message> Messages { get; set; }
    }
}
