using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceTrackr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<User>> GetAll()
    {
        return Ok(service.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<User> GetById(int id)
    {
        var user = service.GetById(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public ActionResult<User> Create(User user)
    {
        var created = service.Create(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, User user)
    {
        var updated = service.Update(id, user);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleted = service.Delete(id);
        return deleted ? NoContent() : NotFound();
    }
}
