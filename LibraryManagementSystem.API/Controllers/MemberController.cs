using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Extensions;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Services.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/members")]
[ApiController]
public class MemberController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly AppDbContext _context;

    public MemberController(
        IMemberService memberService,
        AppDbContext context)
    {
        _memberService = memberService;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        var members = await _memberService.GetAllAsync();
        return Ok(members);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var member = await _memberService.GetByIdAsync(id);

        if (member == null)
            return NotFound();

        return Ok(member);
    }

    [Authorize(Roles = "Member,Student")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyMemberProfile()
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var member = await _memberService.GetByUserIdAsync(userId.Value);

        if (member == null)
            return NotFound("Member profile not found.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);

        return Ok(new
        {
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.UserId,
            Username = user?.Username
        });
    }

    [Authorize(Roles = "Member,Student")]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateMyProfileDto dto)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var member = await _memberService.GetByUserIdAsync(userId.Value);

        if (member == null)
            return NotFound("Member profile not found.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
            return NotFound("User account not found.");

        var emailAlreadyExists = await _context.Users.AnyAsync(u =>
            u.Email == dto.Email &&
            u.Id != user.Id);

        if (emailAlreadyExists)
            return BadRequest("This email is already used by another account.");

        var usernameAlreadyExists = await _context.Users.AnyAsync(u =>
            u.Username == dto.Username &&
            u.Id != user.Id);

        if (usernameAlreadyExists)
            return BadRequest("This username is already used by another account.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;

        user.Username = dto.Username;
        user.Email = dto.Email;

        await _memberService.UpdateAsync(member);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            data = new
            {
                member.Id,
                member.FirstName,
                member.LastName,
                member.Email,
                member.UserId,
                user.Username
            }
        });
    }

    [Authorize(Roles = "Member,Student")]
    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangeMyPassword(ChangePasswordDto dto)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        if (dto.NewPassword != dto.ConfirmNewPassword)
            return BadRequest("New password and confirmation password do not match.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
            return NotFound("User account not found.");

        var passwordHasher = new PasswordHasher<User>();

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.CurrentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
            return BadRequest("Current password is incorrect.");

        user.PasswordHash = passwordHasher.HashPassword(user, dto.NewPassword);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Password changed successfully."
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMember(int id, Member updatedMember)
    {
        var member = await _memberService.GetByIdAsync(id);

        if (member == null)
            return NotFound();

        member.FirstName = updatedMember.FirstName;
        member.LastName = updatedMember.LastName;
        member.Email = updatedMember.Email;

        await _memberService.UpdateAsync(member);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _memberService.GetByIdAsync(id);

        if (member == null)
            return NotFound();

        await _memberService.DeleteAsync(member);

        return NoContent();
    }

}