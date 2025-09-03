using TimeWise.Models;

namespace TimeWise.Services
{
    /// <summary>
    /// 笔记服务接口
    /// </summary>
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetAllNotesAsync();
        Task<Note> AddNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int noteId);
        Task<Note?> GetNoteByIdAsync(int noteId);
        Task ClearAllNotesAsync();
    }
}