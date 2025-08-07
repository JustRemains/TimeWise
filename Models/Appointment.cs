using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeWise.Models
{
    /// <summary>
    /// ??????
    /// </summary>
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "TIME")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Column(TypeName = "TIME")]
        public TimeSpan EndTime { get; set; }

        // 保留CategoryId以便向后兼容，但它将成为主要Category
        [Required]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 单个主要Category的导航属性（向后兼容）
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        // 多对多关系：一个Appointment可以有多个Categories
        public virtual ICollection<AppointmentCategory> AppointmentCategories { get; set; } = new List<AppointmentCategory>();

        // 便捷属性：获取所有相关的Categories
        [NotMapped]
        public virtual IEnumerable<Category> Categories => AppointmentCategories.Select(ac => ac.Category);

        // 计算属性
        [NotMapped]
        public DateTime StartDateTime => Date.Date + StartTime;

        [NotMapped]
        public DateTime EndDateTime => Date.Date + EndTime;

        [NotMapped]
        public TimeSpan Duration => EndTime - StartTime;

        [NotMapped]
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}