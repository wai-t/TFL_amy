using StackExchange.Redis;
using tfl_stats.Server.Client;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Services;

namespace tfl_stats.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
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

            builder.Configuration.AddUserSecrets<Program>();

            builder.Services.Configure<AppSettings>(
                builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var redisConnection = configuration.GetConnectionString("Redis");

                var options = ConfigurationOptions.Parse(redisConnection);
                options.ConnectTimeout = 10000;
                options.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(options);
            });


            builder.Services.AddHttpClient<ApiClient>();

            builder.Services.AddScoped<LineService>();
            builder.Services.AddScoped<StopPointService>();
            builder.Services.AddScoped<JourneyService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var stopPointService = scope.ServiceProvider.GetRequiredService<StopPointService>();
                await stopPointService.PreloadStopPoints();
            }


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
