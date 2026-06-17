using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TaskManagerLib.Models;
using TaskManagerLib.Services;

namespace TaskManagerUI
{
    public partial class MainWindow : Window
    {
        private readonly TaskManager _taskManager = new TaskManager();
        private TaskItem _selectedTask;

        public MainWindow()
        {
            InitializeComponent();
            CmbPriority.ItemsSource = Enum.GetValues(typeof(Priority));
            CmbStatus.ItemsSource = Enum.GetValues(typeof(Status));
            CmbFilterStatus.ItemsSource = Enum.GetValues(typeof(Status));
            RefreshList();
            RefreshList();
        }

        // Обновление списка задач в ListView
        private void RefreshList()
        {
            LvTasks.ItemsSource = _taskManager.GetAllTasks().ToList();
            UpdateStatistics();
            ClearInputFields();
            _selectedTask = null;
        }

        // Обновление статистики
        private void UpdateStatistics()
        {
            int completed = _taskManager.GetCompletedCount();
            int overdue = _taskManager.GetOverdueCount();
            TblStats.Text = $"✅ Завершено: {completed}  |  ⏰ Просрочено: {overdue}";
        }

        // Очистка полей ввода
        private void ClearInputFields()
        {
            TxtTitle.Text = string.Empty;
            TxtDescription.Text = string.Empty;
            CmbPriority.SelectedIndex = 1; // Medium
            DpDueDate.SelectedDate = DateTime.Now.AddDays(7);
            CmbStatus.SelectedIndex = 0;   // New
            ChkImportant.IsChecked = false;
        }

        // Заполнение полей для редактирования при выборе задачи
        private void LvTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LvTasks.SelectedItem is TaskItem task)
            {
                _selectedTask = task;
                TxtTitle.Text = task.Title;
                TxtDescription.Text = task.Description;
                CmbPriority.SelectedItem = task.Priority;
                DpDueDate.SelectedDate = task.DueDate;
                CmbStatus.SelectedItem = task.Status;
                ChkImportant.IsChecked = task.IsImportant;
            }
        }

        // Создание TaskItem из полей ввода
        private TaskItem GetTaskFromInput()
        {
            return new TaskItem
            {
                Title = TxtTitle.Text.Trim(),
                Description = TxtDescription.Text.Trim(),
                Priority = (Priority)CmbPriority.SelectedItem,
                DueDate = DpDueDate.SelectedDate ?? DateTime.Now.AddDays(7),
                Status = (Status)CmbStatus.SelectedItem,
                IsImportant = ChkImportant.IsChecked ?? false
            };
        }

        // Добавить
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtTitle.Text))
                {
                    MessageBox.Show("Введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var task = GetTaskFromInput();
                _taskManager.AddTask(task);
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Редактировать
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedTask == null)
                {
                    MessageBox.Show("Выберите задачу для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(TxtTitle.Text))
                {
                    MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var updated = GetTaskFromInput();
                _taskManager.EditTask(_selectedTask.Id, updated);
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Удалить
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedTask == null)
                {
                    MessageBox.Show("Выберите задачу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"Удалить задачу '{_selectedTask.Title}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _taskManager.DeleteTask(_selectedTask.Id);
                    RefreshList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Сохранить
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = "tasks.json"
                };
                if (dialog.ShowDialog() == true)
                {
                    _taskManager.SaveToFile(dialog.FileName);
                    MessageBox.Show("Задачи сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Загрузить
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };
                if (dialog.ShowDialog() == true)
                {
                    _taskManager.LoadFromFile(dialog.FileName);
                    RefreshList();
                    MessageBox.Show("Задачи загружены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Поиск
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();
            var result = _taskManager.Search(query);
            LvTasks.ItemsSource = result.ToList();
            UpdateStatistics();
        }

        // Фильтр по статусу
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            var status = (Status)CmbFilterStatus.SelectedItem;
            var result = _taskManager.FilterByStatus(status);
            LvTasks.ItemsSource = result.ToList();
            UpdateStatistics();
        }

        // Сортировка по приоритету
        private void BtnSortPriority_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _taskManager.SortByPriority();
            LvTasks.ItemsSource = sorted.ToList();
            UpdateStatistics();
        }

        // Сортировка по сроку
        private void BtnSortDueDate_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _taskManager.SortByDueDate();
            LvTasks.ItemsSource = sorted.ToList();
            UpdateStatistics();
        }

        // Статистика (дополнительно в MessageBox)
        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            int completed = _taskManager.GetCompletedCount();
            int overdue = _taskManager.GetOverdueCount();
            int total = _taskManager.GetAllTasks().Count();
            MessageBox.Show(
                $"📊 Статистика:\n\n" +
                $"Всего задач: {total}\n" +
                $"Завершено: {completed}\n" +
                $"Просрочено: {overdue}\n" +
                $"В работе: {total - completed}",
                "Статистика",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

}
