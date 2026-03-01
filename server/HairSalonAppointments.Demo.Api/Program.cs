using System.Diagnostics;
using System.Net.Http.Json;
using HairSalonAppointments.Demo.Api;

var baseUrl = Environment.GetEnvironmentVariable("API_BASE")
              ?? "http://localhost:8080";

var iterations = int.TryParse(Environment.GetEnvironmentVariable("ITERATIONS"), out var i)
    ? i
    : 10;

var http = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

Console.WriteLine($"[API Demo] Base: {http.BaseAddress}, Iterations: {iterations}");

var sw = Stopwatch.StartNew();

for (var run = 1; run <= iterations; run++)
{
    Console.WriteLine($"\n--- Run {run}/{iterations} ---");

    var today = DateTimeOffset.Now;
    var from = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, today.Offset);
    var to = from.AddDays(1);

    var createBody = new
    {
        title = $"Striženje – Demo {run}",
        start = from.AddHours(10).AddMinutes(run * 5),
        end = from.AddHours(10.5).AddMinutes(run * 5),
        resourceId = 1,
        phone = "+38640123456",
        service = "Haircut",
        customerName = $"Ana {run}"
    };

    sw.Restart();
    var resp = await http.PostAsJsonAsync("/api/v1/appointments?api-version=1.0", createBody);
    sw.Stop();

    var ok = resp.IsSuccessStatusCode;
    CsvLog.Append(
        "api",
        "api",
        "create_appointment",
        sw.ElapsedMilliseconds,
        (int)resp.StatusCode,
        ok,
        $"run_{run}");
    Console.WriteLine($"Create: {resp.StatusCode} ({sw.ElapsedMilliseconds}ms)");

    var listUrl =
        $"/api/v1/appointments?api-version=1.0&from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
    sw.Restart();
    var listResp = await http.GetAsync(listUrl);
    sw.Stop();

    CsvLog.Append(
        "api",
        "api",
        "list_appointments",
        sw.ElapsedMilliseconds,
        (int)listResp.StatusCode,
        listResp.IsSuccessStatusCode,
        $"run_{run}");
    Console.WriteLine($"List: {listResp.StatusCode} ({sw.ElapsedMilliseconds}ms)");

    const int morning = 0;
    const int client = 0;
    var req = new
    {
        targetDate = from.DateTime,
        timePreference = morning,
        requestedBy = client,
        serviceId = "haircut"
    };

    sw.Restart();
    var sugResp = await http.PostAsJsonAsync("/api/v1/suggestions?api-version=1.0", req);
    sw.Stop();

    CsvLog.Append(
        "api",
        "api",
        "suggestions",
        sw.ElapsedMilliseconds,
        (int)sugResp.StatusCode,
        sugResp.IsSuccessStatusCode,
        $"run_{run}");
    Console.WriteLine($"Suggestions: {sugResp.StatusCode} ({sw.ElapsedMilliseconds}ms)");
}

Console.WriteLine($"\n[API Demo] Completed {iterations} iterations. Results in ./docs/results/api.csv");