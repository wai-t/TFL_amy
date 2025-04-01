using tfl_stats.Server.Configurations;
using tfl_stats.Server.Services.JourneyService;
using tfl_stats.Server.Services.LineService;

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

            //builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddUserSecrets<Program>();

            builder.Services.Configure<AppSettings>(
                builder.Configuration.GetSection("AppSettings"));


            builder.Services.AddHttpClient<LineService, LineService>();
            builder.Services.AddHttpClient<JourneyService>();




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
