using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace InsuranceService
{
    public class Program
    {
        //--- DB logger
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
           .Build();
           
        public static void Main(string[] args)
        {

            //--- DB logger: read connection string from app settings
            string connectionString = Configuration.GetConnectionString("SqlConnLogging");
            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn("UserName",SqlDbType.VarChar)
                }
            };

            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.MSSqlServer(connectionString,
            sinkOptions: new SinkOptions { TableName = "Logs" }
            , null, null, LogEventLevel.Information, null, columnOptions: columnOptions, null, null)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .CreateLogger();                         
         
           
                CreateHostBuilder(args).Build().Run();
         
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    
                    logging.ClearProviders();
                    logging.AddConsole(options => options.IncludeScopes = true);
                    logging.AddDebug();
                    logging.AddEventLog(options => options.SourceName="Insurance Domain");
                    logging.AddEventSourceLogger();
                    logging.AddNLog();
                    logging.AddSerilog();
                   
                })
                
                .ConfigureWebHostDefaults(webBuilder =>
                {                        
                    webBuilder.UseStartup<Startup>();
                });
            
    }
}
