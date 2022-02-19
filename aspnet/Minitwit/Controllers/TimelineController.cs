using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Services;

namespace Minitwit.Controllers
{
    public class TimelineController : Controller
    {
        private readonly MinitwitContext _context;
        private readonly IUserService _userService;
        private const int PER_PAGE = 30;

        public TimelineController(MinitwitContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }


        [HttpGet]
        [Route("[controller]/timeline")]
        public async Task<IActionResult> PrivateTimeline(int id)
        {
            //Todo get the id from authentication

            //Todo Redirect to public timeline
            if (!UserExists(id)) return RedirectToAction(nameof(PublicTimeline));

            var postsAndFollows = await _context.Users
                .Include(u => u.Messages)
                .Include(u => u.Follows)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Messages,
                    u.Follows
                })
                .FirstOrDefaultAsync();

            //Almost certain you need this subquery to find the posts of the people the user follows
            var followsPosts = await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged)
                .Where(p => postsAndFollows.Follows.Exists(u => u.Id == p.Author.Id))
                .Concat(postsAndFollows.Messages)
                .OrderByDescending(p => p.PublishDate)
                .Take(PER_PAGE)
                .ToListAsync();

            return Ok(followsPosts);
        }

        [HttpGet]
        [Route("[controller]/timeline/public")]
        public async Task<IActionResult> PublicTimeline()
        {

            var posts = await _context.Posts
                .Where(p => !p.Flagged)
                .OrderByDescending(p => p.PublishDate)
                .Take(PER_PAGE)
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet]
        [Route("[controller]/timeline/{username}")]
        public async Task<IActionResult> UserTimeline(string username)
        {
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .FirstOrDefaultAsync();
            if (user == null) return NotFound($"UserName with name {username} not found");

            var postsAndIsFollowing = await _context.Users
                .Include(u => u.Messages)
                .Include(u => u.Follows)
                .Where(u => u.UserName == username)
                .Select(u => new
                    {
                        messages = u.Messages
                            .OrderByDescending(p => p.PublishDate)
                            .Take(PER_PAGE),
                        follows = u.Follows.Exists(u => u.Id == user.Id)
                    }
                )
                .FirstOrDefaultAsync();

            return Ok(new
            {
                user,
                postsAndIsFollowing.messages,
                postsAndIsFollowing.follows
            });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
