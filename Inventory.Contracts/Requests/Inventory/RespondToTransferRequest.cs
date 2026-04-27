using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Inventory;

public class RespondToTransferRequest : IRequest<ActionResponse>
{
    public int TransferRequestId { get; set; }
    public bool Accept { get; set; } // True = Accepted, False = Rejected
}