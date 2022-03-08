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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IOptions<AppsettingsConfig> config;

        public MinitwitController(MinitwitContext context, IOptions<AppsettingsConfig> config,
            IUserRepository userRepository, IMessageRepository messageRepository)
        {
            this.config = config;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }


        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Private_Timeline()
        {
            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId == null) return RedirectToAction(nameof(Public_Timeline));
            var userId = int.Parse(userStringId);

            var follows = await _userRepository.GetUserFollows(userId);

            var posts = await _messageRepository.GetPrivateTimeline(userId, follows);

            ViewData["Messages"] = posts;
            return View();
        }

        [HttpGet]
        [Route("/public")]
        public async Task<IActionResult> Public_Timeline()
        {

            var posts = await _messageRepository.GetMessages();

            ViewData["Messages"] = posts;
            return View();
        }

        [HttpGet]
        [Route("[controller]/{username}")]
        public async Task<IActionResult> User_Timeline(string username)
        {
            var user = await _userRepository.GetUserByUsername(username);
            if (user == null) return NotFound($"UserName with name {username} not found");

            var posts = await _messageRepository.GetMessagesByAuthorId(user.Id);

            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId != null)
            {
                var loggedInUserId = int.Parse(userStringId);
                var IsFollowing = (await _userRepository.GetFollow(loggedInUserId, user.Id)) is not null;
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

            var followee = await _userRepository.GetUserByUsername(username);

            if (followee == null) return NotFound($"User with name {username} not found");
            
            if ((await _userRepository.GetFollow(userId, followee.Id)) is not null)
                return Conflict($"User is already following {username}");
            
            await _userRepository.Follow(userId, followee.Id);

            return RedirectToAction(username, "Minitwit");
        }


        [HttpGet]
        [Route("[Controller]/{username}/unFollow")]
        public async Task<IActionResult> UnFollow(string username)
        {
            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId == null) return RedirectToAction("login", "Users");
            var userId = int.Parse(userStringId);

            var followee = await _userRepository.GetUserByUsername(username);
            if (followee == null) return NotFound($"User with name {username} not found");

            var follow = await _userRepository.GetFollow(userId, followee.Id);

            if (follow == null) return NotFound();

            await _userRepository.Unfollow(follow);

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

            await _messageRepository.InsertMessage(newMessage);

            return RedirectToAction(nameof(Public_Timeline), "Minitwit");
        }

        [HttpPost]
        [Route("[controller]/FlagMessage")]
        [AllowAnonymous]
        public async Task<IActionResult> FlagMessage([FromBody] FlagMessageDTO flagDTO)
        {

            if (!ModelState.IsValid) return BadRequest(flagDTO);
            if (!IsRequestFromModerator()) return Unauthorized();

            if (await _messageRepository.GetMessage(flagDTO.MessageId) == null)
                return NotFound(flagDTO.MessageId);

            await _messageRepository.FlagMessage(flagDTO.MessageId, flagDTO.Flagged);

            return Ok();
        }

        private bool IsRequestFromModerator()
        {
            return Request.Headers["Authorization"].Equals($"Basic {config.Value.ModeratorAPIKey}");
        }
    }
}
