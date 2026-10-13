namespace DaricTrading.Domain;

public sealed record GoldWeight
{
    public decimal Grams { get; }
    private GoldWeight(decimal grams) => Grams = grams;
    public static GoldWeight FromGrams(decimal grams)
    {
        if (grams <= 0 || decimal.Round(grams, 3) != grams) throw new ArgumentOutOfRangeException(nameof(grams), "Weight must be positive with at most 3 decimal places.");
        return new(grams);
    }
    internal static GoldWeight Zero => new(0);
    public GoldWeight Add(GoldWeight other) => FromGrams(Grams + other.Grams);
    public GoldWeight Subtract(GoldWeight other) => FromGrams(Grams - other.Grams);
}

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency = "IRR")
    {
        if (amount < 0 || decimal.Round(amount, 0) != amount) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative and use whole rial precision.");
        if (currency != "IRR") throw new ArgumentException("Only IRR is supported.", nameof(currency));
        Amount = amount; Currency = currency;
    }
}

public sealed record Tag
{
    public string Value { get; }
    private static readonly HashSet<string> Allowed = ["پس‌انداز_بلندمدت", "نوسان‌گیری_روزانه", "صندوق_طلای_فیزیکی", "تسویه_شمش"];
    public Tag(string value) { if (!Allowed.Contains(value)) throw new ArgumentException("Unsupported tag.", nameof(value)); Value = value; }
    public override string ToString() => Value;
}

public enum OrderType { Buy, Sell }
public enum OrderStatus { Settled, Pending, Cancelled }
public abstract record DomainEvent(DateTimeOffset OccurredAt);
public sealed record GoldPurchasedDomainEvent(Guid OrderId, Guid UserId, GoldWeight Weight, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
public sealed record GoldSoldDomainEvent(Guid OrderId, Guid UserId, GoldWeight Weight, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed class GoldWallet
{
    private readonly object _gate = new();
    public Guid UserId { get; }
    public GoldWeight Balance { get; private set; }
    public GoldWallet(Guid userId, GoldWeight? balance = null) { UserId = userId; Balance = balance ?? GoldWeight.Zero; }
    public void Credit(GoldWeight weight) { lock (_gate) Balance = GoldWeight.FromGrams(Balance.Grams + weight.Grams); }
    public void Debit(GoldWeight weight) { lock (_gate) { if (Balance.Grams < weight.Grams) throw new InsufficientGoldBalanceException(); var remain = Balance.Grams - weight.Grams; Balance = remain == 0 ? GoldWeight.Zero : GoldWeight.FromGrams(remain); } }
}

public sealed class GoldOrder
{
    private readonly List<Tag> _tags;
    public Guid Id { get; } public Guid UserId { get; } public OrderType Type { get; } public GoldWeight Weight { get; }
    public Money UnitPrice { get; } public Money Fee { get; } public Money TotalAmount { get; }
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly(); public string? Note { get; private set; }
    public OrderStatus Status { get; private set; } public DateTimeOffset CreatedAt { get; }
    private GoldOrder(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags, DateTimeOffset now)
    { Id=Guid.NewGuid(); UserId=userId; Type=type; Weight=weight; UnitPrice=unitPrice; Fee=fee; TotalAmount=new Money(decimal.Ceiling(weight.Grams*unitPrice.Amount)+fee.Amount); _tags=tags?.Distinct().ToList() ?? []; Status=OrderStatus.Settled; CreatedAt=now; }
    public static GoldOrder Buy(Guid userId, GoldWeight w, Money price, Money fee, IEnumerable<Tag>? tags=null, DateTimeOffset? now=null) => new(userId,OrderType.Buy,w,price,fee,tags,now??DateTimeOffset.UtcNow);
    public static GoldOrder Sell(Guid userId, GoldWeight w, Money price, Money fee, IEnumerable<Tag>? tags=null, DateTimeOffset? now=null) => new(userId,OrderType.Sell,w,price,fee,tags,now??DateTimeOffset.UtcNow);
    public static GoldOrder Pending(Guid userId, OrderType type, GoldWeight w, Money price, Money fee, DateTimeOffset? now=null) { var order=new GoldOrder(userId,type,w,price,fee,null,now??DateTimeOffset.UtcNow); order.Status=OrderStatus.Pending; return order; }
    public void UpdateTags(IEnumerable<Tag> tags, string? note=null) { if (Status != OrderStatus.Settled) throw new DomainRuleViolationException("Only settled orders can update tags or notes."); ArgumentNullException.ThrowIfNull(tags); _tags.Clear(); _tags.AddRange(tags.Distinct()); Note=note; }
    public void Cancel() { if(Status!=OrderStatus.Pending) throw new DomainRuleViolationException("Only pending orders can be cancelled."); Status=OrderStatus.Cancelled; }
}

public interface IGoldOrderRepository { Task AddAsync(GoldOrder order, CancellationToken ct=default); Task<GoldOrder?> GetAsync(Guid id, CancellationToken ct=default); Task<IReadOnlyList<GoldOrder>> SearchAsync(Guid userId, string? tag, DateTimeOffset? from, DateTimeOffset? to, OrderType? type, CancellationToken ct=default); }
public interface IGoldWalletRepository { Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken ct=default); }

