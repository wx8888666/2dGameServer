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
            //�첽ִ�з�ֹ while(true)ִ�в���,��ʹ��Run����Ϊ�˷�ֹ�������߼���
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
