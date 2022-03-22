using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minitwit.Repositories;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using static System.Int32;

namespace Minitwit.Controllers
{
    public class SimulatorController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ILatestRepository _latestRepository;

        private readonly UserManager<User> _userManager;
        
        private const string simulatorAPIToken = "c2ltdWxhdG9yOnN1cGVyX3NhZmUh";

        public SimulatorController(IUserRepository userRepository, 
            UserManager<User> userManager, IMessageRepository messageRepository, ILatestRepository latestRepository)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _latestRepository = latestRepository;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("[Controller]/latest")]
        [AllowAnonymous]
        public async Task<IActionResult> Latest()
        {
            return Ok(new
            {
                latest = (await _latestRepository.GetLatest()).Value
            });
        }

        [HttpPost]
        [Route("[Controller]/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] SimulatorUserRegistrationDTO userDTO)
        {
            await UpdateLatestAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values);
            }

            var user = new User()
            {
                UserName = userDTO.username,
                Email = userDTO.email
            };

            var result = await _userManager.CreateAsync(user, userDTO.pwd);
            
            if (result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName") != null) return StatusCode(400);
            if (!result.Succeeded) return BadRequest(result);
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
                var filteredMessages = await _messageRepository.GetFilteredMessages(limit);

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

            var user = await _userRepository.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound($"User with name {username} not found");
            }

            var filteredMessages = await _messageRepository.GetFilteredMessagesByAuthorId(user.Id);

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

            var user = await _userRepository.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound($"User with name {username} not found");
            }

            Console.Write("-------------------------------");
            await _messageRepository.InsertMessage(new Message()
            {
                Text = messageDTO.content,
                Author = user,
                PublishDate = DateTime.UtcNow
            });

            return StatusCode(204);

        }

        [HttpGet]
        [Route("[controller]/fllws/{UserName}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowsFromUser(string username, [FromQuery(Name = "no")] int limit = 100)
        {
            await UpdateLatestAsync();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var follower = await _userRepository.GetUserByUsername(username);
            if (follower == null) return NotFound(username);

            var follows = await _userRepository.GetFilteredFollows(username);
            
            return Ok(follows);
        }

        [HttpPost]
        [Route("[controller]/fllws/{UserName}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowAsUser(string username, [FromBody] SimulatorFollowOrUnfollowDTO followDTO)
        {
            await UpdateLatestAsync();
            if (!IsRequestFromSimulator()) return Unauthorized();

            var follower = await _userRepository.GetUserByUsername(username);
            if (follower == null) return NotFound(username);
            

            if (followDTO?.follow != null)
            {
                var followee = await _userRepository.GetUserByUsername(followDTO.follow);
                if (followee == null) return NotFound(followDTO.follow);
                var following = (await _userRepository.GetFollow(follower.Id, followee.Id)) is not null;
                if (following) return StatusCode(204);

                await _userRepository.Follow(follower.Id, followee.Id);
            }
            else if (followDTO?.unfollow != null)
            {
                var unFollowee = await _userRepository.GetUserByUsername(followDTO.unfollow);
                if (unFollowee == null) return NotFound(followDTO.unfollow);
                var following = await _userRepository.GetFollow(follower.Id, unFollowee.Id);
                if (following == null) return StatusCode(204);


                await _userRepository.Unfollow(following);
            }

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
                await _latestRepository.InsertLatest(latest);
            }
            // dumb return value to allow for awaiting this function
            return 1;
        }
    }
}
