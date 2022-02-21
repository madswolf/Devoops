using Microsoft.Build.Framework;

namespace Minitwit.Models.Entity
{
    public class Follow
    {
        [Required]
        public int FollowerId { get; set; }
        public User Follower { get; set; }
        [Required]
        public int FolloweeId{ get; set; }
        public User Followee { get; set; }
    }
}
