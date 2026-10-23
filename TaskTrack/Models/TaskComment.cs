using System.ComponentModel.DataAnnotations;
namespace TaskTrack.Models;
public class TaskComment
{
    public int Id { get; set; }
    public int ProjectTaskId { get; set; }
    public ProjectTask ProjectTask { get; set; } = null!;
    [Required, StringLength(450)] public string AuthorId { get; set; } = "";
    [Required, StringLength(2000)] public string Body { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
