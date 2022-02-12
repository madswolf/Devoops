using Microsoft.AspNetCore.SignalR;

namespace Minitwit.Models.DTO
{
    public class MessageDTO
    {
        public string AuthorName { get; set; }
        public string Text { get; set; }
        public bool Flagged { get; set; }
        public long Created { get; set; }
    }
}
