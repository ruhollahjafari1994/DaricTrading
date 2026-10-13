namespace DaricTrading.Domain;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
