using System.ComponentModel.DataAnnotations;

namespace Minitwit.Models.DTO
{
    public class MessageCreationDTO
    {
        [Required]
        [StringLength(255, ErrorMessage = "Message should not exceed 255 characters.")]
        public string Text { get; set; }
    }
}
