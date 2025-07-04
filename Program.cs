using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OmniFileCSharp.Configuration;
using OmniFileCSharp.Services;
using Serilog;

namespace OmniFileCSharp;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting OmniFile C# application");

            // Build service collection
            var services = new ServiceCollection();

            // Add configuration
            var appConfig = AppConfig.Load(configuration);
            services.AddSingleton(appConfig);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });

            // Add MongoDB
            var mongoClient = new MongoClient(appConfig.MongoUri);
            var database = mongoClient.GetDatabase(appConfig.DbName);
            services.AddSingleton<IMongoDatabase>(database);

            // Add services
            services.AddScoped<FileScannerService>();

            var serviceProvider = services.BuildServiceProvider();

            // Get services
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var fileScannerService = serviceProvider.GetRequiredService<FileScannerService>();

            // Database self-check
            var dbOk = await fileScannerService.DbSelfCheckAsync();
            if (!dbOk)
            {
                logger.LogError("Canceling execution due to database self-check failure");
                return;
            }

            // Interactive prompt
            Console.Write("What do you want to do? (write 'ejecutar' to scan files, 'consultar' to only view database status, or 'remoto' to only scan remote connections): ");
            var answer = Console.ReadLine()?.Trim().ToLower();

            if (answer == "consultar")
            {
                logger.LogInformation("Query finished. File scanning will not be executed.");
                return;
            }
            else if (answer != "ejecutar" && answer != "remoto")
            {
                logger.LogWarning("Unrecognized option. Canceling execution.");
                return;
            }

            // Ensure path index
            await fileScannerService.EnsurePathIndexAsync();

            // Get last record
            var last = await fileScannerService.GetLastRecordAsync();
            if (last != null)
            {
                logger.LogInformation("Last file or directory read:");
                logger.LogInformation("{@LastRecord}", last);
            }
            else
            {
                logger.LogInformation("No previous records in database.");
            }

            // Continue or truncate prompt
            Console.Write("Do you want to continue from the last file/directory read (write 'continuar') or truncate the database and start from scratch (write 'truncar')?: ");
            var continueOrTruncate = Console.ReadLine()?.Trim().ToLower();

            if (continueOrTruncate == "truncar")
            {
                await fileScannerService.TruncateCollectionAsync();
            }
            else if (continueOrTruncate != "continuar")
            {
                logger.LogWarning("Unrecognized option. Canceling execution.");
                return;
            }

            // Execute scanning based on user choice
            if (answer == "ejecutar")
            {
                logger.LogInformation("Scanning local files...");
                await fileScannerService.ScanDirectoryAsync(appConfig.RootDir);

                if (appConfig.EnableRemoteScanning)
                {
                    logger.LogInformation("Scanning remote connections...");
                    await fileScannerService.ScanRemoteConnectionsAsync();
                }
            }
            else if (answer == "remoto")
            {
                logger.LogInformation("Scanning only remote connections...");
                await fileScannerService.ScanRemoteConnectionsAsync();
            }

            // Show protocol statistics
            await fileScannerService.ShowProtocolStatsAsync();
            logger.LogInformation("Scan and save completed.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
} 