using System;
using System.Security.Principal;
using System.Web;
using Mediachase.Commerce.Security;
using Microsoft.Owin;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    public static class OwinPrincipalExtensions
    {
        public static Guid GetContactId(this IPrincipal principal)
        {
            return string.IsNullOrEmpty(principal?.Identity?.Name)
                ? GetAnonymousUserId()
                : principal.GetCustomerContact().PrimaryKeyId.Value;
        }

        private static Guid GetAnonymousUserId()
        {
            IOwinContext owinContext = HttpContext.Current?.GetOwinContext();
            string anonymousId = owinContext?.Get<string>(OwinConstants.AnonymousId);

            return string.IsNullOrEmpty(anonymousId)
                ? Guid.NewGuid()
                : new Guid(anonymousId);
        }
    }
}