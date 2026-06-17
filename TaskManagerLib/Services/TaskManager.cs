using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using TaskManagerLib.Models;
using TaskManagerLib.Interfaces;

namespace TaskManagerLib.Services
{
    public class TaskManager : ITaskManager
    {
        private List<TaskItem> _tasks = new List<TaskItem>();
        private int _nextId = 1;

        public void AddTask(TaskItem task)
        {
            task.Id = _nextId++;
            _tasks.Add(task);
        }

        public IEnumerable<TaskItem> GetAllTasks() => _tasks;

        public IEnumerable<TaskItem> FilterByStatus(Status status)
            => _tasks.Where(t => t.Status == status);

        public IEnumerable<TaskItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _tasks;
            return _tasks.Where(t =>
                t.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                t.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public void EditTask(int id, TaskItem updatedTask)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == id);
            if (existing == null)
                throw new ArgumentException($"Задача с Id {id} не найдена.");

            existing.Title = updatedTask.Title;
            existing.Description = updatedTask.Description;
            existing.Priority = updatedTask.Priority;
            existing.DueDate = updatedTask.DueDate;
            existing.Status = updatedTask.Status;
            existing.IsImportant = updatedTask.IsImportant;
        }

        public void DeleteTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
                _tasks.Remove(task);
        }

        public void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_tasks, options);
            File.WriteAllText(filePath, json);
        }

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;
            string json = File.ReadAllText(filePath);
            var loaded = JsonSerializer.Deserialize<List<TaskItem>>(json);
            if (loaded != null)
            {
                _tasks = loaded;
                _nextId = _tasks.Any() ? _tasks.Max(t => t.Id) + 1 : 1;
            }
        }

        // Дополнительные методы
        public IEnumerable<TaskItem> SortByPriority()
            => _tasks.OrderBy(t => t.Priority); // Low → High

        public IEnumerable<TaskItem> SortByDueDate()
            => _tasks.OrderBy(t => t.DueDate);

        public int GetCompletedCount()
            => _tasks.Count(t => t.Status == Status.Completed);

        public int GetOverdueCount()
            => _tasks.Count(t => t.DueDate < DateTime.Now && t.Status != Status.Completed);
    }

}
