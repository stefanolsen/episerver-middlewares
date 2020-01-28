using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataProtection;
using Owin;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    public class AnonymousIdentificationMiddleware : OwinMiddleware
    {
        private readonly ILogger _logger;
        private readonly AnonymousIdentificationOptions _options;

        public AnonymousIdentificationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            AnonymousIdentificationOptions options)
            : base(next)
        {
            _options = options;
            _logger = app.CreateLogger<AnonymousIdentificationMiddleware>();

            if (string.IsNullOrEmpty(options.CookieName))
            {
                options.CookieName = ".ASPXANONYMOUS";
            }

            if (string.IsNullOrEmpty(options.HeaderName))
            {
                options.HeaderName = "X-AnonymousId";
            }

            if (options.CookieManager == null)
            {
                options.CookieManager = new ChunkingCookieManager();
            }

            if (options.AnonymousIdDataFormat == null)
            {
                IDataProtector protector = app.CreateDataProtector(
                    typeof(AnonymousIdentificationMiddleware).FullName,
                    "Anonymous",
                    "v1");
                options.AnonymousIdDataFormat = new AnonymousIdDataFormat(protector);
            }
        }

        public override Task Invoke(IOwinContext context)
        {
            string cookieValue = _options.CookieManager.GetRequestCookie(context, _options.CookieName);
            string headerValue = context.Request.Headers[_options.HeaderName];

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                HandleHeader(context, headerValue);
            }
            else
            {
                HandleCookie(context, cookieValue);
            }

            

            return Next.Invoke(context);
        }

        private void HandleCookie(IOwinContext context, string cookieValue)
        {
            bool authenticated = context.Request.User?.Identity != null &&
                                 context.Request.User.Identity.IsAuthenticated;

            string anonymousId = null;
            AnonymousId anonymousIdData = null;

            // Read the Anonymous ID.
            if (cookieValue != null && cookieValue.Length > 1)
            {
                anonymousIdData = _options.AnonymousIdDataFormat.Unprotect(cookieValue);
                if (anonymousIdData?.Id != null)
                {
                    // Use the provided ID if available.
                    anonymousId = anonymousIdData.Id;

                    _logger.WriteVerbose($"Adding the incoming AnonymousID ('{anonymousId}') to OWIN context.");

                    context.Set(OwinConstants.AnonymousId, anonymousId);
                }
            }

            if (authenticated)
            {
                // If authenticated, don't send an anonymous cookie.
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    // If the request is authenticated AND has anonymous ID, it must be because we just authenticated.
                    // It is safe to delete the cookie now. Migration of carts and orders should be handled later in the pipeline.
                    _logger.WriteVerbose($"Deleting cookie named {_options.CookieName}.");

                    _options.CookieManager.DeleteCookie(
                        context,
                        _options.CookieName,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Domain = _options.CookieDomain,
                            Path = _options.CookiePath,
                            SameSite = _options.CookieSameSiteMode,
                            Secure = _options.CookieRequireSSL
                        });
                }

                _logger.WriteVerbose("The request is authenticated. Skipping sending new cookie.");

                return;
            }

            bool setCookie = false;
            if (anonymousId == null)
            {
                anonymousId = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);

                _logger.WriteVerbose($"Adding the newly generated AnonymousID ('{anonymousId}') to OWIN context.");

                context.Set(OwinConstants.AnonymousId, anonymousId);
                setCookie = true;
            }

            DateTime utcNow = DateTime.UtcNow;
            if (!setCookie && _options.CookieSlidingExpiration)
            {
                // If the cookie has already expired, but not deleted, renew the cookie.
                if (anonymousIdData.ExpireDate < utcNow)
                {
                    _logger.WriteVerbose("Extending expiration of expired cookie.");
                    setCookie = true;
                }
                else
                {
                    // If the cookie has not expired, but more than half the remaining time has passed, renew the cookie.
                    double seconds = (anonymousIdData.ExpireDate - utcNow).TotalSeconds;
                    if (seconds < _options.CookieExpiration.TotalSeconds / 2)
                    {
                        _logger.WriteVerbose("Extending expiration of still-valid cookie.");
                        setCookie = true;
                    }
                }
            }

            if (!setCookie)
            {
                _logger.WriteVerbose("The cookie is not about to expire. Skipping sending new cookie.");
                return;
            }

            if (_options.CookieRequireSSL && !context.Request.IsSecure)
            {
                _logger.WriteWarning("The cookie cannot be set. CookieRequireSSL is set to true, but the connection is not secure.");
                return;
            }

            DateTime expirationDate = DateTime.UtcNow + _options.CookieExpiration;
            anonymousIdData = new AnonymousId(anonymousId, expirationDate);
            cookieValue = _options.AnonymousIdDataFormat.Protect(anonymousIdData);

            _logger.WriteVerbose(
                $"Sending cookie named {_options.CookieName}, with expiration in {_options.CookieExpiration.TotalMinutes:N0} minutes.");

            _options.CookieManager.AppendResponseCookie(
                context,
                _options.CookieName,
                cookieValue,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = expirationDate,
                    Domain = _options.CookieDomain,
                    Path = _options.CookiePath,
                    SameSite = _options.CookieSameSiteMode,
                    Secure = _options.CookieRequireSSL
                });
        }

        private void HandleHeader(IOwinContext context, string headerValue)
        {
            bool authenticated = context.Request.User?.Identity != null &&
                                 context.Request.User.Identity.IsAuthenticated;

            context.Set(OwinConstants.AnonymousId, headerValue);

            if (!authenticated)
            {
                return;
            }

            // Send a signal to the client that the anonymous header should not be sent anymore.
            _logger.WriteVerbose($"Sending signal in header to stop sending anonymous header value.");

            context.Response.Headers[_options.HeaderName] = "authenticated";
        }
    }
}