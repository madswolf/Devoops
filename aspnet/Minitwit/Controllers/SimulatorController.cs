using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Minitwit.DatabaseUtil;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using static System.Int32;

namespace Minitwit.Controllers
{
    public class SimulatorController : Controller
    {
        private readonly MinitwitContext _context;
        private readonly UsersController _usersController;
        private readonly EntityAccessor _entityAccessor;
        private const string simulatorAPIToken = "c2ltdWxhdG9yOnN1cGVyX3NhZmUh";

        public SimulatorController(MinitwitContext context, UsersController usersController, EntityAccessor entityAccessor)
        {
            _context = context;
            _usersController = usersController;
            _entityAccessor = entityAccessor;
        }

        private bool IsRequestFromSimulator()
        {
            return Request.Headers["Authorization"].Equals($"Basic {simulatorAPIToken}");
        }

        private async void UpdateLatest()
        {
            var isLatestInQuery = Request.Query.TryGetValue("latest",out var latestString);
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

        [HttpGet]
        [Route("/latest")]
        [AllowAnonymous]
        public async Task<IActionResult> Latest()
        {
            
            return Ok(new
            {
                latest =
                    _context.Latest
                        .OrderByDescending(l => l.CreationTime)
                        .FirstOrDefault()
            });
        }

        [HttpPost]
        [Route("/regitster")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserCreationDTO userDTO)
        {
            UpdateLatest();
            return await _usersController.Create(userDTO);
        }

        [HttpGet]
        [Route("/msgs")]
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
                        user = p.Author.Username
                    });

                return Ok(filteredMessages);
            }

            return Unauthorized();
        }

        [HttpGet]
        [Route("/msgs/{Username}")]
        [AllowAnonymous]
        public async Task<IActionResult> MessagesFromUser(string username, [FromQuery(Name = "no")] int limit = 100)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var filteredMessages = await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged && p.Author.Username == username)
                .OrderByDescending(p => p.PublishDate)
                .Take(limit)
                .Select(p => new
                {
                    content = p.Text,
                    pub_date = p.PublishDate,
                    user = p.Author.Username
                })
                .ToListAsync();

            return Ok(filteredMessages);

        }

        [HttpPost]
        [Route("/msgs/{Username}")]
        [AllowAnonymous]
        public async Task<IActionResult> MessagesAsUser(string username, [FromBody] SimulatorMessageCreationDTO messageDTO)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var user = await _entityAccessor.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound(username);
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
        [Route("/fllws/{Username}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowsFromUser(string username, [FromQuery(Name = "no")] int limit = 100)
        {
            UpdateLatest();
            if (!IsRequestFromSimulator()) return Unauthorized();
            
            var filteredMessages = await _context.Users
                .Include(u => u.Follows)
                .Where(u => u.Username == username)
                .Take(limit)
                .Select(u => new
                {
                    follows = u.Follows.Select(u2 => u2.Username)
                })
                .ToListAsync();

            return Ok(filteredMessages);
        }

        [HttpPost]
        [Route("/msgs/{Username}")]
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
    }
}
