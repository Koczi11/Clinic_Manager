using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace ClinicManager.PerformanceTests;

public static class VisitsLoadTest
{
    private const string ApiUrl = "http://localhost:5080/api/visits/active";

    public static void Run()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("visits_active_load_test", async context =>
        {
            var request = Http.CreateRequest("GET", ApiUrl)
                .WithHeader("Accept", "application/json");

            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.IterationsForConstant(copies: 50, iterations: 100)
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("nbomber-reports")
            .Run();
    }
}
