using System.ComponentModel.DataAnnotations;

namespace Minitwit.Models.DTO
{
    public class MessageCreationDTO
    {
        [Required]
        public string Text { get; set; }
    }
}
