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

    /// <summary>Uses Google Gemini to write a description from device specs and saves it to Description.</summary>
    [HttpPost("{id:int}/generate-description")]
    public async Task<ActionResult<Device>> GenerateDescription(int id, CancellationToken cancellationToken)
    {
        var (success, error, device, geminiDetail) = await service.GenerateAiDescriptionAsync(id, cancellationToken);
        if (success && device is not null)
        {
            return Ok(device);
        }

        return error switch
        {
            "no_api_key" => BadRequest(new
            {
                message = "Gemini API key is not configured. Put your key in appsettings.json under Gemini:ApiKey (local only), or set env Gemini__ApiKey, or: dotnet user-secrets set \"Gemini:ApiKey\" \"YOUR_KEY\"."
            }),
            "gemini_failed" => StatusCode(502, new
            {
                message = geminiDetail ?? "Gemini did not return usable text.",
                detail = geminiDetail
            }),
            _ => NotFound()
        };
    }
}
