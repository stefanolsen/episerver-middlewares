using System;
using Microsoft.Owin.Extensions;
using Owin;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseAnonymousAuthentication(
            this IAppBuilder app)
        {
            return UseAnonymousAuthentication(app, new AnonymousIdentificationOptions());
        }

        public static IAppBuilder UseAnonymousAuthentication(
            this IAppBuilder app,
            AnonymousIdentificationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Use(typeof(AnonymousIdentificationMiddleware), app, options);
            app.UseStageMarker(PipelineStage.PostAuthenticate);

            return app;
        }
    }
}