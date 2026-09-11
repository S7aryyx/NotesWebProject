using LocalJsonModule.DTOs;
using LocalJsonModule.Models;

namespace LocalJsonModule.Services;

public interface INoteService
{
    Task<List<Note>> GetAllAsync();
    Task<Note?> GetByIdAsync(Guid id);
    Task<List<Note>> GetByOwnerIdAsync(Guid ownerId);
    Task<Note> CreateAsync(NoteDTO request);
    Task<bool> UpdateAsync(Guid id, NoteDTO request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteByOwnerIdAsync(Guid ownerId);
}
