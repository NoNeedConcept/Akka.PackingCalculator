using Akka.PackingCalculator.AcceptanceTests.Containers;
using Akka.PackingCalculator.AcceptanceTests.Drivers;
using Akka.PackingCalculator.Core;
using Serilog;
using TechTalk.SpecFlow;

namespace Akka.PackingCalculator.AcceptanceTests.Hooks;

[Binding]
public class EnvironmentSetupHooks
{
    public static MongoDbContainer MongoDbContainer = null!;
    public static PostgreContainer PostgreContainer = null!;
    public static LighthouseNodeContainer SeedNodeContainer = null!;
    public static PackingCalculatorApplicationFactory ApplicationFactory = null!;
    public static AkkaDriver AkkaDriver = null!;

    [BeforeFeature]
    public static async Task BeforeFeature()
    {
        Log.Logger = CreateLogger();
        Log.Information("[TEST][EnvironmentSetupHooks][BeforeFeature]");
        MongoDbContainer = new();
        PostgreContainer = new();
        AkkaDriver = new();
        SeedNodeContainer = new();

        var lighthouseTask = SeedNodeContainer.InitializeAsync();
        var mongoTask = MongoDbContainer.InitializeAsync();
        var postgreTask = PostgreContainer.InitializeAsync();

        await lighthouseTask;
        await mongoTask;
        await postgreTask;

        var mongoDBString = MongoDbContainer.GetConnectionString();
        var postgreSQLString = PostgreContainer.GetConnectionString();

        Log.Debug("[TEST][EnvironmentSetupHooks] - MongoDb: {ConnectionString}", mongoDBString);
        Log.Debug("[TEST][EnvironmentSetupHooks] - Postgre: {ConnectionString}", postgreSQLString);
        AkkaPackingCalculator.SetEnvironmentVariable("mongodb", mongoDBString);
        AkkaPackingCalculator.SetEnvironmentVariable("postgre", postgreSQLString);

        AkkaDriver = new();
        await AkkaDriver.InitializeAsync();

        ApplicationFactory = new();
        await ApplicationFactory.InitializeAsync();

        await Task.Delay(5000);
    }

    [AfterScenario]
    public static void AfterScenario() => Log.Information("[TEST][EnvironmentSetupHooks][AfterScenario]");

    [AfterFeature]
    public static async Task AfterFeature()
    {
        Log.Information("[TEST][EnvironmentSetupHooks][AfterFeature]");

        await ApplicationFactory.DisposeAsync();
        await AkkaDriver.DisposeAsync();
        await MongoDbContainer.DisposeAsync();
        await PostgreContainer.DisposeAsync();
        await SeedNodeContainer.DisposeAsync();
        await Task.Delay(2500);
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        Log.Information("[TEST][EnvironmentSetupHooks][AfterFeature]");
    }

    private static ILogger CreateLogger()
        => new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();
}
