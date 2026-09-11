using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Interfaces
{
    public interface INoteJsonService
    {
        Task<List<Note>> LoadNotesAsync();
        Task<Note> GetNoteByIdAsync(Guid id);
        Task<List<Note>> GetNotesByOwnerIdAsync(Guid ownerId);
        Task<bool> UpdateNoteByIdAsync(Guid id, string newTitle, string newContent);
        Task AddNoteAsync(string newTitle, string newContent, Guid ownerId);
        Task SaveNoteAsync(List<Note> notes);
        Task<bool> DeleteNoteByIdAsync(Guid id);
        Task<bool> DeleteNotesByOwnerIdAsync(Guid ownerId);
    }
}
