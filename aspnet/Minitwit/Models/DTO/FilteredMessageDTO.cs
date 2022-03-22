using Microsoft.AspNetCore.SignalR;

namespace Minitwit.Models.DTO
{
    public class FilteredMessageDTO
    {
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public string AuthorName { get; set; }
    }
}
