using Kela.Core.Common.Exceptions;

namespace Kela.Core.Common.Authorization;

/// <summary>Servislərdə təkrarlanan sahiblik yoxlamaları.</summary>
public static class OwnershipGuard
{
    /// <summary>Dəyişmək üçün yalnız sahibi olduğu qeyd (admin üçün məhdudiyyət yoxdur).</summary>
    public static void EnsureOwned(long? ownerId, long? userId, string message)
    {
        if (ownerId is long id && userId != id)
        {
            throw new ForbiddenException(message);
        }
    }

    /// <summary>Oxumaq üçün öz qeydi və ya paylaşılan/sahibsiz qeyd.</summary>
    public static void EnsureReadable(long? ownerId, long? userId, bool allowedForOthers, string message)
    {
        if (ownerId is long id && userId != id && !allowedForOthers)
        {
            throw new ForbiddenException(message);
        }
    }
}
