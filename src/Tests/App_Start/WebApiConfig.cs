namespace Tests
{
    using System.Web.Http;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Add compression message handler
            config.MessageHandlers.Insert(0, new ServerCompressionHandler(0, new GZipCompressor(), new DeflateCompressor()));

            // Configure error details policy
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.EnsureInitialized();
        }
    }
}