using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Repositories;

namespace DeviceTrackr.Api.Services;

public class UserService(UserRepository repo)
{
    public List<User> GetAll() => repo.GetAll();

    public User? GetById(int id) => repo.GetById(id);

    public User Create(User user)
    {
        return repo.Create(user);
    }

    public bool Update(int id, User user)
    {
        user.Id = id;
        return repo.Update(user);
    }

    public bool Delete(int id) => repo.Delete(id);
}
