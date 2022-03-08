using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minitwit.DatabaseUtil;
using Minitwit.Models;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.Controllers
{
    public class MinitwitController : Controller
    {
        private readonly MinitwitContext _context;
        private readonly IUserRepository _userAccessor;
        private readonly IOptions<AppsettingsConfig> config;
        private const int PER_PAGE = 30;

        public MinitwitController(MinitwitContext context, IOptions<AppsettingsConfig> config, IEntityAccessor entityAccessor,
            IUserRepository userAccessor)
        {
            _context = context;
            this.config = config;
            _userAccessor = userAccessor;
        }


        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Private_Timeline()
        {
            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId == null) return RedirectToAction(nameof(Public_Timeline));
            var userId = int.Parse(userStringId);

            var follows = await _userAccessor.GetUserFollows(userId);

            var posts = await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged)
                .Where(p => 
                    follows.Contains(p.AuthorId)
                    || p.AuthorId == userId
                    )                            
                .OrderByDescending(p => p.PublishDate)
                .Take(PER_PAGE)
                .ToListAsync();

            ViewData["Messages"] = posts;
            return View();
        }

        [HttpGet]
        [Route("/public")]
        public async Task<IActionResult> Public_Timeline()
        {

            var posts = await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged)
                .OrderByDescending(p => p.PublishDate)
                .Take(PER_PAGE)
                .ToListAsync();

            ViewData["Messages"] = posts;
            return View();
        }

        [HttpGet]
        [Route("[controller]/{username}")]
        public async Task<IActionResult> User_Timeline(string username)
        {
            var user = await _userAccessor.GetUserByUsername(username);
            if (user == null) return NotFound($"UserName with name {username} not found");

            var posts = await _context.Users
                .Include(u => u.Messages)
                .Where(u => u.Id == user.Id)
                .SelectMany(u => 
                             u.Messages
                            .OrderByDescending(p => p.PublishDate)
                            .Where(p => !p.Flagged)
                            .Take(PER_PAGE)
                )
                .ToListAsync();

            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId != null)
            {
                var loggedInUserId = int.Parse(userStringId);
                var IsFollowing = _context.Follows.Contains(new Follow
                    { FolloweeId = user.Id, FollowerId = loggedInUserId });
                ViewData["IsFollowing"] = IsFollowing;
            }

            ViewData["ViewedUserName"] = username;
            ViewData["Messages"] = posts;

            return View();
        }

        [HttpGet]
        [Route("/{username}/Follow")]
        public async Task<IActionResult> Follow(string username)
        {
            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId == null) return RedirectToAction("login", "Users");
            var userId = int.Parse(userStringId);

            var followee = await _userAccessor.GetUserByUsername(username);

            if (followee == null) return NotFound($"User with name {username} not found");
            if (_context.Follows.Any(f => f.FolloweeId == followee.Id && f.FollowerId == userId))
                return Conflict($"User is already following {username}");
            
            await _userAccessor.Follow(userId, followee.Id);

            return RedirectToAction(username, "Minitwit");
        }


        [HttpGet]
        [Route("[Controller]/{username}/unFollow")]
        public async Task<IActionResult> UnFollow(string username)
        {
            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId == null) return RedirectToAction("login", "Users");
            var userId = int.Parse(userStringId);

            var followee = await _userAccessor.GetUserByUsername(username);
            if (followee == null) return NotFound($"User with name {username} not found");

            var follow = await _userAccessor.GetFollow(userId, followee.Id);

            if (follow == null) return NotFound();

            await _userAccessor.Unfollow(follow);

            return RedirectToAction(username, "Minitwit");
        }

        [HttpPost]
        [Route("[controller]/PostMessage")]
        public async Task<IActionResult> PostMessage(MessageCreationDTO message)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login", "Users");

            if (!ModelState.IsValid) return BadRequest("Text is required");

            Message newMessage = new Message()
            {
                AuthorId = int.Parse(userId),
                Text = message.Text,
                PublishDate = DateTime.UtcNow
            };

            _context.Posts.Add(newMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Public_Timeline), "Minitwit");
        }

        [HttpPost]
        [Route("[controller]/FlagMessage")]
        [AllowAnonymous]
        public async Task<IActionResult> FlagMessage([FromBody] FlagMessageDTO flagDTO)
        {

            if (!ModelState.IsValid) return BadRequest(flagDTO);
            if (!IsRequestFromModerator()) return Unauthorized();

            var message = await _context.Posts.FirstOrDefaultAsync(m => m.Id == flagDTO.MessageId);
            if (message == null) return NotFound(flagDTO.MessageId);
            message.Flagged = flagDTO.Flagged;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool IsRequestFromModerator()
        {
            return Request.Headers["Authorization"].Equals($"Basic {config.Value.ModeratorAPIKey}");
        }
    }
}
