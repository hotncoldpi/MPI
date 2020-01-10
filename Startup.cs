using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MasterPatientIndex.Repo;

namespace MasterPatientIndex
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<PatientContext>(op => op.UseSqlServer(
                Configuration.GetConnectionString("test")
            ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, 
            Microsoft.Extensions.Hosting.IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }

            //global error handler
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;

                var result = System.Text.Json.JsonSerializer.Serialize(new { error = "unhandled exception" });
                if (env.IsDevelopment())
                    result = System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message });
                context.Response.ContentType = "application/json";

                context.Response.StatusCode = 500;
                await context.Response.Body.WriteAsync(System.Text.Encoding.ASCII.GetBytes(result));
            }));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (Configuration["init"] != "true")
                return;

            //run via --init, so we init db and exit
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                    var context = serviceScope.ServiceProvider.GetRequiredService<PatientContext>();
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
            }
            lifetime.StopApplication();            
        }
    }
}
