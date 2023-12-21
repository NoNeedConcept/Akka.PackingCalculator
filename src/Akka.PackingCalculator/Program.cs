using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Akka.HealthCheck.Hosting.Web;
using Akka.Persistence.Sql.Hosting;
using Akka.Persistence.Sql.Config;
using Akka.HealthCheck.Hosting;
using Akka.Persistence.Hosting;
using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;
using Akka.Logger.Serilog;
using MongoDB.Driver;
using Akka.PackingCalculator;
using Akka.Hosting;
using Serilog;
using Akka.PackingCalculator.Core;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Log.Logger);

RegisterMongoShit();

builder.Services.AddHealthChecks();

builder.Services.WithAkkaHealthCheck(HealthCheckType.All)
.AddSingleton<IMongoClient>(x => new MongoClient(AkkaPackingCalculator.GetEnvironmentVariable("mongodb")))
.AddScoped(x => x.GetRequiredService<IMongoClient>().GetDatabase("PackingCalculator"))
.AddAkka("Re", (builder, sp) =>
    {
        var connectionString = AkkaPackingCalculator.GetEnvironmentVariable("postgre");
        var shardingJournalOptions = new Akka.Persistence.Sql.Hosting.SqlJournalOptions(true)
        {
            ConnectionString = connectionString,
            ProviderName = LinqToDB.ProviderName.PostgreSQL15,
            TagStorageMode = TagMode.TagTable,
            AutoInitialize = true
        };

        var shardingSnapshotOptions = new Akka.Persistence.Sql.Hosting.SqlSnapshotOptions(true)
        {
            ConnectionString = connectionString,
            ProviderName = LinqToDB.ProviderName.PostgreSQL15,
            AutoInitialize = true
        };

        builder.ConfigureLoggers(setup =>
            {
                // Clear all loggers
                setup.ClearLoggers();

                // Add serilog logger
                setup.AddLogger<SerilogLogger>();
                setup.LogMessageFormatter = typeof(SerilogLogMessageFormatter);
            })
            .AddHocon(hocon: "akka.remote.dot-netty.tcp.maximum-frame-size = 256000b", addMode: HoconAddMode.Prepend)
            .WithHealthCheck(x => x.AddProviders(HealthCheckType.All))
            .WithWebHealthCheck(sp)
            .WithRemoting("0.0.0.0", 1337, "127.0.0.1")
            .WithClustering(new ClusterOptions
            {
                Roles = new[] { "LULW" },
                SeedNodes = new[] { "akka.tcp://Re@127.0.0.1:42000" }
            })
            .WithSqlPersistence(connectionString!, LinqToDB.ProviderName.PostgreSQL15, PersistenceMode.Both, autoInitialize: true, tagStorageMode: TagMode.Both)
            .WithJournalAndSnapshot(shardingJournalOptions, shardingSnapshotOptions);
            // .WithShardRegion<PointWorker>("PointWorker", (_, _, dependecyResolver) => x => dependecyResolver.Props<PointWorker>(x), new MessageExtractor(3000), new ShardOptions()
            // {
            //     JournalOptions = shardingJournalOptions,
            //     SnapshotOptions = shardingSnapshotOptions,
            //     Role = "KEKW",
            //     ShouldPassivateIdleEntities = true,
            //     PassivateIdleEntityAfter = TimeSpan.FromMinutes(1)
            // })
            // .WithShardRegionProxy<PointWorkerProxy>("PointWorker", "KEKW", new MessageExtractor(3000))
            // .WithShardRegion<PathfinderWorker>("PathfinderWorker", (_, _, dependecyResolver) => x => dependecyResolver.Props<PathfinderWorker>(x), new MessageExtractor(), new ShardOptions()
            // {
            //     JournalOptions = shardingJournalOptions,
            //     SnapshotOptions = shardingSnapshotOptions,
            //     Role = "KEKW",
            //     ShouldPassivateIdleEntities = true,
            //     PassivateIdleEntityAfter = TimeSpan.FromMinutes(1)
            // })
            // .WithShardRegionProxy<PathfinderProxy>("PathfinderWorker", "KEKW", new MessageExtractor())
            // .WithSingleton<MapManager>("MapManager", (_, _, dependecyResolver) => dependecyResolver.Props<MapManager>(), new ClusterSingletonOptions() { Role = "KEKW" }, false)
            // .WithSingletonProxy<MapManagerProxy>("MapManager", new ClusterSingletonOptions() { Role = "KEKW" })
            // .WithSingleton<SenderManager>("SenderManager", (_, _, dependecyResolver) => dependecyResolver.Props<SenderManager>(), new ClusterSingletonOptions() { Role = "KEKW" }, false)
            // .WithSingletonProxy<SenderManagerProxy>("SenderManager", new ClusterSingletonOptions() { Role = "KEKW" });
    });


var host = builder.Build();
await CreateIndexes(host.Services);
host.UseHealthChecks("/health/ready", new HealthCheckOptions() { AllowCachingResponses = false, Predicate = _ => true });
await host.RunAsync().ConfigureAwait(false);

public partial class Program
{
    private static int _registered;
    protected Program()
    {
    }

    private static void RegisterMongoShit()
    {
        if (Interlocked.Increment(ref _registered) == 1)
            BsonShit.Register();
    }

    private static async Task CreateIndexes(IServiceProvider provider)
    {
        //todo:
        await Task.CompletedTask;
    }
}
