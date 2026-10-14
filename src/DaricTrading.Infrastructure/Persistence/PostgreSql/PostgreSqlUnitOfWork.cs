using DaricTrading.Application.Common;
using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>ذخیره EF Core را متمرکز می‌کند و خطاهای unique/concurrency را به conflict دامنه تبدیل می‌کند.</summary>
public sealed class PostgreSqlUnitOfWork(TradingDbContext dbContext) : IUnitOfWork
{
    /// <summary>تغییرات tracked را commit کرده و DbUpdateConcurrencyException یا کلید تکراری wallet را به conflict تبدیل می‌کند.</summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException(
                "The record changed during this operation. Reload it and retry.");
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "PK_gold_wallets"
            })
        {
            throw new ConcurrencyConflictException(
                "The wallet was created by another request. Reload it and retry.");
        }
    }
}
