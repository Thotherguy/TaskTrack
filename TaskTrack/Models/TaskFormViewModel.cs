
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskTrack.Models
{
    public class TaskFormViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression("^(To Do|In Progress|Completed)$")]
        public string Status { get; set; } = "To Do";

        [Required]
        [RegularExpression("^(Low|Medium|High)$")]
        public string Priority { get; set; } = "Medium";

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public string? AssignedUserId { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; }
            = new List<SelectListItem>();
    }
}
