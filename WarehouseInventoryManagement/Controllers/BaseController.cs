using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

public abstract class BaseController : Controller
{
    private IMediator? _mediator;
    // Uses Null Coalescing Assignment to get the mediator service
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}