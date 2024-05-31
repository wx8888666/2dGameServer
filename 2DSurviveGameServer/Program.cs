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

            builder.Logging.ClearProviders();//����Ĭ����־���
            builder.Services.AddDirectoryBrowser();//
            builder.Services.AddControllers();

            var app = builder.Build();

            app.Urls.Add("http://*:5000");
            //wwwroot�ڵ�һ�α����ʱ�򣬻���bin���Զ�����һ����ͬ��Ŀ¼����Ŀ�еĵ�ַҲ�Ǳ���ú�wwwroot�ĵ�ַ
            //������Դ��ַ���ں���ı�����һ�㲻���ʼ�����wwwroot
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
            //�첽ִ�з�ֹ while(true)ִ�в���
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
