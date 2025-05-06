using tfl_stats.Server.Client;
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

            builder.Services.AddHttpClient<ApiClient>(options =>
            {
                options.BaseAddress = new Uri(baseUrl);
                options.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddScoped<LineService>();
            builder.Services.AddScoped<StopPointService>();
            builder.Services.AddScoped<JourneyService>();

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

            app.Run();

        }
    }
}
