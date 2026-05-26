using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce_BE.Filters;

public class AuditLogActionFilter : IAsyncActionFilter
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogActionFilter(IAuditLogService auditLogService) => _auditLogService = auditLogService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        var httpMethod = context.HttpContext.Request.Method;
        if (httpMethod == HttpMethods.Get) return;

        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var controllerName = descriptor?.ControllerName ?? "Unknown";
        var actionName = descriptor?.ActionName ?? "Unknown";

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = context.HttpContext.User.FindFirstValue(ClaimTypes.Email);
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.HttpContext.Request.Headers.UserAgent.ToString();

        var statusCode = executed.Exception == null
            ? (executed.Result as Microsoft.AspNetCore.Mvc.ObjectResult)?.StatusCode ?? 200
            : 500;

        await _auditLogService.LogAsync(
            userId: userId,
            userEmail: userEmail,
            action: $"{httpMethod} {actionName}",
            entityType: controllerName,
            description: $"{httpMethod} {context.HttpContext.Request.Path}",
            ipAddress: ipAddress,
            userAgent: userAgent,
            httpStatusCode: statusCode
        );
    }
}
