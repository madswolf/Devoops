using System.Net;
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
                    await _context.Latest
                        .OrderByDescending(l => l.CreationTime)
                        .Select(l => l.Value)
                        .FirstOrDefaultAsync()
                    
            });
        }

        [HttpPost]
        [Route("[Controller]/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] SimulatorUserRegistrationDTO userDTO)
        {
            await UpdateLatestAsync();
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = new User()
            {
                UserName = userDTO.username,
                Email = userDTO.email
            };

            if (_context.Users.Any(u => u.UserName == user.UserName))
                return StatusCode(400);

            var result = await _userManager.CreateAsync(user, userDTO.pwd);
            if (!result.Succeeded) return BadRequest(result);
            await _signInManager.SignInAsync(user, false);
            return StatusCode(204);
        }

        [HttpGet]
        [Route("[controller]/msgs")]
        [AllowAnonymous]
        public async Task<IActionResult> Messages([FromQuery(Name = "no")] int limit = 100)
        {
            await UpdateLatestAsync();
            if (IsRequestFromSimulator())
            {
                var filteredMessages = await _context.Posts
                    .Include(p => p.Author)
                    .Where(p => !p.Flagged)
                    .OrderByDescending(p => p.PublishDate)
                    .Take(limit)
                    .Select(p => new
                    {
                        content = p.Text,
                        pub_date = p.PublishDate,
                        user = p.Author.UserName
                    }).ToListAsync();

                return Ok(filteredMessages);
            }

            return Unauthorized();
        }

        [Route("[controller]/msgs/{username}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MessagesFromUser(string username, [FromQuery(Name = "no")] int limit = 100)
        {
            await UpdateLatestAsync();
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
            await UpdateLatestAsync();
            if (!IsRequestFromSimulator()) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = await _entityAccessor.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound($"User with name {username} not found");
            }

            Console.Write("-------------------------------");
            await _context.Posts.AddAsync(new Message()
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
            await UpdateLatestAsync();
            if (!IsRequestFromSimulator()) return Unauthorized();
            
            var follows = await _context.Users
                .Include(u => u.Follows)
                .Where(u => u.UserName == username)
                .Select(u => new
                {
                    follows = u.Follows
                        .Join(
                            _context.Users,
                            f => f.FolloweeId,
                            u => u.Id,
                            (f,u) => u
                            )
                        .Select(u2 => u2.UserName)
                        .Take(limit)
                })
                .FirstOrDefaultAsync();
            
            return Ok(follows);
        }

        [HttpPost]
        [Route("[controller]/fllws/{UserName}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowAsUser(string username, [FromBody] SimulatorFollowOrUnfollowDTO followDTO)
        {
            await UpdateLatestAsync();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var follower = await _entityAccessor.GetUserByUsername(username);
            if (follower == null) return NotFound(username);
            

            if (followDTO?.follow != null)
            {
                var followee = await _entityAccessor.GetUserByUsername(followDTO.follow);
                if (followee == null) return NotFound(followDTO.follow);
                var following = _context.Follows.Any(f => f.FolloweeId == followee.Id && f.FollowerId == follower.Id);
                if (following) return StatusCode(204);

                _context.Follows.Add(new Follow()
                {
                    FollowerId = follower.Id,
                    FolloweeId = followee.Id,
                });
            }
            else if (followDTO?.unfollow != null)
            {
                var unFollowee = await _entityAccessor.GetUserByUsername(followDTO.unfollow);
                if (unFollowee == null) return NotFound(followDTO.unfollow);

                var following = _context.Follows.Any(f => f.FolloweeId == unFollowee.Id && f.FollowerId == follower.Id);
                if (!following)
                    return StatusCode(204);
                ;

                _context.Follows.Remove(new Follow()
                {
                    FollowerId = follower.Id,
                    FolloweeId = unFollowee.Id
                });
            }


            var result = await _context.SaveChangesAsync();
            return StatusCode(204);
        }
        private bool IsRequestFromSimulator()
        {
            return Request.Headers["Authorization"].Equals($"Basic {simulatorAPIToken}");
        }

        private async Task<int> UpdateLatestAsync()
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
            // dumb return value to allow for awaiting this function
            return 1;
        }
    }
}
