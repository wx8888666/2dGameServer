using _2DSurviveGameServer._01Common;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;

namespace _2DSurviveGameServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Logging.ClearProviders();//禁用默认日志输出
            builder.Services.AddDirectoryBrowser();//
            builder.Services.AddControllers();

            var app = builder.Build();

            app.Urls.Add("http://*:5000");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory + "wwwroot")
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory + "wwwroot")
            });//
            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            app.RunAsync();

            ServerRoot.Instance.Init();


            while(true)
            {
                ServerRoot.Instance.Update();

                Thread.Sleep(1);
            }
        }
    }
}
