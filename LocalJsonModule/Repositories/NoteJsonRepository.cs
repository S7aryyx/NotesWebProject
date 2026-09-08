using LocalJsonModule.Interfaces;
using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Repositories
{
    public class NoteJsonRepository : INoteJsonService
    {
        public Task<List<Note>> LoadNotesAsync()
        {
            return null;
        }

        public Task<List<Note>> GetNoteByIdAsync(int id)
        {
            return null;
        }

        //Данный метод под вопросом , тк связывать JSON файлы напрямую не очень хорошо,
        //Данный метод 100% будет в Репозитории БД...
        public Task<List<Note>> GetNotesByOwnerIdAsync(int ownerId)
        {
            return null;
        }
        //Второй метод из разряда "Под вопросом" , метод , разом удаляющий
        //все заметки пользователя по его ID , в случае удаления пользователя из системы.
        public Task<bool> DeleteNotesByOwnerIdAsync(int ownerId)
        {
            return null;
        }

        public Task<bool> UpdateNoteByIdAsync(int id, string newTitle, string newDescription)
        {
            return null;
        }
        public Task AddNoteAsync(string newTitle, string newDescription, int ownerId)
        {
            return null;
        }
        public Task SaveNoteAsync(List<Note> notes)
        {
            return null;
        }
        public Task<bool> DeleteNoteByIdAsync(int id)
        {
            return null;
        }

        
    }
}
