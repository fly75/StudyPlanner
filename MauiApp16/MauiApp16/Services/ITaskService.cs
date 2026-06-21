using MauiApp16.Models;

namespace MauiApp16.Services;

public interface ITaskService
{
    Task<List<TaskModel>> GetAllTasksAsync();
    Task<List<TaskModel>> GetTasksByCourseAsync(int courseId);
    Task<List<TaskModel>> GetUpcomingTasksAsync(int days);
    Task<TaskModel> GetTaskByIdAsync(int id);
    Task<bool> SaveTaskAsync(TaskModel task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> CompleteTaskAsync(int taskId);

    Task<List<TaskModel>> SearchTasksAsync(
        int courseId,
        string query,
        Models.TaskStatus? status,
        DateTime? deadlineFrom,
        DateTime? deadlineTo);
}