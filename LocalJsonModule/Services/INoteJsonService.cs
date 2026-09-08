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
        Task<List<Note>> GetNoteByIdAsync(int id);

        //Данный метод под вопросом , тк связывать JSON файлы напрямую не очень хорошо,
        //Данный метод 100% будет в Репозитории БД...
        Task<List<Note>> GetNotesByOwnerIdAsync(int ownerId);


        Task<bool> UpdateNoteByIdAsync(int id, string newTitle, string newDescription);
        Task AddNoteAsync(string newTitle, string newDescription, int ownerId);
        Task SaveNoteAsync(List<Note> notes);
        Task<bool> DeleteNoteByIdAsync(int id);

        //Второй метод из разряда "Под вопросом" , метод , разом удаляющий
        //все заметки пользователя по его ID , в случае удаления пользователя из системы.
        Task<bool> DeleteNotesByOwnerIdAsync(int ownerId);
    }
}
