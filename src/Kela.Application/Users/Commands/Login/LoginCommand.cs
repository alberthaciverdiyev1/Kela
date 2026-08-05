using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Users.Commands.Login;

/// <summary>
/// Kimlik doğrulama komutu. Tenant, middleware tarafından istek bazlı çözüldüğü için
/// bu komut tenant-scoped çalışır (UserRepository sorgusu tenant filter'a tabidir).
/// Geçersiz kimlik bilgisi → null döner (controller 401'e çevirir).
/// </summary>
public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResult?>;
