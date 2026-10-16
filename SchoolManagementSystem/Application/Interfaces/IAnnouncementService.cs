
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IAnnouncementService
{
    Task<List<Announcement>> GetAllAsync();

    Task<List<Announcement>> GetPublishedAsync(string? audience = null);

    Task<Announcement?> GetByIdAsync(int id);

    Task CreateAsync(Announcement announcement);

    Task<bool> UpdateAsync(Announcement announcement);

    Task<bool> DeleteAsync(int id);

    Task<bool> TogglePublishAsync(int id);
}