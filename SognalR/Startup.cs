using Microsoft.AspNetCore.Builder;
using SognalR.HubConfig;

namespace SognalR
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => { 
                options.AddPolicy("AllowAllHeaders", builder => 
                {
                  builder.AllowAnyOrigin()
                         .AllowAnyHeader()
                         .AllowAnyMethod();
                });
            
            });

            services.AddSignalR(option =>
            {
                option.EnableDetailedErrors = true;
            });

            services.AddControllers();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          if (env.IsDevelopment())
          {
              app.UseDeveloperExceptionPage();
          }

          app.UseRouting();
          app.UseCors("AllowAllHeaders"); // Apply the CORS policy

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // for API controllers
                endpoints.MapHub<ChatHub>("/chatHub"); // Map the ChatHub to the /chatHub endpoint
            });

        }
    }
}
