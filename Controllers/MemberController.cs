using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/members")]
[ApiController]
public class MemberController : ControllerBase
{
    private readonly AppDbContext _context;

    public MemberController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        return await _context.Members.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return member;
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, Member updatedMember)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        member.FirstName = updatedMember.FirstName;
        member.LastName = updatedMember.LastName;
        member.Email = updatedMember.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}