using DanceSchoolApi.Contracts;
using DanceSchoolApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchoolApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController(SubscriptionService subscriptions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SubscriptionResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await subscriptions.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubscriptionResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await subscriptions.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionResponse>> Create(PurchaseSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.PurchaseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = subscription.SubscriptionId }, subscription);
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionResponse>> Cancel(int id, CancellationToken cancellationToken) =>
        Ok(await subscriptions.CancelAsync(id, cancellationToken));

    [HttpGet("active-details")]
    [Authorize]
    public async Task<ActionResult<List<SubscriptionResponse>>> GetActiveWithDetails(CancellationToken cancellationToken) =>
        Ok(await subscriptions.GetActiveAsync(cancellationToken));

    [HttpGet("expired")]
    [Authorize]
    public async Task<ActionResult<List<SubscriptionResponse>>> GetExpired(CancellationToken cancellationToken) =>
        Ok(await subscriptions.GetExpiredAsync(cancellationToken));
}