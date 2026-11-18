using System.Text.Json;

try
{
    if (args.Length != 1) throw new ArgumentException("Pass one JSON input file");
    using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
    Console.WriteLine(JsonSerializer.Serialize(Analyze(data.RootElement)));
    return 0;
}
catch (Exception error) when (error is IOException or JsonException or FormatException
                               or ArgumentException or InvalidOperationException
                               or KeyNotFoundException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

static object Analyze(JsonElement input)
{
    double Positive(string key)
    {
        double value = input.GetProperty(key).GetDouble();
        if (!double.IsFinite(value) || value <= 0) throw new ArgumentException($"{key} must be positive and finite");
        return value;
    }
    double original = Positive("shutterSeconds");
    double aperture = Positive("aperture"), nextAperture = Positive("newAperture");
    double iso = Positive("iso"), nextIso = Positive("newIso");
    double filterStops = input.GetProperty("filterStops").GetDouble();
    double stops = 2 * (Math.Log2(nextAperture) - Math.Log2(aperture)) + Math.Log2(iso) - Math.Log2(nextIso) + filterStops;
    double duration = original * Math.Pow(2, stops);
    if (!double.IsFinite(duration) || duration <= 0) throw new ArgumentException("The exposure change is out of range");
    return new { newShutterSeconds = Math.Round(duration, 12), shutterChangeStops = Math.Round(stops, 6) };
}
