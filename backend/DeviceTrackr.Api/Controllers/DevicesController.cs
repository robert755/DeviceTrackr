using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceTrackr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(DeviceService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Device>> GetAll()
    {
        return Ok(service.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Device> GetById(int id)
    {
        var device = service.GetById(id);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpPost]
    public ActionResult<Device> Create(Device device)
    {
        var created = service.Create(device);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Device device)
    {
        var updated = service.Update(id, device);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleted = service.Delete(id);
        return deleted ? NoContent() : NotFound();
    }
}
