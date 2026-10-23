using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
