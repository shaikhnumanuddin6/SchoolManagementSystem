namespace SchoolManagementSystem.Application.Interfaces;

public interface IAIService
{
    Task<string> GetResponseAsync(string prompt);
}