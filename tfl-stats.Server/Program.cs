using tfl_stats.Core.Client.Generated;
using tfl_stats.Server.Middleware;
using tfl_stats.Server.Services;
using tfl_stats.Server.Services.Cache;

namespace tfl_stats.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.AddConsole();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost",
                    builder => builder.WithOrigins("https://localhost:55811")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

            var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("ApiSettings:BaseUrl is not configured in appsettings.json.");
            }

            /*builder.Services.AddHttpClient<ApiClient>(options =>
            {
                options.BaseAddress = new Uri(baseUrl);
                options.Timeout = TimeSpan.FromSeconds(10);
            });*/

            builder.Services.AddHttpClient<LineClient>();
            builder.Services.AddHttpClient<JourneyClient>();
            builder.Services.AddHttpClient<StopPointClient>();

            builder.Services.AddScoped<LineService>();
            builder.Services.AddScoped<StopPointService>();
            builder.Services.AddScoped<JourneyService>();
            builder.Services.AddSingleton<LineDiagramService>();
            // Not sure if it should be Sigleton
            builder.Services.AddSingleton<ArrivalService>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowLocalhost");

            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            // use the Recorder class as a middleware layer
            app.UseMiddleware<Recorder>();

            app.Run();

        }
    }
}
