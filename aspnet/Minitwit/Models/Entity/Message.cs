using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minitwit.Models.Entity
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public User Author { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime PublishDate { get; set; }
        public bool Flagged { get; set; }
    }
}
