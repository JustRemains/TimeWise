using Microsoft.EntityFrameworkCore;
using TimeWise.Data;
using TimeWise.Models;

namespace TimeWise.Services
{
    /// <summary>
    /// 笔记服务实现
    /// </summary>
    public class NoteService : INoteService
    {
        private readonly TimeWiseDbContext _context;

        public NoteService(TimeWiseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Note>> GetNotesByDateAsync(DateTime date)
        {
            return await _context.Notes
                .Where(n => n.Date.Date == date.Date)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Note> AddNoteAsync(Note note)
        {
            note.CreatedAt = DateTime.Now;
            note.UpdatedAt = DateTime.Now;
            
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> UpdateNoteAsync(Note note)
        {
            note.UpdatedAt = DateTime.Now;
            
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task DeleteNoteAsync(int noteId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Note?> GetNoteByIdAsync(int noteId)
        {
            return await _context.Notes.FindAsync(noteId);
        }
    }
}