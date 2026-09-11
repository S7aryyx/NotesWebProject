namespace LocalJsonModule.DTOs.Notes;

public class CreateNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}
