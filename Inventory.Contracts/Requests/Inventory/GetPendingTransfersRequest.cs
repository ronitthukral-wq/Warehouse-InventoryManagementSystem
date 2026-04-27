using Inventory.Contracts.Responses;
using MediatR;
using System.Collections.Generic;

namespace Inventory.Contracts.Requests.Inventory;

// Ensure it returns a List to match the controller's 'transfers' variable
public class GetPendingTransfersRequest : IRequest<List<TransferRequestResponse>>
{
}