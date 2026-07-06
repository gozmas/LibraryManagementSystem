using System.Security.Claims;

namespace LibraryManagementSystem.API.Extensions;

// LoanController, FineController ve MemberController'da birebir aynı
// şekilde tekrarlanan GetCurrentUserId() private metodunun yerine geçer.
// Herhangi bir controller'da `User.GetUserId()` olarak çağrılabilir.
public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
            return null;

        if (!int.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}