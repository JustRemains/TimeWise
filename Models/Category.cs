using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeWise.Models
{
    /// <summary>
    /// ????????
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)] // ????????????? #FF5733
        public string ColorHex { get; set; } = "#CCCCCC";

        [Required]
        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ????
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}