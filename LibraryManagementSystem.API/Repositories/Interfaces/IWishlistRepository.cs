using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Repositories.Interfaces;

public interface IWishlistRepository
{
    Task<IEnumerable<Wishlist>> GetByMemberIdAsync(int memberId);
    Task<Wishlist?> GetByMemberAndBookAsync(int memberId, int bookId);
    Task AddAsync(Wishlist wishlist);
    Task RemoveAsync(Wishlist wishlist);

    // Bir kitap tekrar müsait hale geldiğinde, o kitabı wishlist'inde
    // bulunduran (ve giriş yapabilecek bir User hesabı olan) member'ları
    // bulmak için kullanılıyor; LoanService buradan gelen listeyle
    // SignalR üzerinden hedefli bildirim gönderiyor.
    Task<IEnumerable<Member>> GetMembersWishlistingBookAsync(int bookId);
}