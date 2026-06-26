using LibraryManagementSystem.API.Responses;
using LibraryManagementSystem.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/fines")]
[ApiController]
public class FineController : ControllerBase
{
    private readonly IFineService _fineService;
    private readonly ILogger<FineController> _logger;

    public FineController(
        IFineService fineService,
        ILogger<FineController> logger)
    {
        _fineService = fineService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetFines()
    {
        var fines = await _fineService.GetAllAsync();

        _logger.LogInformation("Fines listed successfully.");

        return Ok(new ApiResponse<IEnumerable<FineDto>>(
            true,
            "Fines retrieved successfully.",
            fines));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFine(int id)
    {
        var fine = await _fineService.GetByIdAsync(id);

        if (fine == null)
        {
            _logger.LogWarning("Fine with ID {FineId} was not found.", id);

            return NotFound(new ApiResponse<object>(
                false,
                "Fine not found.",
                null));
        }

        _logger.LogInformation("Fine with ID {FineId} retrieved successfully.", id);

        return Ok(new ApiResponse<FineDto>(
            true,
            "Fine retrieved successfully.",
            fine));
    }

    [Authorize(Roles = "Admin,Student")]
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> PayFine(int id)
    {
        var success = await _fineService.PayFineAsync(id);

        if (!success)
        {
            _logger.LogWarning("Fine payment failed. FineId: {FineId}", id);

            return BadRequest(new ApiResponse<object>(
                false,
                "Fine not found or already paid.",
                null));
        }

        _logger.LogInformation("Fine paid successfully. FineId: {FineId}", id);

        return Ok(new ApiResponse<object>(
            true,
            "Fine paid successfully.",
            null));
    }
}