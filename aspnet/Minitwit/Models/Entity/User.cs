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
            this.Follows = new List<Follow>();
            this.FollowedBy = new List<Follow>();
            this.Messages = new List<Message>();
        }
        public List<Follow> Follows { get; set; }
        public List<Follow> FollowedBy { get; set; }
        public List<Message> Messages { get; set; }
    }
}
