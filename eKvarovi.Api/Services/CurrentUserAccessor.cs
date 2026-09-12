using System.Security.Claims;

namespace eKvarovi.Api.Services;

public static class CurrentUserAccessor
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static int? GetEmployeeId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("employeeId");
        return int.TryParse(value, out var id) ? id : null;
    }

    public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
        => roles.Any(user.IsInRole);
}