namespace Edutor.Core.Modules.Subscription.Responses;

public record SubscriptionPlanInfo(
    long Id,
    string Code,
    string Name,
    decimal MonthlyPrice,
    string Currency,
    int YearlyFreeMonths);

/// <summary>Cari istifadə və effektiv limit (plan + əlavə paketlər).</summary>
public record SubscriptionUsageResponse(
    long? PlanId,
    string PlanCode,
    string PlanName,
    string Status,
    int? DaysLeft,
    DateTimeOffset? ExpiresAt,
    int Students,
    int MaxStudents,
    int Teachers,
    int MaxTeachers,
    int Groups,
    int MaxGroups,
    int Subjects,
    int MaxSubjects,
    int Lessons,
    int MaxLessons,
    int VideoMinutes,
    int MonthlyVideoMinutes,
    long StorageBytes,
    long MaxStorageBytes,
    int MaxVideoHeight,
    bool AllowVideoUpload,
    bool AllowAudioUpload,
    bool AllowFileUpload,
    IReadOnlyList<string> Features,
    IReadOnlyList<AddonOfferResponse> AddonOffers);

public record SubscriptionResponse(
    SubscriptionPlanInfo? Plan,
    SubscriptionUsageResponse Usage);

public record AddonOfferResponse(
    string Kind,
    string Label,
    decimal UnitPrice,
    string Unit);

public record PlanResponse(
    long Id,
    string Code,
    string Name,
    string? Description,
    decimal MonthlyPrice,
    string Currency,
    int YearlyFreeMonths,
    int MaxTeachers,
    int MaxStudents,
    int MaxGroups,
    int MaxSubjects,
    int MaxLessons,
    int MonthlyVideoMinutes,
    long StorageBytes,
    int ConcurrentConversions,
    int MaxVideoHeight,
    decimal PricePerStudent,
    decimal PricePerGroup,
    decimal PricePerVideoHour,
    int OriginalRetentionDays,
    bool AllowVideoUpload,
    bool AllowAudioUpload,
    bool AllowFileUpload,
    string Features,
    bool IsActive,
    int SortOrder);

public record TenantAddonResponse(
    long Id,
    string Period,
    string Kind,
    int Quantity,
    decimal Amount,
    string Currency,
    string? Note,
    DateTimeOffset CreatedAt);
