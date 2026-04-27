using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Dashboard;

public class GetDashboardRequest : IRequest<DashboardMetricsResponse> { }