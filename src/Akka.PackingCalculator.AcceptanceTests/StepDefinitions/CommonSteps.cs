using Akka.PackingCalculator.AcceptanceTests.Drivers;
using Akka.PackingCalculator.AcceptanceTests.Hooks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TechTalk.SpecFlow;

namespace Akka.PackingCalculator.AcceptanceTests.StepDefinitions;

[Binding]
public class CommonSteps
{
    private readonly ScenarioContext _context;
    private readonly AkkaDriver _akkaDriver;
    private readonly ILogger _logger = Log.Logger.ForContext<CommonSteps>();

    public CommonSteps(ScenarioContext context)
    {
        _logger.Information("[TEST][CommonStepDefinitions][ctor]", GetType().Name);
        _context = context;
        _akkaDriver = EnvironmentSetupHooks.AkkaDriver;
    }
}