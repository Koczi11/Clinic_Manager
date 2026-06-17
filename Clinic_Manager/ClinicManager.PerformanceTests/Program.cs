using ClinicManager.PerformanceTests;

Console.WriteLine("=== NBomber: test wydajnosci GET /api/visits/active ===");
Console.WriteLine("Upewnij sie, ze API dziala pod http://localhost:5080");
Console.WriteLine();

VisitsLoadTest.Run();
