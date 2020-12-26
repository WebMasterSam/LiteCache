using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Cors.Internal;

using Swashbuckle.AspNetCore.Swagger;

using Newtonsoft.Json;

using LiteCache.Backend.Helpers;

namespace LiteCache.Backend.IIS
{
    public class Startup
    {
        private const string CORS_POLICY = "Default";
        private const string SWAGGER_NAME = "LiteCache";
        private const string SWAGGER_VERSION = "1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            ConfigHelper.SetConfigurationInstance(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CORS_POLICY,
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .SetIsOriginAllowed(origin => true)
                           .AllowCredentials();
                });
            });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                services.AddLogging(logging => logging.AddDebug().AddConsole().AddEventLog());
            else
                services.AddLogging(logging => logging.AddDebug().AddConsole());

            // Singleton – Created only for the first request. If a particular instance is specified at registration time, this instance will be provided to all consumers of the registration type.
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc(string.Format("v{0}", SWAGGER_VERSION), new Info { Title = SWAGGER_NAME, Version = string.Format("v{0}", SWAGGER_VERSION) });
                });
            }

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(CORS_POLICY));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", string.Format("{0} v{1}", SWAGGER_NAME, SWAGGER_VERSION));
                });
            }
            else
            {
                app.UseExceptionHandler(a => a.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = feature.Error;
                    var result = JsonConvert.SerializeObject(new { error = exception.Message });

                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(result);
                }));
            }

            app.UseCors(CORS_POLICY);
            app.UseMvc();
        }
    }
}
