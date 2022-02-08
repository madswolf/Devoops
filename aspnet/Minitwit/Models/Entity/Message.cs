using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minitwit.Models.Entity
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public User Author { get; set; }
        public string Text { get; set; }
        public DateTime PublishDate { get; set; }
        public int Flagged { get; set; }
    }
}
