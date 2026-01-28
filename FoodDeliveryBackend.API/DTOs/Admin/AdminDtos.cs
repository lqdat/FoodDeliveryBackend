using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryBackend.API.DTOs.Admin;

// ==================== Auth DTOs ====================

public record AdminLoginDto(
    [Required] string Email,
    [Required] string Password);

public record AdminLoginResponseDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? RegionCode,
    string Token);

// ==================== Approval DTOs ====================

public record ApprovalActionDto(
    string? Reason);

public record ApprovalRequestDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string CurrentStatus,
    string RegionCode,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ApprovalLogDto> Logs);

public record ApprovalLogDto(
    Guid Id,
    string Action,
    string? FromStatus,
    string ToStatus,
    Guid PerformedBy,
    string? PerformerRole,
    string? PerformerAccountType,
    string? Reason,
    DateTime CreatedAt);

public record ApprovalListResponseDto(
    List<ApprovalRequestDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

// ==================== Audit Log DTOs ====================

public record AuditLogDto(
    Guid Id,
    Guid ApprovalRequestId,
    string EntityType,
    Guid EntityId,
    string RegionCode,
    string Action,
    string? FromStatus,
    string ToStatus,
    Guid PerformedBy,
    string? PerformerRole,
    string? PerformerAccountType,
    string? Reason,
    DateTime CreatedAt);

public record AuditLogListResponseDto(
    List<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
