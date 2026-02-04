using API.Data;
using API.Model.UserModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Resources
{
    public class IsAccssAuthorizationHandler : AuthorizationHandler<IsAccssRequirement>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IsAccssAuthorizationHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAccssRequirement requirement)
        {
            var CurrentRoute = _httpContextAccessor.HttpContext.Request.Path;
            var userRoleInClaim = context.User.FindFirstValue(ClaimTypes.Role);
            var userCodeInClaim = context.User.FindFirstValue(ClaimTypes.SerialNumber);
            if (CurrentRoute == null || userCodeInClaim == null || userCodeInClaim == null)
            {
                return Task.CompletedTask;
            }

            //var userRoleInDb = _dbContext.UserRole.FirstOrDefault(r => r.RoleCode == userRoleInClaim);


            //var RoleAccessInDb = _dbContext.RouteAccess.Include(r => r.Route).ToList().Where(d => d.Route.Path == CurrentRoute && d.RoleCode == userRoleInClaim);

            //if (userRoleInDb != null && userRoleInDb.RoleLevel == RoleLevels.SUPREME)
            //{
            //    context.Succeed(requirement);
            //}
            //else if (userRoleInDb != null && RoleAccessInDb != null && RoleAccessInDb.Any(x => x.Status == true))
            //{
            //    context.Succeed(requirement);
            //}
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

}
