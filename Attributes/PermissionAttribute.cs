using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace FrotaTaxi.Attributes
{
    public class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _controller;
        private readonly string _action;

        public PermissionAttribute(string controller, string action = "View")
        {
            _controller = controller;
            _action = action;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar se o usuário está autenticado
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Verificar se o usuário tem a permissão necessária
            var requiredPermission = $"{_controller}:{_action}";
            var userPermissions = context.HttpContext.User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            if (!userPermissions.Contains(requiredPermission))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }
        }
    }
}
