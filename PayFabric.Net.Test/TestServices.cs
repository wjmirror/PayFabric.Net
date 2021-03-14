using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;
using Serilog.Extensions.Logging;
using System.Net.Http;

namespace PayFabric.Net.Test
{
    public static class TestServices
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfigurationRoot Configuration { get; private set; }

        public static ILoggerFactory LoggerFactory { get; private set; }

        private static bool isInitialized = false;
        public static void InitializeService()
        {
            if (!isInitialized)
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                Configuration = configurationBuilder.Build();


                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(Configuration);
                ConfigureServices(services);
                ServiceProvider = services.BuildServiceProvider();
                LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();

                isInitialized = true;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration, "Serilog").CreateLogger();


            services.AddLogging(config => config.AddConfiguration(Configuration.GetSection("Logging"))
                                            .AddConsole().AddDebug().AddSerilog());
            
            services.AddOptions();
            services.Configure<PayFabricOptions>(options => Configuration.GetSection(nameof(PayFabricOptions)).Bind(options));
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(sp => sp.GetService<ILoggerFactory>().CreateLogger("PayFabric.Net"));
            services.AddSingleton<IPaymentService, PayFabricPaymentService>();
            services.AddSingleton<IWalletService, PayFabricWalletService>();
            services.AddSingleton<ITransactionService, PayFabricPaymentService>();
        }

        public static ILogger<T> GetLogger<T>()
        {
            if (!isInitialized)
                InitializeService();
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
