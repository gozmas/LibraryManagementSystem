using System.Security.Claims;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Responses;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Member,Student")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyFines()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var fines = await _fineService.GetMyFinesAsync(userId.Value);

        _logger.LogInformation(
            "My fines retrieved successfully. UserId: {UserId}",
            userId.Value);

        return Ok(new ApiResponse<IEnumerable<FineDto>>(
            true,
            "My fines retrieved successfully.",
            fines));
    }

    [Authorize(Roles = "Admin,Member,Student")]
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> PayFine(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var isAdmin = User.IsInRole("Admin");

        var success = await _fineService.PayFineAsync(id, userId.Value, isAdmin);

        if (!success)
        {
            _logger.LogWarning(
                "Fine payment failed. FineId: {FineId}, UserId: {UserId}",
                id,
                userId.Value);

            return BadRequest(new ApiResponse<object>(
                false,
                "Fine not found, already paid, or you are not allowed to pay this fine.",
                null));
        }

        _logger.LogInformation(
            "Fine paid successfully. FineId: {FineId}, UserId: {UserId}",
            id,
            userId.Value);

        return Ok(new ApiResponse<object>(
            true,
            "Fine paid successfully.",
            null));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}