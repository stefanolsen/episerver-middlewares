# OWIN middleware for Episerver

## Anonymous Identification
The Anonymous Identification middleware supports anonymous ID from both a cookie and a custom header.

### Configuration options
The middleware can be configured by creating an instance of the `AnonymousIdentificationOptions` class and setting the following properties, as needed.

| Property | Notes |
|---|---|
| CookieName | Name of the cookie (defaults to `".ASPXANONYMOUS"`). |
| CookieDomain | Domain for which the cookie may be used for (defaults to `null`). |
| CookiePath | Path for which the cookie may be used for (defaults to `"/"`). |
| CookieExpiration | The maximum time in which the cookie is valid (defaults to 100000 minutes). |
| CookieRequireSSL | Specifies whether or not the cookie should only be sent over HTTPS connections (defaults to `false`). |
| CookieSameSiteMode | Specifies the SameSite mode of the cookie (defaults to `Strict`). |
| CookieSlidingExpiration | Specifies whether or not the cookie expiration should be continuously extended on requests (defaults `true`).  |
| CookieManager | Specifies the `ICookieManager` implementation that the middleware should use (defaults to `ChunkingCookieManager`). |
| HeaderName | Name of the customer header (defaults to `"X-AnonymousId"`). |
| AnonymousIdDataFormat | Specifies the `ISecureDataFormat<AnonymousId>` implementation that the middleware should use (defaults to `AnonymousIdDataFormat`). |

## Profile Migration
The Profile Migration middleware performs migration of carts, wish-lists and orders from an anonymous user to a logged-in user, when logging in to a user account.

## Examples

### Setting up Anonymous Identification and Profile Migration
Add the two middlewares to your Startup class, as shown below.
```
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;
using StefanOlsen.Episerver.Owin.AnonymousId;
using StefanOlsen.Episerver.Owin.ProfileMigration;

[assembly: OwinStartup(typeof(Startup))]
public class Startup
{
	public void Configuration(IAppBuilder app)
	{
		// TODO: Add your own authentication middleware here.
		app.UseStageMarker(PipelineStage.Authenticate);
		app.UseAnonymousAuthentication(new AnonymousIdentificationOptions
		{
			CookieRequireSSL = false,
			CookieSlidingExpiration = true,
			CookieSameSiteMode = SameSiteMode.Strict
		});
		app.UseProfileMigration();
		app.UseStageMarker(PipelineStage.PostAuthenticate);

		// TODO: Add your own remaining middleware here.
	}
}
```

### Using ContactId/AnonymousID in code
When your code needs to get the ContactId for any purpose, use the new extension method, `OwinPrincipalExtensions.GetContactId` instead of the built-in extension `PrincipalExtensions.GetContactId` from the `Mediachase.Commerce.Security` namespace.

In many Episerver Commerce solutions, this is mainly called from the `CartService`. So make sure any usages are changed.