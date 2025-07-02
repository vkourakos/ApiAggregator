using System.Text.Json.Serialization;

namespace ApiAggregator.Infrastructure.Responses;

public class WeatherApiResponse
{
    [JsonPropertyName("location")]
    public WeatherLocation Location { get; set; } = new();

    [JsonPropertyName("current")]
    public WeatherCurrent Current { get; set; } = new();
}

public class WeatherLocation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
    [JsonPropertyName("localtime_epoch")]
    public long LocaltimeEpoch { get; set; }
}

public class WeatherCurrent
{
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }
    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }
    [JsonPropertyName("wind_dir")]
    public string WindDir { get; set; } = string.Empty;
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
    [JsonPropertyName("feelslike_c")]
    public double FeelslikeC { get; set; }
    [JsonPropertyName("condition")]
    public WeatherCondition Condition { get; set; } = new();
}

public class WeatherCondition
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
