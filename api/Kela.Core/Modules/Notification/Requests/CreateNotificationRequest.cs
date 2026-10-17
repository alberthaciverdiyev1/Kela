using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Notification.Requests;

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public IFormFile? Image { get; set; }

    /// <summary>true = bütün öğrencilere; false = StudentIds ilə seçilənlərə.</summary>
    public bool All { get; set; } = true;

    public List<long> StudentIds { get; set; } = new();

    public string? Data { get; set; }
}
