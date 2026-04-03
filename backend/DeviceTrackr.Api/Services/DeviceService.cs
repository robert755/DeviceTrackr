using DeviceTrackr.Api.Configuration;
using DeviceTrackr.Api.Dtos;
using Microsoft.Extensions.Hosting;
using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Repositories;

namespace DeviceTrackr.Api.Services;

public class DeviceService(
    DeviceRepository repo,
    UserRepository users,
    GeminiDescriptionService gemini,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment)
{
    public List<Device> GetAll() => repo.GetAll();

    public Device? GetById(int id) => repo.GetById(id);

    public Device Create(Device device)
    {
        device.Description = string.IsNullOrWhiteSpace(device.Description) ? string.Empty : device.Description.Trim();
        return repo.Create(device);
    }

    /// <summary>Actualizează doar dacă dispozitivul nu e alocat. Erori: not_found, assigned.</summary>
    public (bool Success, string? Error) Update(int id, Device device)
    {
        var existing = repo.GetByIdTracked(id);
        if (existing is null)
        {
            return (false, "not_found");
        }

        if (existing.AssignedUserId.HasValue)
        {
            return (false, "assigned");
        }

        existing.Name = device.Name;
        existing.Manufacturer = device.Manufacturer;
        existing.Type = device.Type;
        existing.OperatingSystem = device.OperatingSystem;
        existing.OsVersion = device.OsVersion;
        existing.Processor = device.Processor;
        existing.RamAmountGb = device.RamAmountGb;
        existing.Description = string.IsNullOrWhiteSpace(device.Description) ? string.Empty : device.Description.Trim();
        repo.SaveChanges();
        return (true, null);
    }

    /// <summary>Șterge doar dacă dispozitivul nu e alocat. Erori: not_found, assigned.</summary>
    public (bool Success, string? Error) Delete(int id)
    {
        var existing = repo.GetByIdTracked(id);
        if (existing is null)
        {
            return (false, "not_found");
        }

        if (existing.AssignedUserId.HasValue)
        {
            return (false, "assigned");
        }

        repo.Remove(existing);
        repo.SaveChanges();
        return (true, null);
    }

    public (bool Success, string Error) AssignToUser(int deviceId, AssignDeviceRequestDto request)
    {
        var user = users.GetById(request.UserId);
        if (user is null)
        {
            return (false, "User not found.");
        }

        var device = repo.GetByIdTracked(deviceId);
        if (device is null)
        {
            return (false, "Device not found.");
        }

        if (device.AssignedUserId.HasValue && device.AssignedUserId.Value != request.UserId)
        {
            return (false, "Device is already assigned to another user.");
        }

        device.AssignedUserId = request.UserId;
        repo.SaveChanges();
        return (true, string.Empty);
    }

    public (bool Success, string Error) UnassignFromUser(int deviceId, AssignDeviceRequestDto request)
    {
        var device = repo.GetByIdTracked(deviceId);
        if (device is null)
        {
            return (false, "Device not found.");
        }

        if (!device.AssignedUserId.HasValue)
        {
            return (false, "Device is not assigned.");
        }

        if (device.AssignedUserId.Value != request.UserId)
        {
            return (false, "You can unassign only your own device.");
        }

        device.AssignedUserId = null;
        repo.SaveChanges();
        return (true, string.Empty);
    }

    /// <summary>Generates description with Gemini and persists it. Works for any device (assigned or not); only updates Description.</summary>
    /// <returns>GeminiDetail: extra message when Error is gemini_failed (from Google API or client).</returns>
    public async Task<(bool Success, string? Error, Device? Device, string? GeminiDetail)> GenerateAiDescriptionAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(GeminiConfigHelper.ResolveApiKey(configuration, hostEnvironment.ContentRootPath)))
        {
            return (false, "no_api_key", null, null);
        }

        var existing = repo.GetByIdTracked(id);
        if (existing is null)
        {
            return (false, "not_found", null, null);
        }

        var (text, geminiHint) = await gemini.GenerateDeviceDescriptionAsync(existing, cancellationToken);
        if (string.IsNullOrWhiteSpace(text))
        {
            return (false, "gemini_failed", null, geminiHint);
        }

        const int maxDesc = 2000;
        if (text.Length > maxDesc)
        {
            text = text[..maxDesc];
        }

        existing.Description = text;
        repo.SaveChanges();
        return (true, null, repo.GetById(id), null);
    }
}
