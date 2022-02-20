#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MinitwitContext _context;

        public MessagesController(MinitwitContext context)
        {
            _context = context;
        }


        [HttpPost]
        [Route("[Controller]/{username}/Follow")]
        public async Task<IActionResult> Follow(string username)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login", "Users");

            User whom = _context.Users
                .Include(u => u.FollowedBy)
                .FirstOrDefault(u => u.Id == int.Parse(userId));

            User who = _context.Users
                .Include(u => u.Follows)
                .FirstOrDefault(u => u.UserName == username);

            if (who == null) return NotFound($"User with name {username} not found");

            whom.FollowedBy.Add(who);
            who.Follows.Add(whom);
            await _context.SaveChangesAsync();



            //Todo Redirect to private timeline
            return RedirectToAction(nameof(TimelineController.PrivateTimeline), "Timeline");
        }


        //Todo If this is not too much trouble in the front end, lets simply make 1 endpoint and a boolean for follow/unfollow
        [HttpPost]
        [Route("[Controller]/{username}/unFollow")]
        public async Task<IActionResult> UnFollow(string username)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login", "Users");

            User whom = _context.Users
                .Include(u => u.FollowedBy)
                .FirstOrDefault(u => u.Id == int.Parse(userId));

            User who = _context.Users
                .Include(u => u.Follows)
                .FirstOrDefault(u => u.UserName == username);

            if (who == null) return NotFound($"User with name {username} not found");

            whom.FollowedBy.Remove(who);
            who.Follows.Remove(whom);
            await _context.SaveChangesAsync();

            //Todo Redirect to private timeline
            return RedirectToAction(nameof(TimelineController.PrivateTimeline), "Timeline");
        }

        [HttpPost]
        [Route("[controller]/PostMessage")]
        public async Task<IActionResult> PostMessage([FromBody] string text)
        { 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login","Users");

            if (!ModelState.IsValid) return BadRequest("Text is required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            Message newMessage = new Message()
            {
                Author = user,
                Text = text,
                PublishDate = DateTime.UtcNow
            };

            _context.Posts.Add(newMessage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TimelineController.PublicTimeline), "Timeline");
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            return View(await _context.Posts.ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> MigrationCreate(MessageDTO message)
        {
            if (ModelState.IsValid)
            {
                User author = await _context.Users.FirstOrDefaultAsync(u => u.UserName == message.AuthorName);
                if (author == null)
                {
                    return NotFound();
                }

                Message newMessage = new Message()
                {
                    Author = author,
                    Text = message.Text,
                    Flagged = message.Flagged,
                    PublishDate = DateTimeOffset.FromUnixTimeSeconds(message.Created).UtcDateTime
                };

                _context.Posts.Add(newMessage);
                await _context.SaveChangesAsync();
                return Ok();
;            }

            return new UnsupportedMediaTypeResult();
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Posts.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,PublishDate,Flagged")] Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
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
            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
