using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.Repositories
{
    public class MessageRepository: IMessageRepository
    {
        private readonly MinitwitContext _context;

        public MessageRepository(MinitwitContext context)
        {
            _context = context;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Posts
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Message>> GetPrivateTimeline(int id, List<int> follows, int limit = 30)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged)
                .Where(p =>
                    follows.Contains(p.AuthorId)
                    || p.AuthorId == id
                    )
                .OrderByDescending(p => p.PublishDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByAuthorId(int id, int limit = 30)
        {
            return await _context.Users
                .Include(u => u.Messages)
                .Where(u => u.Id == id)
                .SelectMany(u =>
                             u.Messages
                            .OrderByDescending(p => p.PublishDate)
                            .Where(p => !p.Flagged)
                            .Take(limit)
                )
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessages(int limit = 30)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged)
                .OrderByDescending(p => p.PublishDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<FilteredMessageDTO>> GetFilteredMessages(int limit = 100)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.Flagged)
                .OrderByDescending(p => p.PublishDate)
                .Take(limit)
                .Select(p => new FilteredMessageDTO
                {
                    Content = p.Text,
                    PublishDate = p.PublishDate,
                    AuthorName = p.Author.UserName
                })
                .ToListAsync();
        }

        public async Task InsertMessage(Message message)
        {
            _context.Posts.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task FlagMessage(int id, bool flagged)
        {
            _context.Posts.Where(p => p.Id == id).FirstOrDefault().Flagged = flagged;
            await _context.SaveChangesAsync();
        }
    }
}
