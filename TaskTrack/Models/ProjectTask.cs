
using System;
using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Models
{
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression(
            "^(To Do|In Progress|Completed)$",
            ErrorMessage = "Invalid task status.")]
        public string Status { get; set; } = "To Do";

        [Required]
        [RegularExpression(
            "^(Low|Medium|High)$",
            ErrorMessage = "Invalid priority.")]
        public string Priority { get; set; } = "Medium";

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        public string? AssignedUserId { get; set; }
    }
}
