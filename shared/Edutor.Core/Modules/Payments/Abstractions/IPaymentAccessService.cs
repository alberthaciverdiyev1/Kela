namespace Edutor.Core.Modules.Payments.Abstractions;

/// <summary><see cref="BlockAll"/> → bütün sistem; <see cref="BlockedGroupIds"/> → yalnız həmin qruplar bağlıdır.</summary>
public record PaymentAccessState(bool BlockAll, IReadOnlyList<long> BlockedGroupIds)
{
    public static readonly PaymentAccessState Allow = new(false, []);

    public bool IsGroupBlocked(long groupId) => BlockAll || BlockedGroupIds.Contains(groupId);
}

public interface IPaymentAccessService
{
    /// <summary>Ödənişi gecikən şagird üçün konfiqurasiyaya uyğun giriş məhdudiyyəti.</summary>
    Task<PaymentAccessState> GetAsync(long studentId, CancellationToken ct = default);
}
