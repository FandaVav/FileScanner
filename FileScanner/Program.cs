
using PracticeTask.Services;
using Serilog;

namespace PracticeTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();


            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IDirectoryScanner, DirectoryScanner>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
