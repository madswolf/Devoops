#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using System.Security.Claims;

namespace Minitwit.Controllers
{
    public class UsersController : Controller
    {
        private readonly MinitwitContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        public UsersController(MinitwitContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("[controller]/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        [Route("[controller]/register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegistrationDTO userDTO)
        {
            TempData.Remove("ErrorMessage");
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            var user = new User()
            {
                UserName = userDTO.username,
                Email = userDTO.email
            };

            if (_context.Users.Any(u => u.UserName == user.UserName))
                TempData["ErrorMessage"] = $"User with {user.UserName} already exists";
                //return Conflict($"User with {user.UserName} already exists

            var result = await _userManager.CreateAsync(user, userDTO.pwd);
            if (!result.Succeeded && !TempData.ContainsKey("ErrorMessage"))
                TempData["ErrorMessage"] = result.ToString();

            if (TempData.ContainsKey("ErrorMessage"))
                return RedirectToAction("register", "Users");

            await _signInManager.SignInAsync(user, userDTO.rememberMe);
            return RedirectToAction("Private_Timeline","Minitwit");
        }

        [HttpGet]
        [Route("[controller]/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("[controller]/login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            TempData.Remove("ErrorMessage");
            var user = await _userManager.FindByNameAsync(loginDTO.username);
            if (user == null)
                TempData["ErrorMessage"] = "User does not exist";
            else
            {
                // Hypothetical email-reset if pwd null
                if (user.PasswordHash == null)
                    TempData["ErrorMessage"] = "Password uses old hashing function. (change password with the link sent to your email)";

                var result = await _signInManager.PasswordSignInAsync(user, loginDTO.pwd, loginDTO.rememberMe, false);
                if (!result.Succeeded)
                    TempData["ErrorMessage"] = "Wrong Username/Password";
            }


            if (TempData.ContainsKey("ErrorMessage"))
                return RedirectToAction("login", "Users");
            else
                return RedirectToAction("Private_Timeline", "Minitwit");
        }

        [HttpGet]
        [Route("[controller]/logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Public_Timeline", "Minitwit");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Public_Timeline", "Minitwit");
        }
        
        [HttpPost]
        public async Task<IActionResult> MigrationCreate(UserDTO user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);
            User newUser = new User
            {
                UserName = user.Username,
                PasswordHash = user.PasswordHash,
                Email = user.Email
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MigrationFollow(FollowDTO followDto)
        {
            User whom = _context.Users
                .Include(u => u.FollowedBy)
                .FirstOrDefault(u => u.UserName  == followDto.Whom);

            User who = _context.Users
                .Include(u => u.Follows)
                .FirstOrDefault(u => u.UserName == followDto.Who);

            if (whom != null && who != null)
            {
                _context.Follows.Add(new Follow()
                {
                    FollowerId = who.Id,
                    FolloweeId = whom.Id,
                });
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
