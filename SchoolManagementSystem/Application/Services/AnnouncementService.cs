
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;

    public AnnouncementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Announcement>> GetAllAsync()
    {
        return await _context.Announcements
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Announcement>> GetPublishedAsync(
        string? audience = null)
    {
        var now = DateTime.UtcNow;

        var query = _context.Announcements
            .AsNoTracking()
            .Where(a => a.IsPublished)
            .Where(a => a.PublishAt == null || a.PublishAt <= now)
            .Where(a => a.ExpiresAt == null || a.ExpiresAt > now);

        if (!string.IsNullOrWhiteSpace(audience))
        {
            query = query.Where(a =>
                a.TargetAudience == "Everyone" ||
                a.TargetAudience == audience);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Announcement?> GetByIdAsync(int id)
    {
        return await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task CreateAsync(Announcement announcement)
    {
        announcement.Title = announcement.Title.Trim();
        announcement.Content = announcement.Content.Trim();
        announcement.TargetAudience =
            announcement.TargetAudience.Trim();

        announcement.CreatedAt = DateTime.UtcNow;

        _context.Announcements.Add(announcement);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Announcement announcement)
    {
        var existing = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == announcement.Id);

        if (existing == null)
        {
            return false;
        }

        existing.Title = announcement.Title.Trim();
        existing.Content = announcement.Content.Trim();
        existing.TargetAudience =
            announcement.TargetAudience.Trim();

        existing.PublishAt = announcement.PublishAt;
        existing.ExpiresAt = announcement.ExpiresAt;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
        {
            return false;
        }

        _context.Announcements.Remove(announcement);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TogglePublishAsync(int id)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
        {
            return false;
        }

        announcement.IsPublished = !announcement.IsPublished;

        await _context.SaveChangesAsync();

        return true;
    }
}
