using System;
using Microsoft.Owin.Extensions;
using Owin;

namespace StefanOlsen.Episerver.Owin.ProfileMigration
{
    public static class AnonymousIdExtensions
    {
        public static IAppBuilder UseProfileMigration(
            this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Use(typeof(ProfileMigrationMiddleware), app);
            app.UseStageMarker(PipelineStage.PostAuthenticate);

            return app;
        }
    }
}