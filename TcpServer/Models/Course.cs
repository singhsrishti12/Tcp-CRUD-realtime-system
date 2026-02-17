using System.ComponentModel.DataAnnotations;

namespace ExamServer.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int Credits { get; set; }

    }
}
