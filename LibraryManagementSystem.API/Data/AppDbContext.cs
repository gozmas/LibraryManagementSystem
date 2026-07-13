using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    // Data/AppDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Member>()
        .HasOne(m => m.User)
        .WithOne(u => u.Member)
        .HasForeignKey<Member>(m => m.UserId)
        .OnDelete(DeleteBehavior.SetNull);  // user silinse bile member kaydı kalsın

    // Aynı member aynı kitabı iki kez wishlist'e ekleyemesin.
    modelBuilder.Entity<Wishlist>()
        .HasIndex(w => new { w.MemberId, w.BookId })
        .IsUnique();
}

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Fine> Fines => Set<Fine>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
}