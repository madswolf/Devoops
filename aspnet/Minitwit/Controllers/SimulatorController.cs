using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Minitwit.DatabaseUtil;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using Minitwit.Services;
using static System.Int32;

namespace Minitwit.Controllers
{
    public class SimulatorController : Controller
    {
        private readonly MinitwitContext _context;
        private readonly IEntityAccessor _entityAccessor;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private const string simulatorAPIToken = "c2ltdWxhdG9yOnN1cGVyX3NhZmUh";

        public SimulatorController(MinitwitContext context, IEntityAccessor entityAccessor, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _entityAccessor = entityAccessor;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("[Controller]/latest")]
        [AllowAnonymous]
        public async Task<IActionResult> Latest()
        {
            
            return Ok(new
            {
                latest =
                    _context.Latest
                        .OrderByDescending(l => l.CreationTime)
                        .Select(l => l.Value)
                        .FirstOrDefault()
                    
            });
        }

        [HttpPost]
        [Route("[Controller]/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userDTO)
        {
            UpdateLatest();
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = new User()
            {
                UserName = userDTO.username,
                Email = userDTO.email
            };

            var result = await _userManager.CreateAsync(user, userDTO.pwd);
            if (!result.Succeeded) return BadRequest(result);
            await _signInManager.SignInAsync(user, false);
            return Ok();
        }

        [HttpGet]
        [Route("[controller]/msgs")]
        [AllowAnonymous]
        public async Task<IActionResult> Messages([FromQuery(Name = "no")] int limit = 100)
        {
            UpdateLatest();
            if (IsRequestFromSimulator())
            {
                var filteredMessages = _context.Posts
                    .Include(p => p.Author)
                    .Where(p => !p.Flagged)
                    .OrderByDescending(p => p.PublishDate)
                    .Take(limit)
                    .Select(p => new
                    {
                        content = p.Text,
                        pub_date = p.PublishDate,
                        user = p.Author.UserName
                    });

                return Ok(filteredMessages);
            }

            return Unauthorized();
        }

        [Route("[controller]/msgs/{username}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MessagesFromUser(string username, [FromQuery(Name = "no")] int limit = 100)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var filteredMessages = await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged && p.Author.UserName == username)
                .OrderByDescending(p => p.PublishDate)
                .Take(limit)
                .Select(p => new
                {
                    content = p.Text,
                    pub_date = p.PublishDate,
                    user = p.Author.UserName
                })
                .ToListAsync();

            return Ok(filteredMessages);

        }

        [HttpPost]
        [Route("[controller]/msgs/{UserName}")]
        [AllowAnonymous]
        public async Task<IActionResult> MessagesAsUser(string username, [FromBody] SimulatorMessageCreationDTO messageDTO)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = await _entityAccessor.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound($"User with name {username} not found");
            }

            _context.Posts.Add(new Message()
            {
                Text = messageDTO.content,
                Author = user,
                PublishDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return StatusCode(204);

        }

        [HttpGet]
        [Route("[controller]/fllws/{UserName}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowsFromUser(string username, [FromQuery(Name = "no")] int limit = 100)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();
            
            var filteredMessages = await _context.Users
                .Include(u => u.Follows)
                .Where(u => u.UserName == username)
                .Select(u => new
                {
                    follows = u.Follows
                        .Select(u2 => u2.UserName)
                        .Take(limit)
                })
                .ToListAsync();

            return Ok(filteredMessages);
        }

        [HttpPost]
        [Route("[controller]/fllws/{UserName}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowAsUser(string username, [FromBody] SimulatorFollowOrUnfollowDTO followDTO)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var follower = await _entityAccessor.GetUserByUsername(username);
            if (follower == null) return NotFound(username);
            

            if (followDTO?.follow != null)
            {
                var followee = await _entityAccessor.GetUserByUsername(followDTO.follow);
                if (followee == null) return NotFound(followDTO.follow);
                follower.Follows.Add(followee);
                followee.Follows.Add(follower);
            }
            else if (followDTO?.unfollow != null)
            {
                var unFollowee = await _entityAccessor.GetUserByUsername(followDTO.unfollow);
                if (unFollowee == null) return NotFound(followDTO.unfollow);
                follower.Follows.Remove(unFollowee);
                unFollowee.Follows.Remove(follower);
            }


            await _context.SaveChangesAsync();
            return StatusCode(204);
        }
        private bool IsRequestFromSimulator()
        {
            return Request.Headers["Authorization"].Equals($"Basic {simulatorAPIToken}");
        }

        private async void UpdateLatest()
        {
            var isLatestInQuery = Request.Query.TryGetValue("latest", out var latestString);
            if (isLatestInQuery)
            {
                var latest = new Latest()
                {
                    Value = Parse(latestString),
                    CreationTime = DateTime.UtcNow
                };
                _context.Latest.Add(latest);
                await _context.SaveChangesAsync();
            }
        }
    }
}
