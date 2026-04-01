using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Repositories;

namespace DeviceTrackr.Api.Services;

public class DeviceService(DeviceRepository repo)
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
}
