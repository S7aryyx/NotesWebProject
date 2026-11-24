namespace LocalJsonModule.DTOs.Notes;

public class CreateNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid OwnerId { get; set; } //Позже вырезать. Заменить на передачу через куки
    public Guid? FolderId { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsArchive { get; set; }
    public string? NoteType { get; set; }
}
