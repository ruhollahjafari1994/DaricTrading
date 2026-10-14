using DaricTrading.Application.Common;

namespace DaricTrading.Infrastructure.Persistence.InMemory;

/// <summary>واحد کار محیط حافظه‌ای است که تغییرات store مشترک در آن بلافاصله اعمال می‌شوند.</summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    /// <summary>Task کامل‌شده برمی‌گرداند چون repository حافظه‌ای تغییرات را هم‌زمان با فراخوانی اعمال کرده است.</summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
