using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;

namespace LibraryManagementSystem.API.Services.Implementations;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await _memberRepository.GetAllAsync();
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    public async Task<Member?> GetByUserIdAsync(int userId)
    {
        return await _memberRepository.GetByUserIdAsync(userId);
    }

    public async Task AddAsync(Member member)
    {
        await _memberRepository.AddAsync(member);
    }

    public async Task UpdateAsync(Member member)
    {
        await _memberRepository.UpdateAsync(member);
    }

    public async Task DeleteAsync(Member member)
    {
        await _memberRepository.DeleteAsync(member);
    }
}