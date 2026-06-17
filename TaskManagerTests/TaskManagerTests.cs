using TaskManagerLib.Models;
using TaskManagerLib.Services;

namespace TaskManagerTests
{
    public class TaskManagerTests
    {
        [Fact]
        public void AddTask_ShouldIncreaseCountAndAssignId()
        {
            // Arrange
            var manager = new TaskManager();
            var task = new TaskItem { Title = "Test", DueDate = DateTime.Now.AddDays(1) };

            // Act
            manager.AddTask(task);

            // Assert
            var all = manager.GetAllTasks().ToList();
            Assert.Single(all);
            Assert.Equal(1, all[0].Id);
        }

        [Fact]
        public void FilterByStatus_ShouldReturnCorrectTasks()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { Title = "A", Status = Status.New });
            manager.AddTask(new TaskItem { Title = "B", Status = Status.Completed });
            manager.AddTask(new TaskItem { Title = "C", Status = Status.New });

            var result = manager.FilterByStatus(Status.New).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.Equal(Status.New, t.Status));
        }

        [Fact]
        public void Search_ShouldFindByTitleOrDescription()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { Title = "Alpha", Description = "First task" });
            manager.AddTask(new TaskItem { Title = "Beta", Description = "Second" });

            var result = manager.Search("alpha").ToList();
            Assert.Single(result);
            Assert.Equal("Alpha", result[0].Title);

            var result2 = manager.Search("task").ToList();
            Assert.Single(result2);
            Assert.Equal("Alpha", result2[0].Title);
        }

        [Fact]
        public void EditTask_ShouldUpdateFields()
        {
            var manager = new TaskManager();
            var original = new TaskItem { Title = "Old", Description = "Old desc", Priority = Priority.Low, DueDate = DateTime.Now, Status = Status.New };
            manager.AddTask(original);

            var updated = new TaskItem { Title = "New", Description = "New desc", Priority = Priority.High, DueDate = DateTime.Now.AddDays(5), Status = Status.InProgress, IsImportant = true };
            manager.EditTask(1, updated);

            var edited = manager.GetAllTasks().First();
            Assert.Equal("New", edited.Title);
            Assert.Equal("New desc", edited.Description);
            Assert.Equal(Priority.High, edited.Priority);
            Assert.Equal(Status.InProgress, edited.Status);
            Assert.True(edited.IsImportant);
        }

        [Fact]
        public void DeleteTask_ShouldRemoveTask()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { Title = "ToDelete" });
            manager.AddTask(new TaskItem { Title = "Keep" });

            manager.DeleteTask(1);
            var all = manager.GetAllTasks().ToList();

            Assert.Single(all);
            Assert.Equal(2, all[0].Id);
        }

        [Fact]
        public void SaveAndLoad_ShouldPreserveData()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { Title = "Task1", Description = "Desc1", Priority = Priority.High, DueDate = new DateTime(2026, 12, 31), Status = Status.InProgress, IsImportant = true });
            manager.AddTask(new TaskItem { Title = "Task2" });

            string tempFile = Path.GetTempFileName();
            try
            {
                manager.SaveToFile(tempFile);

                var newManager = new TaskManager();
                newManager.LoadFromFile(tempFile);

                var loaded = newManager.GetAllTasks().ToList();
                Assert.Equal(2, loaded.Count);
                Assert.Equal("Task1", loaded[0].Title);
                Assert.Equal(Priority.High, loaded[0].Priority);
                Assert.Equal(new DateTime(2026, 12, 31), loaded[0].DueDate);
                Assert.Equal(Status.InProgress, loaded[0].Status);
                Assert.True(loaded[0].IsImportant);
                Assert.Equal(3, loaded[1].Id); // следующий Id должен быть max+1
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void SortByPriority_ShouldOrderLowToHigh()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { Title = "High", Priority = Priority.High });
            manager.AddTask(new TaskItem { Title = "Low", Priority = Priority.Low });
            manager.AddTask(new TaskItem { Title = "Medium", Priority = Priority.Medium });

            var sorted = manager.SortByPriority().ToList();
            Assert.Equal("Low", sorted[0].Title);
            Assert.Equal("Medium", sorted[1].Title);
            Assert.Equal("High", sorted[2].Title);
        }

        [Fact]
        public void SortByDueDate_ShouldOrderChronologically()
        {
            var manager = new TaskManager();
            var now = DateTime.Now;
            manager.AddTask(new TaskItem { Title = "Later", DueDate = now.AddDays(5) });
            manager.AddTask(new TaskItem { Title = "Earlier", DueDate = now.AddDays(1) });
            manager.AddTask(new TaskItem { Title = "Middle", DueDate = now.AddDays(3) });

            var sorted = manager.SortByDueDate().ToList();
            Assert.Equal("Earlier", sorted[0].Title);
            Assert.Equal("Middle", sorted[1].Title);
            Assert.Equal("Later", sorted[2].Title);
        }

        [Fact]
        public void GetCompletedCount_ShouldReturnCorrectNumber()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { Status = Status.Completed });
            manager.AddTask(new TaskItem { Status = Status.New });
            manager.AddTask(new TaskItem { Status = Status.Completed });

            Assert.Equal(2, manager.GetCompletedCount());
        }

        [Fact]
        public void GetOverdueCount_ShouldReturnTasksPastDueAndNotCompleted()
        {
            var manager = new TaskManager();
            manager.AddTask(new TaskItem { DueDate = DateTime.Now.AddDays(-2), Status = Status.New });      // просрочена
            manager.AddTask(new TaskItem { DueDate = DateTime.Now.AddDays(-1), Status = Status.Completed }); // не просрочена (завершена)
            manager.AddTask(new TaskItem { DueDate = DateTime.Now.AddDays(1), Status = Status.New });        // не просрочена

            Assert.Equal(1, manager.GetOverdueCount());
        }
    }
}