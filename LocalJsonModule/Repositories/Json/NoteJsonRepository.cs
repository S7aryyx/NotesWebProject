using LocalJsonModule.Interfaces;
using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LocalJsonModule.Services;
using LocalJsonModule.DataPathProvider;

namespace LocalJsonModule.Repositories.Json
{
    public class NoteJsonRepository :  INoteJsonService
    {
        private readonly string _filePath;
        public NoteJsonRepository(IDataPathProvider dataPathProvider)
        {
            var dataPath = dataPathProvider.GetNotesFilePath();

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            _filePath = Path.Combine(dataPath, "notes.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }
        public async Task<List<Note>> LoadNotesAsync()
        {
            try
            {
                string original_json;
                using (var file_stream = new FileStream(_filePath,
                    FileMode.OpenOrCreate, FileAccess.Read,
                    FileShare.Read, bufferSize: 4096, useAsync: true))
                using (var reader = new StreamReader(file_stream, Encoding.UTF8))
                {
                    original_json = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrEmpty(original_json))
                {
                    original_json = "[]";
                }

                var notes = JsonSerializer.Deserialize<List<Note>>(original_json);
                Console.WriteLine($"Загрузка завершена.\n(Поток:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                return notes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных.{ex.Message}");
                return new List<Note>();
            }
        }
        public async Task<Note> GetNoteByIdAsync(Guid id)
        {
            try
            {
                var notes = await LoadNotesAsync();
                var found_note = notes.FirstOrDefault(n => n.Id == id);

                if (found_note == null)
                {
                    return new Note();
                }
                return found_note;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении заметки по ID: {ex.Message}");
                return new Note();
            }
        }
        public async Task<List<Note>> GetNotesByOwnerIdAsync(Guid ownerId)
        {
            //Данный метод под вопросом , тк связывать JSON файлы напрямую не очень хорошо,
            //Данный метод 100% будет в Репозитории БД...
            Console.WriteLine($"В данный момент метод GetNotesByOwnerIdAsync не реализован");
            return null;
        }
        public async Task<bool> DeleteNotesByOwnerIdAsync(Guid ownerId)
        {
            //Второй метод из разряда "Под вопросом" , метод , разом удаляющий
            //все заметки пользователя по его ID , в случае удаления пользователя из системы.
            Console.WriteLine($"В данный момент метод DeleteNotesByOwnerIdAsync не реализован");
            return false;
        }
        public async Task<bool> UpdateNoteByIdAsync(Guid id, string newTitle, string newContent)
        {
            try
            {
                var notes = await LoadNotesAsync();
                var found_note = notes.FirstOrDefault(n => n.Id == id);

                if (found_note == null)
                {
                    Console.WriteLine($"Заметка с ID {id} не найдена.");
                    return false;
                }

                found_note.Title = newTitle; found_note.Content = newContent;
                return true;
            } 
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message); 
                return false; 
            }
        }
        public async Task AddNoteAsync(string newTitle, string newContent, Guid ownerId)
        {
            try
            {
                var notes = await LoadNotesAsync();
                var newNote = new Note
                {
                    Id = Guid.NewGuid(),
                    Title = newTitle,
                    Content = newContent,
                    OwnerId = ownerId
                };

                notes.Add(newNote);
                await SaveNoteAsync(notes);
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task SaveNoteAsync(List<Note> notes)
        {
            try
            {
                string original_json;
                using (var file_stream = new FileStream(_filePath,
                    FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(file_stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(JsonSerializer.Serialize(notes));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных.{ex.Message}");
            }
        }
        public async Task<bool> DeleteNoteByIdAsync(Guid id)
        {
            try
            {
                var notes = await LoadNotesAsync();
                var noteToDelete = notes.FirstOrDefault(n => n.Id == id);

                if (noteToDelete == null)
                {
                    return false;
                }

                notes.Remove(noteToDelete);
                await SaveNoteAsync(notes);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


        }  
    }
}
