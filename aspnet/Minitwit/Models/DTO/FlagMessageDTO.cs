using System.ComponentModel.DataAnnotations;

namespace Minitwit.Models.DTO
{
    public class FlagMessageDTO
    {
        [Required]
        public int MessageId { get; set; }
        [Required]
        public bool Flagged { get; set; }
    }
}
