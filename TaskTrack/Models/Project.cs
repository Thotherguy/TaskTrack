
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public ICollection<ProjectTask> Tasks { get; set; }
            = new List<ProjectTask>();

        public ICollection<ProjectMember> Members { get; set; }
            = new List<ProjectMember>();
    }
}
