using FoodDeliveryBackend.Core.Entities.Admin;
using FoodDeliveryBackend.Core.Entities.Approval;
using FoodDeliveryBackend.Core.Entities.ChainOwner;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Generic approval service implementation.
/// Handles state machine transitions and entity activation upon final approval.
/// </summary>
public class ApprovalService : IApprovalService
{
    private readonly FoodDeliveryDbContext _context;

    public ApprovalService(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovalRequest> SubmitForApprovalAsync(
        EntityType entityType,
        Guid entityId,
        string regionCode,
        Guid submitterId,
        string submitterAccountType)
    {
        // Check if there's already a pending request for this entity
        var existing = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.EntityType == entityType && 
                                      r.EntityId == entityId &&
                                      r.CurrentStatus != ApprovalStatus.Approved &&
                                      r.CurrentStatus != ApprovalStatus.RejectedByRegion &&
                                      r.CurrentStatus != ApprovalStatus.RejectedByMaster);

        if (existing != null)
        {
            throw new InvalidOperationException($"Entity already has a pending approval request: {existing.Id}");
        }

        var request = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            CurrentStatus = ApprovalStatus.Submitted,
            RegionCode = regionCode,
            CreatedAt = DateTime.UtcNow
        };

        var log = new ApprovalLog
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = request.Id,
            Action = ApprovalAction.Submit,
            FromStatus = null,
            ToStatus = ApprovalStatus.Submitted,
            PerformedBy = submitterId,
            PerformerRole = null,
            PerformerAccountType = submitterAccountType,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalRequests.Add(request);
        _context.ApprovalLogs.Add(log);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<IEnumerable<ApprovalRequest>> GetPendingForRegionAsync(string regionCode)
    {
        return await _context.ApprovalRequests
            .Include(r => r.ApprovalLogs)
            .Where(r => r.RegionCode == regionCode && 
                        r.CurrentStatus == ApprovalStatus.Submitted)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApprovalRequest>> GetPendingForMasterAsync()
    {
        return await _context.ApprovalRequests
            .Include(r => r.ApprovalLogs)
            .Where(r => r.CurrentStatus == ApprovalStatus.ApprovedByRegion)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ApprovalRequest> ApproveByRegionAsync(Guid requestId, Guid adminId, string? reason = null)
    {
        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException($"Approval request not found: {requestId}");

        if (request.CurrentStatus != ApprovalStatus.Submitted)
            throw new InvalidOperationException($"Request is not in Submitted status. Current: {request.CurrentStatus}");

        var oldStatus = request.CurrentStatus;
        request.CurrentStatus = ApprovalStatus.ApprovedByRegion;
        request.UpdatedAt = DateTime.UtcNow;

        var log = new ApprovalLog
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = request.Id,
            Action = ApprovalAction.Approve,
            FromStatus = oldStatus,
            ToStatus = ApprovalStatus.ApprovedByRegion,
            PerformedBy = adminId,
            PerformerRole = AdminRole.AdminRestaurantRegion,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalLogs.Add(log);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<ApprovalRequest> RejectByRegionAsync(Guid requestId, Guid adminId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for rejection");

        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException($"Approval request not found: {requestId}");

        if (request.CurrentStatus != ApprovalStatus.Submitted)
            throw new InvalidOperationException($"Request is not in Submitted status. Current: {request.CurrentStatus}");

        var oldStatus = request.CurrentStatus;
        request.CurrentStatus = ApprovalStatus.RejectedByRegion;
        request.UpdatedAt = DateTime.UtcNow;

        var log = new ApprovalLog
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = request.Id,
            Action = ApprovalAction.Reject,
            FromStatus = oldStatus,
            ToStatus = ApprovalStatus.RejectedByRegion,
            PerformedBy = adminId,
            PerformerRole = AdminRole.AdminRestaurantRegion,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalLogs.Add(log);
        
        // Update entity status to Rejected
        await UpdateEntityStatusAsync(request.EntityType, request.EntityId, false);
        
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<ApprovalRequest> ApproveByMasterAsync(Guid requestId, Guid adminId, string? reason = null)
    {
        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException($"Approval request not found: {requestId}");

        if (request.CurrentStatus != ApprovalStatus.ApprovedByRegion)
            throw new InvalidOperationException($"Request must be ApprovedByRegion first. Current: {request.CurrentStatus}");

        // Validate parent entity is active for hierarchical entities
        await ValidateParentActiveAsync(request.EntityType, request.EntityId);

        var oldStatus = request.CurrentStatus;
        request.CurrentStatus = ApprovalStatus.Approved;
        request.UpdatedAt = DateTime.UtcNow;

        var log = new ApprovalLog
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = request.Id,
            Action = ApprovalAction.Approve,
            FromStatus = oldStatus,
            ToStatus = ApprovalStatus.Approved,
            PerformedBy = adminId,
            PerformerRole = AdminRole.AdminRestaurantMaster,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalLogs.Add(log);
        
        // Activate the entity
        await UpdateEntityStatusAsync(request.EntityType, request.EntityId, true);
        
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<ApprovalRequest> RejectByMasterAsync(Guid requestId, Guid adminId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for rejection");

        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException($"Approval request not found: {requestId}");

        if (request.CurrentStatus != ApprovalStatus.ApprovedByRegion)
            throw new InvalidOperationException($"Request must be ApprovedByRegion first. Current: {request.CurrentStatus}");

        var oldStatus = request.CurrentStatus;
        request.CurrentStatus = ApprovalStatus.RejectedByMaster;
        request.UpdatedAt = DateTime.UtcNow;

        var log = new ApprovalLog
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = request.Id,
            Action = ApprovalAction.Reject,
            FromStatus = oldStatus,
            ToStatus = ApprovalStatus.RejectedByMaster,
            PerformedBy = adminId,
            PerformerRole = AdminRole.AdminRestaurantMaster,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalLogs.Add(log);
        
        // Update entity status to Rejected
        await UpdateEntityStatusAsync(request.EntityType, request.EntityId, false);
        
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<ApprovalRequest?> GetByIdAsync(Guid requestId)
    {
        return await _context.ApprovalRequests
            .Include(r => r.ApprovalLogs.OrderBy(l => l.CreatedAt))
            .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task<(IEnumerable<ApprovalLog> Logs, int TotalCount)> GetAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        EntityType? entityTypeFilter = null,
        string? regionCodeFilter = null)
    {
        var query = _context.ApprovalLogs
            .Include(l => l.ApprovalRequest)
            .AsQueryable();

        if (entityTypeFilter.HasValue)
        {
            query = query.Where(l => l.ApprovalRequest.EntityType == entityTypeFilter.Value);
        }

        if (!string.IsNullOrEmpty(regionCodeFilter))
        {
            query = query.Where(l => l.ApprovalRequest.RegionCode == regionCodeFilter);
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }

    /// <summary>
    /// Update entity status based on approval result.
    /// </summary>
    private async Task UpdateEntityStatusAsync(EntityType entityType, Guid entityId, bool approved)
    {
        switch (entityType)
        {
            case EntityType.ChainOwner:
                var chainOwner = await _context.ChainOwnerAccounts.FindAsync(entityId);
                if (chainOwner != null)
                {
                    chainOwner.Status = approved ? AccountStatus.Active : AccountStatus.Rejected;
                    chainOwner.UpdatedAt = DateTime.UtcNow;
                }
                break;

            case EntityType.StoreAccount:
                var store = await _context.StoreAccounts.FindAsync(entityId);
                if (store != null)
                {
                    store.Status = approved ? AccountStatus.Active : AccountStatus.Rejected;
                    store.UpdatedAt = DateTime.UtcNow;
                }
                break;

            case EntityType.StoreManager:
                var manager = await _context.StoreManagers.FindAsync(entityId);
                if (manager != null)
                {
                    manager.Status = approved ? AccountStatus.Active : AccountStatus.Rejected;
                    manager.UpdatedAt = DateTime.UtcNow;
                }
                break;

            case EntityType.Food:
                var food = await _context.Foods.FindAsync(entityId);
                if (food != null)
                {
                    food.Status = approved ? FoodStatus.Published : FoodStatus.Rejected;
                    food.UpdatedAt = DateTime.UtcNow;
                }
                break;
        }
    }

    /// <summary>
    /// Validate that parent entity is active before approving child entity.
    /// Business rule: Child cannot be active if parent is not active.
    /// </summary>
    private async Task ValidateParentActiveAsync(EntityType entityType, Guid entityId)
    {
        switch (entityType)
        {
            case EntityType.StoreAccount:
                var store = await _context.StoreAccounts
                    .Include(s => s.ChainOwner)
                    .FirstOrDefaultAsync(s => s.Id == entityId);
                if (store?.ChainOwner.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException("Cannot approve Store: ChainOwner is not Active");
                }
                break;

            case EntityType.StoreManager:
                var manager = await _context.StoreManagers
                    .Include(m => m.Store)
                    .FirstOrDefaultAsync(m => m.Id == entityId);
                if (manager?.Store.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException("Cannot approve Manager: Store is not Active");
                }
                break;

            case EntityType.Food:
                var food = await _context.Foods
                    .Include(f => f.Store)
                    .FirstOrDefaultAsync(f => f.Id == entityId);
                if (food?.Store.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException("Cannot publish Food: Store is not Active");
                }
                break;
        }
    }
}
