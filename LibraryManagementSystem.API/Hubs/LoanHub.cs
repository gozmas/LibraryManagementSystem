using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LibraryManagementSystem.API.Hubs;

// Bu hub'ın kendi içinde bir metodu yok; sadece bağlı client'ları
// tutan bir "canlı yayın noktası" görevi görüyor. Event'ler
// LoanService içinden IHubContext<LoanHub> üzerinden gönderiliyor.
[Authorize]
public class LoanHub : Hub
{
}