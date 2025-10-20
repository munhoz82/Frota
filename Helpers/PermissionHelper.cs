using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrotaTaxi.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this ClaimsPrincipal user, string controller, string action = "View")
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            var requiredPermission = $"{controller}:{action}";
            var userPermissions = user.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            return userPermissions.Contains(requiredPermission);
        }

        public static bool CanView(this ClaimsPrincipal user, string controller)
        {
            return user.HasPermission(controller, "View");
        }

        public static bool CanEdit(this ClaimsPrincipal user, string controller)
        {
            return user.HasPermission(controller, "Edit");
        }
    }
}
