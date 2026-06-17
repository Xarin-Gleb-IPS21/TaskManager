using System;

namespace TaskManagerLib.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Medium;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        public Status Status { get; set; } = Status.New;
        public bool IsImportant { get; set; } = false;

        public override string ToString()
        {
            return $"{Title} ({(IsImportant ? "★ " : "")}{Status})";
        }
    }

}
