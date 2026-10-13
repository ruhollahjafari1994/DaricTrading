namespace DaricTrading.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public abstract record DomainEvent(DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record GoldPurchasedDomainEvent(Guid OrderId, Guid UserId, GoldWeight Weight, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
public sealed record GoldSoldDomainEvent(Guid OrderId, Guid UserId, GoldWeight Weight, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
public sealed record OrderCancelledDomainEvent(Guid OrderId, Guid UserId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed record GoldWeight : IComparable<GoldWeight>
{
    public decimal Grams { get; }
    private GoldWeight(decimal grams) => Grams = grams;
    public static GoldWeight FromGrams(decimal grams)
    {
        if (grams <= 0 || decimal.Round(grams, 3) != grams)
            throw new DomainRuleViolationException("Gold weight must be positive and have at most three decimal places.");
        return new GoldWeight(grams);
    }
    internal static GoldWeight Zero => new(0m);
    public GoldWeight Add(GoldWeight other) => FromGrams(Grams + other.Grams);
    public GoldWeight Subtract(GoldWeight other) => FromGrams(Grams - other.Grams);
    public int CompareTo(GoldWeight? other) => other is null ? 1 : Grams.CompareTo(other.Grams);
    public static GoldWeight operator +(GoldWeight left, GoldWeight right) => left.Add(right);
    public static GoldWeight operator -(GoldWeight left, GoldWeight right) => left.Subtract(right);
    public static bool operator >(GoldWeight left, GoldWeight right) => left.Grams > right.Grams;
    public static bool operator <(GoldWeight left, GoldWeight right) => left.Grams < right.Grams;
    public static bool operator >=(GoldWeight left, GoldWeight right) => left.Grams >= right.Grams;
    public static bool operator <=(GoldWeight left, GoldWeight right) => left.Grams <= right.Grams;
}

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency = "IRR")
    {
        if (amount < 0 || decimal.Round(amount, 2) != amount)
            throw new DomainRuleViolationException("Money must be non-negative with at most two decimal places.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3 || currency.Any(character => character is < 'A' or > 'Z'))
            throw new DomainRuleViolationException("Currency must be a three-letter uppercase code.");
        Amount = amount;
        Currency = currency;
    }
    public bool Equals(Money? other) => other is not null && Amount == other.Amount && Currency == other.Currency;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}

public sealed class Tag : IEquatable<Tag>
{
    private static readonly HashSet<string> Allowed = ["پس‌انداز_بلندمدت", "نوسان‌گیری_روزانه", "صندوق_طلای_فیزیکی", "تسویه_شمش"];
    public string Value { get; }
    public Tag(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 64 || !Allowed.Contains(value))
            throw new DomainRuleViolationException("Tag is empty, too long, or not allowed.");
        Value = value;
    }
    public bool Equals(Tag? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Tag other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;
}

public enum OrderType { Buy, Sell }
public enum OrderStatus { Pending, Settled, Cancelled }

public sealed class GoldWallet
{
    private readonly object _gate = new();
    public Guid UserId { get; }
    public GoldWeight Balance { get; private set; }
    public long Version { get; private set; }

    public GoldWallet(Guid userId) : this(userId, GoldWeight.Zero, 0) { }
    private GoldWallet(Guid userId, GoldWeight balance, long version)
    {
        if (userId == Guid.Empty) throw new DomainRuleViolationException("Wallet user id is required.");
        UserId = userId;
        Balance = balance;
        Version = version;
    }
    public static GoldWallet Restore(Guid userId, decimal balanceGrams, long version)
    {
        if (balanceGrams < 0 || decimal.Round(balanceGrams, 3) != balanceGrams || version < 0)
            throw new DomainRuleViolationException("Wallet balance must be non-negative with at most three decimal places.");
        return new GoldWallet(userId, balanceGrams == 0 ? GoldWeight.Zero : GoldWeight.FromGrams(balanceGrams), version);
    }
    public void Credit(GoldWeight amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        lock (_gate) { Balance = GoldWeight.FromGrams(Balance.Grams + amount.Grams); Version++; }
    }
    public void Debit(GoldWeight amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        lock (_gate)
        {
            if (Balance.Grams < amount.Grams) throw new InsufficientGoldBalanceException();
            var remaining = Balance.Grams - amount.Grams;
            Balance = remaining == 0 ? GoldWeight.Zero : GoldWeight.FromGrams(remaining);
            Version++;
        }
    }
}

public sealed class GoldOrder
{
    private readonly List<Tag> _tags;
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public OrderType Type { get; private set; }
    public GoldWeight Weight { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money Fee { get; private set; }
    public Money TotalAmount { get; private set; }
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public string? UserNote { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SettledAt { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private GoldOrder() { _tags = []; Weight = GoldWeight.Zero; UnitPrice = new Money(0); Fee = new Money(0); TotalAmount = new Money(0); }
    private GoldOrder(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags, DateTimeOffset now, OrderStatus status)
    {
        if (userId == Guid.Empty) throw new DomainRuleViolationException("Order user id is required.");
        if (type is not OrderType.Buy and not OrderType.Sell) throw new DomainRuleViolationException("Unsupported order type.");
        ArgumentNullException.ThrowIfNull(weight);
        ArgumentNullException.ThrowIfNull(unitPrice);
        ArgumentNullException.ThrowIfNull(fee);
        if (unitPrice.Currency != fee.Currency) throw new DomainRuleViolationException("Price and fee currencies must match.");
        Id = Guid.NewGuid(); UserId = userId; Type = type; Weight = weight; UnitPrice = unitPrice; Fee = fee;
        var gross = decimal.Round(weight.Grams * unitPrice.Amount, 2, MidpointRounding.ToEven);
        var total = type == OrderType.Buy ? gross + fee.Amount : gross - fee.Amount;
        if (total < 0) throw new DomainRuleViolationException("Sell fee cannot exceed gross proceeds.");
        TotalAmount = new Money(total, unitPrice.Currency);
        _tags = tags?.Distinct().ToList() ?? [];
        Status = status; CreatedAt = now;
        if (status == OrderStatus.Settled) SettledAt = now;
    }
    public static GoldOrder Buy(Guid userId, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags = null, DateTimeOffset? now = null) => CreateSettled(userId, OrderType.Buy, weight, unitPrice, fee, tags, now ?? DateTimeOffset.UtcNow);
    public static GoldOrder Sell(Guid userId, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags = null, DateTimeOffset? now = null) => CreateSettled(userId, OrderType.Sell, weight, unitPrice, fee, tags, now ?? DateTimeOffset.UtcNow);
    public static GoldOrder Pending(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, DateTimeOffset? now = null) => new(userId, type, weight, unitPrice, fee, null, now ?? DateTimeOffset.UtcNow, OrderStatus.Pending);
    private static GoldOrder CreateSettled(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags, DateTimeOffset now)
    {
        var order = new GoldOrder(userId, type, weight, unitPrice, fee, tags, now, OrderStatus.Settled);
        order.RaiseSettlementEvent(now);
        return order;
    }
    public void Settle(DateTimeOffset? settledAt = null)
    {
        if (Status != OrderStatus.Pending) throw new InvalidOrderStateTransitionException("Only a pending order can be settled.");
        Status = OrderStatus.Settled;
        SettledAt = settledAt ?? DateTimeOffset.UtcNow;
        RaiseSettlementEvent(SettledAt.Value);
    }
    public void UpdateTags(IEnumerable<Tag> tags, string? userNote)
    {
        if (Status != OrderStatus.Settled) throw new InvalidOrderStateTransitionException("Only settled orders can update tags or notes.");
        ArgumentNullException.ThrowIfNull(tags);
        var updatedTags = tags.Distinct().ToList();
        _tags.Clear(); _tags.AddRange(updatedTags); UserNote = userNote;
    }
    public void Cancel(DateTimeOffset? cancelledAt = null)
    {
        if (Status != OrderStatus.Pending) throw new InvalidOrderStateTransitionException("Only pending orders can be cancelled.");
        Status = OrderStatus.Cancelled;
        _domainEvents.Add(new OrderCancelledDomainEvent(Id, UserId, cancelledAt ?? DateTimeOffset.UtcNow));
    }
    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
    {
        var events = _domainEvents.ToArray(); _domainEvents.Clear(); return events;
    }
    public static GoldOrder Restore(Guid id, Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag> tags, OrderStatus status, string? userNote, DateTimeOffset createdAt, DateTimeOffset? settledAt)
    {
        var order = new GoldOrder(userId, type, weight, unitPrice, fee, tags, createdAt, status)
        { Id = id, UserNote = userNote, SettledAt = settledAt };
        order._domainEvents.Clear();
        return order;
    }
    private void RaiseSettlementEvent(DateTimeOffset occurredAt)
    {
        _domainEvents.Add(Type == OrderType.Buy
            ? new GoldPurchasedDomainEvent(Id, UserId, Weight, occurredAt)
            : new GoldSoldDomainEvent(Id, UserId, Weight, occurredAt));
    }
}

public interface IGoldOrderRepository
{
    Task AddAsync(GoldOrder order, CancellationToken cancellationToken = default);
    Task<GoldOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(GoldOrder order, CancellationToken cancellationToken = default) => Task.CompletedTask;
    Task<IReadOnlyList<GoldOrder>> SearchAsync(Guid userId, string? tag, DateTimeOffset? from, DateTimeOffset? to, OrderType? type, CancellationToken cancellationToken = default);
}
public interface IGoldWalletRepository
{
    Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveAsync(GoldWallet wallet, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default);
}

