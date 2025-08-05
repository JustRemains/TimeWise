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

        [Required]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ????
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        // ????
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