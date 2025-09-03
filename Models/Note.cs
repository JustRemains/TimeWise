using System.ComponentModel.DataAnnotations;

namespace TimeWise.Models
{
    /// <summary>
    /// 笔记模型 - 随时记入的笔记，不与特定日期绑定
    /// </summary>
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}