using TeamFlow.Models;

public class ProjectInvite
{
    public Guid Id { get; set; } // Уникальный id приглашения (invite link)
    public Guid ProjectId { get; set; }
    public string Email { get; set; }
    public ProjectRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Accepted { get; set; }

    public Project Project { get; set; }
}
