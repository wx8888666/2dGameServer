using _2DSurviveGameServer._01Common;
using Microsoft.Extensions.FileProviders;
using Quartz.Impl;
using Quartz;
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
            //异步执行防止 while(true)执行不了,不使用Run就是为了防止阻塞主逻辑；
            app.RunAsync();

            ServerRoot.Instance.Init();
            StartQuartzScheduler().Wait();

            while (true)
            {
                ServerRoot.Instance.Update();

                Thread.Sleep(1);
            }
        }
        static async Task StartQuartzScheduler()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<ResetDailyStatusJob>()
                .WithIdentity("resetDailyStatusJob", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("dailyTrigger", "group1")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
