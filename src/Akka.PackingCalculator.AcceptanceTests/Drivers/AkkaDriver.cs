using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.PackingCalculator.AcceptanceTests.Containers;
using Akka.Remote.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Akka.PackingCalculator.AcceptanceTests.Drivers;

public class AkkaDriver : Hosting.TestKit.TestKit
{
    protected const int Port = 4200;
    protected const string Hostname = "127.0.0.1";
    private readonly string _actorSystemName;

    public AkkaDriver(string actorSystemName = "Re") : base(actorSystemName,
        startupTimeout: TimeSpan.FromSeconds(15))
    {
        Serilog.Log.Information("[TEST][{AkkaDriverName}][{ActorSystemName}] ctor", GetType().Name, actorSystemName);
        _actorSystemName = actorSystemName;
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        Serilog.Log.Information("[TEST][{AkkaDriverName}][{ActorSystemName}] ConfigureAkka", GetType().Name,
            _actorSystemName);

        builder
            .WithRemoting("0.0.0.0", Port, Hostname)
            .WithClustering(new ClusterOptions
            {
                Roles = new[] { "OMEGALUL" },
                SeedNodes = new[]
                {
                    $"akka.tcp://{_actorSystemName}@{LighthouseNodeContainer.Hostname}:{LighthouseNodeContainer.Port}"
                }
            });

        ConfigureAkkaServices(builder);
    }

    protected static void ConfigureAkkaServices(AkkaConfigurationBuilder builder)
    {
    }

    protected override void ConfigureServices(HostBuilderContext _, IServiceCollection services)
    {
    }

    public T Expect<T>(int seconds) => ExpectMsg<T>(TimeSpan.FromSeconds(seconds));
}