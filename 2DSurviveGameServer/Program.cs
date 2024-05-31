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
            //wwwroot在第一次编译的时候，会在bin中自动生成一个相同的目录，项目中的地址也是编译好后wwwroot的地址
            //而不是源地址，在后面的编译中一般不会初始化这个wwwroot
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
            //异步执行防止 while(true)执行不了
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
