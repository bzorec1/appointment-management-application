using System.Globalization;

namespace HairSalonAppointments.Demo.Api;

internal static class CsvLog
{
    public static void Append(string file, string provider, string op, long ms, int status, bool success,
        string? notes = null)
    {
        Directory.CreateDirectory("./docs/results");
        var line = string.Join(",",
            DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture),
            provider,
            op,
            ms.ToString(CultureInfo.InvariantCulture),
            status.ToString(CultureInfo.InvariantCulture),
            success ? "true" : "false",
            notes ?? ""
        );
        File.AppendAllText($"./docs/results/{file}.csv", line + Environment.NewLine);
    }
}