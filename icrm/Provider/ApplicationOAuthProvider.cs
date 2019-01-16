using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using icrm.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace icrm.Provider
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private ApplicationUser user = null;

      

        public override async Task GrantResourceOwnerCredentials(
            OAuthGrantResourceOwnerCredentialsContext context)
          {
           

            ApplicationUserManager storeUserMgr =
            context.OwinContext.Get<ApplicationUserManager>("AspNet.Identity.Owin:"
            + typeof(ApplicationUserManager).AssemblyQualifiedName);
            user = await storeUserMgr.FindAsync(context.UserName,
            context.Password);
            if (user == null)
            {
                context.SetError("invalid_grant",
                "The username or password is incorrect");
            }
            else
            {
                ClaimsIdentity ident = await storeUserMgr.CreateIdentityAsync(user,
             "Custom");

                AuthenticationTicket ticket
                = new AuthenticationTicket(ident, new AuthenticationProperties());
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(ident);
            }
        }
        public override Task ValidateClientAuthentication(
        OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            ApplicationDbContext appcontext = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(appcontext));

            foreach (IdentityUserRole r in user.Roles)
            {
                context.AdditionalResponseParameters.Add("Roles", roleManager.FindById(r.RoleId).Name);
                context.AdditionalResponseParameters.Add("UserName", user.FirstName);
            }
           
           return Task.FromResult<object>(null);
        }
    }
}
