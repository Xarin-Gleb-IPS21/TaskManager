using System.Collections.Generic;
using TaskManagerLib.Models;

namespace TaskManagerLib.Interfaces
{
    public interface ITaskManager
    {
        void AddTask(TaskItem task);
        IEnumerable<TaskItem> GetAllTasks();
        IEnumerable<TaskItem> FilterByStatus(Status status);
        IEnumerable<TaskItem> Search(string query);
        void EditTask(int id, TaskItem updatedTask);
        void DeleteTask(int id);
        void SaveToFile(string filePath);
        void LoadFromFile(string filePath);

        // Дополнительные методы (задания 2.1 – 2.3)
        IEnumerable<TaskItem> SortByPriority();
        IEnumerable<TaskItem> SortByDueDate();
        int GetCompletedCount();
        int GetOverdueCount();
    }

}
