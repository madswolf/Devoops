﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minitwit.DatabaseUtil;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;
using Minitwit.Services;

namespace Minitwit.Controllers
{
    public class MinitwitController : Controller
    {
        private readonly MinitwitContext _context;
        private const int PER_PAGE = 30;

        public MinitwitController(MinitwitContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Private_Timeline()
        {
            var userStringId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userStringId == null) return RedirectToAction(nameof(Public_Timeline));
            var userId = int.Parse(userStringId);

            var follows = await _context.Users
                .Include(u => u.Follows)
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Follows.Select(f => f.FolloweeId))
                .ToListAsync();

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
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .FirstOrDefaultAsync();
            if (user == null) return NotFound($"UserName with name {username} not found");

            var posts = await _context.Users
                .Include(u => u.Messages)
                .Where(u => u.UserName == username)
                .SelectMany(u => 
                             u.Messages
                            .OrderByDescending(p => p.PublishDate)
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login", "Users");

            User follower = _context.Users
                .Include(u => u.Follows)
                .FirstOrDefault(u => u.Id == int.Parse(userId));

            User followee = _context.Users
                .Include(u => u.FollowedBy)
                .FirstOrDefault(u => u.UserName == username);

            if (followee == null) return NotFound($"User with name {username} not found");
            if (_context.Follows.Any(f => f.FolloweeId == followee.Id && f.FollowerId == follower.Id))
                return Conflict($"User is already following {username}");
            _context.Follows.Add(new Follow()
            {
                FollowerId = follower.Id,
                FolloweeId = followee.Id,
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(username, "Minitwit");
        }


        [HttpGet]
        [Route("[Controller]/{username}/unFollow")]
        public async Task<IActionResult> UnFollow(string username)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login", "Users");

            User followee = _context.Users
                .Include(u => u.FollowedBy)
                .FirstOrDefault(u => u.UserName == username);
            if (followee == null) return NotFound($"User with name {username} not found");

            var follow = _context.Users
                .Include(u => u.Follows)
                .SelectMany(u => u.Follows)
                .FirstOrDefault(f => f.FollowerId == int.Parse(userId) && f.FolloweeId == followee.Id);

            if (follow == null) return NotFound();

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return RedirectToAction(username, "Minitwit");
        }

        [HttpPost]
        [Route("[controller]/PostMessage")]
        public async Task<IActionResult> PostMessage([FromBody] string text)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("login", "Users");

            if (!ModelState.IsValid) return BadRequest("Text is required");

            Message newMessage = new Message()
            {
                AuthorId = int.Parse(userId),
                Text = text,
                PublishDate = DateTime.UtcNow
            };

            _context.Posts.Add(newMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Public_Timeline), "Minitwit");
        }
    }
}
