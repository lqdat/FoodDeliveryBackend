namespace FoodDeliveryBackend.Core.Entities.Approval;

/// <summary>
/// Actions that can be performed on an approval request.
/// Used for audit logging.
/// </summary>
public enum ApprovalAction
{
    Submit = 0,
    Approve = 1,
    Reject = 2
}
