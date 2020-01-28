using System;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Security;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

namespace StefanOlsen.Episerver.Owin.ProfileMigration
{
    public class ProfileMigrationMiddleware : OwinMiddleware
    {
        private readonly ILogger _logger;

        public ProfileMigrationMiddleware(
            OwinMiddleware next,
            IAppBuilder app)
            : base(next)
        {
            _logger = app.CreateLogger<ProfileMigrationMiddleware>();
        }

        public override Task Invoke(IOwinContext context)
        {
            DataContext.Current.CurrentUserId = context.Request.User.GetContactId();

            bool authenticated = context.Request.User?.Identity != null &&
                                 context.Request.User.Identity.IsAuthenticated;

            string anonymousId = context.Get<string>(OwinConstants.AnonymousId);
            if (!authenticated || string.IsNullOrEmpty(anonymousId))
            {
                _logger.WriteVerbose("Skipping profile migration.");

                return Next.Invoke(context);
            }

            // Anti-pattern, but it saves on construction, those many times it is not actually needed.
            // Injected<> cannot be used in an OWIN middleware, because it is not constructed from an IOC container.
            IProfileMigrator profileMigrator = ServiceLocator.Current.GetInstance<IProfileMigrator>();

            // If the context is BOTH authenticated AND has an anonymousID, it must be because we just authenticated.
            // In this case, we have one chance to migrate all carts, wish-lists and orders from the anonymous ID to the customer ID.
            Guid guid = new Guid(anonymousId);

            _logger.WriteInformation($"Migrating carts, orders and wish-lists from anonymous ID: '{guid:D}'.");
            profileMigrator.MigrateCarts(guid);
            profileMigrator.MigrateOrders(guid);
            profileMigrator.MigrateWishlists(guid);

            return Next.Invoke(context);
        }
    }
}