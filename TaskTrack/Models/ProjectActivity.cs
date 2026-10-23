using System.ComponentModel.DataAnnotations;
namespace TaskTrack.Models;
public class ProjectActivity
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    // A snapshot ID, deliberately without a task FK, preserves deletion history.
    public int? ProjectTaskId { get; set; }
    [StringLength(450)] public string? ActorId { get; set; }
    [Required, StringLength(500)] public string Summary { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
