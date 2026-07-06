namespace LibraryManagementSystem.API.Common;

// Servis katmanının "başarılı mı, değilse neden değil" bilgisini
// controller'a taşımasını sağlar. Amaç: LoanService gibi yerlerde
// tek bir "null" ile birden fazla farklı hata durumunu (kitap yok,
// kopya yok, üye yok, zaten iade edilmiş...) ayırt edilemez hale
// getirmek yerine, her durumun kendi mesajını ve HTTP kodunu taşıması.
public class ServiceResult<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    private ServiceResult(bool success, T? data, string? errorMessage, int statusCode)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ServiceResult<T> Ok(T data) =>
        new(true, data, null, StatusCodes.Status200OK);

    // statusCode: 404 (bulunamadı), 409 (durum uygun değil / çakışma),
    // 403 (yetkisiz erişim) gibi anlamlı kodlar vermek için var.
    public static ServiceResult<T> Fail(string message, int statusCode = StatusCodes.Status400BadRequest) =>
        new(false, default, message, statusCode);
}

// Microsoft.AspNetCore.Http içindeki StatusCodes'a bağımlı olmamak
// için (Services katmanı Http paketine bağımlı olmamalı) burada
// kendi sabitlerimizi tanımlıyoruz.
internal static class StatusCodes
{
    public const int Status200OK = 200;
    public const int Status400BadRequest = 400;
    public const int Status403Forbidden = 403;
    public const int Status404NotFound = 404;
    public const int Status409Conflict = 409;
}