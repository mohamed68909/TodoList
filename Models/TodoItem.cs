using System.ComponentModel.DataAnnotations;

namespace TodoList.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يجب كتابة المهمة")]
        public string Title { get; set; }

        public bool IsDone { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? UserId { get; set; }
    }
}
