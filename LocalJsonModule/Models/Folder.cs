namespace LocalJsonModule.Models;

public class Folder
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
