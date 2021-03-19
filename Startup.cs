using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InsuranceService.Extensions;
using Serilog.Context;
using Microsoft.EntityFrameworkCore;
using InsuranceService.Infrastructure.Context;
using InsuranceService.ExceptionManagement;

namespace InsuranceService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // DB Connection
            services.AddDbContext<ChannelRefDBContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddControllers();
            // Swagger 
            services.AddApiDoc();
            // DI mapper
            services.RegisterAppServices();

        }
    

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //swagger
            app.UseApiDoc();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
               
            }      

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            // global error handler
            app.UseCustomMiddleware();    
            
            //Get user Name for DB logging
            app.Use(async (httpContext, next) =>
            {
                var username = httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "anonymous";
                LogContext.PushProperty("UserName", username);
                await next.Invoke();
            });

        }
    }
}
