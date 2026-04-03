using DeviceTrackr.Api.Dtos;
using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Repositories;

namespace DeviceTrackr.Api.Services;

public class DeviceService(DeviceRepository repo, UserRepository users)
{
    public List<Device> GetAll() => repo.GetAll();

    public Device? GetById(int id) => repo.GetById(id);

    public Device Create(Device device)
    {
        return repo.Create(device);
    }

    public bool Update(int id, Device device)
    {
        device.Id = id;
        return repo.Update(device);
    }

    public bool Delete(int id) => repo.Delete(id);

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
}
