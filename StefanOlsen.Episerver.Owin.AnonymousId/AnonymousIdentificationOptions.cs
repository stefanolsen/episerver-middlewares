using System;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    public class AnonymousIdentificationOptions
    {
        public AnonymousIdentificationOptions()
        {
            CookieExpiration = TimeSpan.FromMinutes(100000);
            CookiePath = "/";
            CookieSlidingExpiration = true;
        }

        public string CookieName { get; set; }
        public string CookieDomain { get; set; }
        public string CookiePath { get; set; }
        public bool CookieRequireSSL { get; set; }
        public TimeSpan CookieExpiration { get; set; }
        public SameSiteMode? CookieSameSiteMode { get; set; }
        public bool CookieSlidingExpiration { get; set; }
        

        public ICookieManager CookieManager { get; set; }

        public string HeaderName { get; set; }

        public ISecureDataFormat<AnonymousId> AnonymousIdDataFormat { get; set; }
    }
}