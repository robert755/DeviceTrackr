using DeviceTrackr.Api.Dtos;
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
        var (success, error) = service.Update(id, device);
        if (success)
        {
            return NoContent();
        }

        if (error == "assigned")
        {
            return BadRequest(new { message = "Dispozitivul este alocat unui utilizator; nu poate fi modificat." });
        }

        return NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var (success, error) = service.Delete(id);
        if (success)
        {
            return NoContent();
        }

        if (error == "assigned")
        {
            return BadRequest(new { message = "Dispozitivul este alocat; dealocă-l înainte de ștergere." });
        }

        return NotFound();
    }

    [HttpPost("{id:int}/assign")]
    public IActionResult Assign(int id, [FromBody] AssignDeviceRequestDto request)
    {
        var result = service.AssignToUser(id, request);
        return result.Success ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:int}/unassign")]
    public IActionResult Unassign(int id, [FromBody] AssignDeviceRequestDto request)
    {
        var result = service.UnassignFromUser(id, request);
        return result.Success ? NoContent() : BadRequest(new { message = result.Error });
    }
}
