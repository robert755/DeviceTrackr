using DeviceTrackr.Api.Data;
using DeviceTrackr.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceTrackr.Api.Repositories;

public class UserRepository(DeviceTrackrDbContext db)
{
    public List<User> GetAll()
    {
        return db.Users.AsNoTracking().OrderBy(x => x.Name).ToList();
    }

    public User? GetById(int id)
    {
        return db.Users.AsNoTracking().FirstOrDefault(x => x.Id == id);
    }

    public User Create(User user)
    {
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public bool Update(User user)
    {
        var existing = db.Users.FirstOrDefault(x => x.Id == user.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = user.Name;
        existing.Role = user.Role;
        existing.Location = user.Location;
        db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = db.Users.FirstOrDefault(x => x.Id == id);
        if (existing is null)
        {
            return false;
        }

        db.Users.Remove(existing);
        db.SaveChanges();
        return true;
    }
}
