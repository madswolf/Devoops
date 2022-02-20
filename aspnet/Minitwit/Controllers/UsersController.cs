#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using Minitwit.Services;

namespace Minitwit.Controllers
{
    public class UsersController : Controller
    {
        private readonly MinitwitContext _context;
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        public UsersController(MinitwitContext context, IUserService userService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("[controller]/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userDTO)
        {
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

        [HttpPost]
        [Route("[controller]/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.username);
            if (user == null) return BadRequest("User does not exist");
            // Hypothetical email-reset if pwd null
            if (user.PasswordHash == null)
            {
                return BadRequest("Password uses old hashing function. (change password with the link sent to your email)");
            }
            if (_userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDTO.pwd)
                == PasswordVerificationResult.Failed) return Unauthorized("Wrong password");
            await _signInManager.SignInAsync(user, false);
            return Ok();
        }

        [HttpPost]
        [Route("[controller]/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }


        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]UserRegistrationDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);

            return await _userService.CreateUser(userDTO) switch
            {
                Result.Conflict => StatusCode(413),
                Result.Created => StatusCode(201),
                //should never happen
                _ => throw new Exception()
            };
        }
        
        [HttpPost]
        public async Task<IActionResult> MigrationCreate([FromBody] UserDTO user)
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
                whom.FollowedBy.Add(who);
                who.Follows.Add(whom);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,PasswordHash,Email")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
