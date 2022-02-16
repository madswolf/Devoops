using Microsoft.Build.Framework;

namespace Minitwit.Models.DTO
{
    public class SimulatorMessageCreationDTO
    {
        [Required]
        public string content { get; set; }
    }
}
