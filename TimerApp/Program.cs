using Microsoft.EntityFrameworkCore;
using Stl.Fusion;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;
using TimerApp.Services;

namespace TimerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register TimerContext
            builder.Services.AddDbContext<TimerContext>(options =>
                   options.UseSqlServer(builder.Configuration.GetConnectionString("TimerConnection")));

            builder.Services.AddScoped<TimerDataService>();
            var fusion = builder.Services.AddFusion();
            fusion.AddFusionTime();
            fusion.AddService<TimerService>();
            //builder.Services.AddScoped<TimerService>();

            builder.Services.AddServerSideBlazor();

            // Add Razor Pages services
            builder.Services.AddRazorPages();

           // builder.Services.AddHostedService(c => c.GetRequiredService<TimerService>());

            // Default update delay is set to 0.1s
            builder.Services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.1));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            // Add Razor Pages endpoints
            app.MapRazorPages();

            app.Run();
        }
    }
}
