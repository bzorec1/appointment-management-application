using System.Diagnostics;
using System.Net.Http.Json;
using HairSalonAppointments.Demo.Graph;

var baseUrl = Environment.GetEnvironmentVariable("API_BASE") ?? "http://localhost:8080";
var iterations = int.TryParse(Environment.GetEnvironmentVariable("ITERATIONS"), out var i) ? i : 10;

var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
Console.WriteLine($"[Graph Demo] Base: {http.BaseAddress}, Iterations: {iterations}");

var sw = Stopwatch.StartNew();

for (int run = 1; run <= iterations; run++)
{
    Console.WriteLine($"\n--- Run {run}/{iterations} ---");
    var now = DateTimeOffset.Now;

    var body = new
    {
        providerId = (string?)null,
        title = $"Graph Demo Event {run}",
        start = now.AddMinutes(25 + run),
        end = now.AddMinutes(55 + run),
        timeZone = "Europe/Ljubljana",
        notes = (string?)null
    };

    // Create
    sw.Restart();
    var create = await http.PostAsJsonAsync("/api/v1/providers/graph/events?api-version=1.0", body);
    sw.Stop();
    CsvLog.Append("graph", "graph", "create", sw.ElapsedMilliseconds, (int)create.StatusCode,
        create.IsSuccessStatusCode, $"run_{run}");
    Console.WriteLine($"Create: {create.StatusCode} ({sw.ElapsedMilliseconds}ms)");

    // List
    var listUrl =
        $"/api/v1/providers/graph/events?api-version=1.0&from={Uri.EscapeDataString(now.AddHours(-1).ToString("o"))}&to={Uri.EscapeDataString(now.AddHours(6).ToString("o"))}";
    sw.Restart();
    var list = await http.GetAsync(listUrl);
    sw.Stop();
    CsvLog.Append("graph", "graph", "list", sw.ElapsedMilliseconds, (int)list.StatusCode,
        list.IsSuccessStatusCode, $"run_{run}");
    Console.WriteLine($"List: {list.StatusCode} ({sw.ElapsedMilliseconds}ms)");
}

Console.WriteLine($"\n[Graph Demo] Completed {iterations} iterations. Results in ./docs/results/graph.csv");
