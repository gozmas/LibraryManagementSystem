using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/fines")]
[ApiController]
public class FineController : ControllerBase
{
    private readonly IFineService _fineService;

    public FineController(IFineService fineService)
    {
        _fineService = fineService;
    }
[Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetFines()
    {
        var fines = await _fineService.GetAllAsync();
        return Ok(fines);
    }
[Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFine(int id)
    {
        var fine = await _fineService.GetByIdAsync(id);

        if (fine == null)
        {
            return NotFound();
        }

        return Ok(fine);
    }
[Authorize(Roles = "Admin,Student")]
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> PayFine(int id)
    {
        var success = await _fineService.PayFineAsync(id);

        if (!success)
        {
            return BadRequest("Fine not found or already paid.");
        }

        return Ok(new
        {
            message = "Fine paid successfully."
        });
    }
}