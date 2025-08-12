using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly int[] _allowedRoles;

    public AuthorizeRoleAttribute(params int[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var role = session.GetInt32("Role");

        if (!role.HasValue || !_allowedRoles.Contains(role.Value))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
        }
    }
}
