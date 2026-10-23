using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace TaskTrack.Models;
public class DashboardViewModel
{
    public Dictionary<string, string> UserNames { get; set; } = [];
    public int ProjectCount { get; set; }
    public int TaskCount { get; set; }
    public int ToDoCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public List<ProjectTask> UpcomingTasks { get; set; } = [];
    public List<ProjectActivity> RecentActivity { get; set; } = [];
}
public class CommentInput
{
    [Required, StringLength(2000), Display(Name = "Comment")]
    public string Body { get; set; } = "";
}
public class TaskDetailsViewModel
{
    public ProjectTask Task { get; set; } = null!;
    public List<TaskComment> Comments { get; set; } = [];
    public List<ProjectActivity> Activity { get; set; } = [];
    public Dictionary<string, string> UserNames { get; set; } = [];
    public CommentInput Input { get; set; } = new();
    public int CommentPage { get; set; } = 1;
    public bool HasMoreComments { get; set; }
}
public class ProjectMembersViewModel
{
    public Project Project { get; set; } = null!;
    public List<IdentityUser> Members { get; set; } = [];
    [Required, EmailAddress, StringLength(256), Display(Name = "Member email")]
    public string Email { get; set; } = "";
}
public class ProjectActivityViewModel
{
    public Dictionary<string, string> UserNames { get; set; } = [];
    public Project Project { get; set; } = null!;
    public List<ProjectActivity> Events { get; set; } = [];
    public int Page { get; set; } = 1;
    public bool HasMore { get; set; }
}
