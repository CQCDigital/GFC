using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SYE.MiddlewareExtensions;
using System;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;

namespace SYE
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
                options.Filters.Add(new XssReferrerFilter(Configuration));
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.Configure<CookieTempDataProviderOptions>(options =>
                options.Cookie.Name = "GFC-Temp-Data-Cookie"
            );

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "GFC-Anti-Forgery-Cookie"; // AntiForgery Cookie
                options.HeaderName = "GFC-Anti-Forgery-Header";
                options.Cookie.Path = "/";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "GFC-Session-Cookie";
                options.IdleTimeout = TimeSpan.FromMinutes(70);
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddMvc();

            services.AddHttpContextAccessor();
            services.AddOptions();
            services.AddCustomServices(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Xss-Protection", "1");
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                await next();
            });

            //Note : https://z416426.vo.msecnd.net is used by PowerBi / Application Insights
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                //.ScriptSources(s => s.Self().CustomSources("https://az416426.vo.msecnd.net", "https://www.googletagmanager.com", 
                //                                           "https://tagmanager.google.com", "https://www.google-analytics.com",
                //                                           "https://optimize.google.com", "https://ssl.google-analytics.com").UnsafeInline())

                .StyleSources(s => s.Self().CustomSources("https://tagmanager.google.com", "https://fonts.googleapis.com", 
                                                          "https://optimize.google.com").UnsafeInline())

                .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com", "https://fonts.googleapis.com", "data:"))

                .ImageSources(s => s.Self().CustomSources("https://www.googletagmanager.com", "https://www.google-analytics.com", "https://optimize.google.com", 
                                                          "https://ssl.gstatic.com", "https://www.gstatic.com", "data:"))

                .FrameSources(s => s.Self().CustomSources("https://optimize.google.com"))
                .FrameAncestors(s => s.Self())
            );

            app.UseHsts(h => h.MaxAge(365).IncludeSubdomains().Preload());
            app.UseReferrerPolicy(opts => opts.SameOrigin());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.SameOrigin());
            app.UseXContentTypeOptions();

            if (env.IsDevelopment() || env.IsEnvironment("Local"))
            {
                app.UseExceptionHandler("/Error/500");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions{ HttpOnly = HttpOnlyPolicy.Always, Secure = CookieSecurePolicy.Always });

            app.UseSession();

            app.UseRewriter(new RewriteOptions().AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 63423));
            
            var provider = new FileExtensionContentTypeProvider { Mappings = { [".webmanifest"] = "application/manifest+json" } };
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });


            app.UseRouting();

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
