namespace DevHands.Playground.Application;

internal sealed record Payload
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("total_time_ms")]
    public required long TotalTimeMs { get; init; }
}
