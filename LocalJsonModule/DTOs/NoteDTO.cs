namespace LocalJsonModule.DTOs;

public class NoteDTO
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}
