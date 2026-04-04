using DeviceTrackr.Api.Data;
using DeviceTrackr.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceTrackr.Api.Repositories;

public class DeviceRepository(DeviceTrackrDbContext db)
{
    public List<Device> GetAll()
    {
        return db.Devices.AsNoTracking().Include(x => x.AssignedUser).OrderBy(x => x.Name).ToList();
    }

    public Device? GetById(int id)
    {
        return db.Devices.AsNoTracking().Include(x => x.AssignedUser).FirstOrDefault(x => x.Id == id);
    }

    /// <summary>Free-text search across main device fields; whitespace-only query returns all devices (same ordering as GetAll).</summary>
    public List<Device> SearchByText(string? query)
    {
        var baseQuery = db.Devices.AsNoTracking().Include(x => x.AssignedUser);
        if (string.IsNullOrWhiteSpace(query))
        {
            return baseQuery.OrderBy(x => x.Name).ToList();
        }

        var term = query.Trim();
        return baseQuery
            .Where(d =>
                d.Name.Contains(term)
                || d.Manufacturer.Contains(term)
                || d.OperatingSystem.Contains(term)
                || d.OsVersion.Contains(term)
                || d.Processor.Contains(term)
                || d.Description.Contains(term))
            .OrderBy(x => x.Name)
            .ToList();
    }

    public Device? GetByIdTracked(int id)
    {
        return db.Devices.FirstOrDefault(x => x.Id == id);
    }

    public Device Create(Device device)
    {
        db.Devices.Add(device);
        db.SaveChanges();
        return device;
    }

    public bool Update(Device device)
    {
        var existing = db.Devices.FirstOrDefault(x => x.Id == device.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = device.Name;
        existing.Manufacturer = device.Manufacturer;
        existing.Type = device.Type;
        existing.OperatingSystem = device.OperatingSystem;
        existing.OsVersion = device.OsVersion;
        existing.Processor = device.Processor;
        existing.RamAmountGb = device.RamAmountGb;
        existing.Description = device.Description;
        db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = db.Devices.FirstOrDefault(x => x.Id == id);
        if (existing is null)
        {
            return false;
        }

        db.Devices.Remove(existing);
        db.SaveChanges();
        return true;
    }

    public void SaveChanges()
    {
        db.SaveChanges();
    }

    public void Remove(Device entity)
    {
        db.Devices.Remove(entity);
    }
}
