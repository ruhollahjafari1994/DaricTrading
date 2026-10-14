namespace DaricTrading.Domain;

/// <summary>سفارش خرید یا فروش طلا را نگه می‌دارد و تغییر وضعیت و اطلاعات مالی آن را طبق قواعد دامنه کنترل می‌کند.</summary>
public sealed class GoldOrder
{
    /// <summary>قفل هم‌زمانی برای هماهنگ‌کردن دسترسی‌های هم‌زمان به این شیء.</summary>
    private readonly object _gate = new();
    /// <summary>فهرست داخلی برچسب‌ها را نگه می‌دارد و از طریق property به صورت snapshot ارائه می‌کند.</summary>
    private readonly List<Tag> _tags;
    /// <summary>مقدار «_domainEvents» را در این بخش نگه می‌دارد.</summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>شناسه یکتای این موجودیت یا سفارش.</summary>
    public Guid Id { get; private set; }
    /// <summary>مالک قانونی این سفارش؛ جست‌وجو و جداسازی سفارش‌ها با این شناسه انجام می‌شود.</summary>
    public Guid UserId { get; private set; }
    /// <summary>نوع عملیات سفارش، خرید یا فروش.</summary>
    public OrderType Type { get; private set; }
    /// <summary>مقدار وزن طلای سفارش.</summary>
    public GoldWeight Weight { get; private set; }
    /// <summary>قیمت هر واحد وزن طلا.</summary>
    public Money UnitPrice { get; private set; }
    /// <summary>کارمزد سفارش.</summary>
    public Money Fee { get; private set; }
    /// <summary>مبلغ نهایی سفارش پس از اعمال کارمزد.</summary>
    public Money TotalAmount { get; private set; }
    /// <summary>برچسب‌های مرتبط با سفارش.</summary>
    public IReadOnlyCollection<Tag> Tags
    {
        get
        {
            lock (_gate)
            {
                return _tags.ToArray();
            }
        }
    }
    /// <summary>وضعیت فعلی سفارش در چرخه عمر آن.</summary>
    public OrderStatus Status { get; private set; }
    /// <summary>یادداشت اختیاری کاربر درباره سفارش.</summary>
    public string? UserNote { get; private set; }
    /// <summary>زمان ایجاد سفارش.</summary>
    public DateTimeOffset CreatedAt { get; private set; }
    /// <summary>زمان تسویه سفارش، در صورت تسویه شدن.</summary>
    public DateTimeOffset? SettledAt { get; private set; }
    /// <summary>رویدادهای ثبت‌شده روی سفارش که هنوز برای انتشار تحویل نشده‌اند.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
    {
        get
        {
            lock (_gate)
            {
                return _domainEvents.ToArray();
            }
        }
    }

    /// <summary>سازنده خصوصی برای بازسازی سفارش از داده‌های ذخیره‌شده است.</summary>
    private GoldOrder() { _tags = []; Weight = GoldWeight.Zero; UnitPrice = new Money(0); Fee = new Money(0); TotalAmount = new Money(0); }
    /// <summary>سازنده خصوصی مورد استفاده لایه ذخیره‌سازی برای بازسازی سفارش است.</summary>
    private GoldOrder(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags, DateTimeOffset now, OrderStatus status)
    {
        if (userId == Guid.Empty) throw new DomainRuleViolationException("Order user id is required.");
        if (type is not OrderType.Buy and not OrderType.Sell) throw new DomainRuleViolationException("Unsupported order type.");
        if (status is not OrderStatus.Pending and not OrderStatus.Settled and not OrderStatus.Cancelled)
            throw new DomainRuleViolationException("Unsupported order status.");
        ArgumentNullException.ThrowIfNull(weight);
        ArgumentNullException.ThrowIfNull(unitPrice);
        ArgumentNullException.ThrowIfNull(fee);
        if (unitPrice.Currency != fee.Currency) throw new DomainRuleViolationException("Price and fee currencies must match.");
        Id = Guid.NewGuid(); UserId = userId; Type = type; Weight = weight; UnitPrice = unitPrice; Fee = fee;
        Money gross;
        try
        {
            var grossAmount = decimal.Round(
                weight.Grams * unitPrice.Amount,
                2,
                MidpointRounding.ToEven);
            gross = new Money(grossAmount, unitPrice.Currency);
        }
        catch (OverflowException)
        {
            throw new DomainRuleViolationException("The calculated order amount exceeds the supported range.");
        }

        if (type == OrderType.Sell && fee.Amount > gross.Amount)
            throw new DomainRuleViolationException("Sell fee cannot exceed gross proceeds.");

        TotalAmount = type == OrderType.Buy ? gross + fee : gross - fee;
        var orderTags = tags?.ToArray() ?? [];
        if (orderTags.Any(tag => tag is null))
            throw new DomainRuleViolationException("Order tags cannot contain null values.");
        _tags = orderTags.Distinct().ToList();
        Status = status; CreatedAt = now;
        if (status == OrderStatus.Settled) SettledAt = now;
    }
    /// <summary>سفارش خرید را تسویه‌شده می‌سازد، مبلغ کل را در سازنده محاسبه و رویداد خرید را ثبت می‌کند.</summary>
    public static GoldOrder Buy(Guid userId, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags = null, DateTimeOffset? now = null) => CreateSettled(userId, OrderType.Buy, weight, unitPrice, fee, tags, now ?? DateTimeOffset.UtcNow);
    /// <summary>سفارش فروش تسویه‌شده را می‌سازد و رویداد فروش متناسب را ثبت می‌کند.</summary>
    public static GoldOrder Sell(Guid userId, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags = null, DateTimeOffset? now = null) => CreateSettled(userId, OrderType.Sell, weight, unitPrice, fee, tags, now ?? DateTimeOffset.UtcNow);
    /// <summary>سفارش را در وضعیت Pending و بدون رویداد تسویه می‌سازد.</summary>
    public static GoldOrder Pending(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, DateTimeOffset? now = null) => new(userId, type, weight, unitPrice, fee, null, now ?? DateTimeOffset.UtcNow, OrderStatus.Pending);
    /// <summary>سفارش Settled را می‌سازد و event متناسب با نوع خرید یا فروش را در aggregate ثبت می‌کند.</summary>
    private static GoldOrder CreateSettled(Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag>? tags, DateTimeOffset now)
    {
        var order = new GoldOrder(userId, type, weight, unitPrice, fee, tags, now, OrderStatus.Settled);
        order.RaiseSettlementEvent(now);
        return order;
    }
    /// <summary>فقط Pending را تسویه می‌کند، زمان SettledAt را می‌گذارد و رویداد مالی تولید می‌کند.</summary>
    public void Settle(DateTimeOffset? settledAt = null)
    {
        lock (_gate)
        {
            if (Status != OrderStatus.Pending) throw new InvalidOrderStateTransitionException("Only a pending order can be settled.");
            Status = OrderStatus.Settled;
            SettledAt = settledAt ?? DateTimeOffset.UtcNow;
            RaiseSettlementEvent(SettledAt.Value);
        }
    }
    /// <summary>metadata سفارش تسویه‌شده را جایگزین می‌کند و وزن، قیمت، کارمزد و مبلغ کل را تغییر نمی‌دهد.</summary>
    public void UpdateTags(IEnumerable<Tag> tags, string? userNote)
    {
        ArgumentNullException.ThrowIfNull(tags);
        if (userNote?.Length > 500) throw new DomainRuleViolationException("Order notes cannot exceed 500 characters.");
        var updatedTags = tags.ToArray();
        if (updatedTags.Any(tag => tag is null))
            throw new DomainRuleViolationException("Order tags cannot contain null values.");
        var distinctTags = updatedTags.Distinct().ToList();
        lock (_gate)
        {
            if (Status != OrderStatus.Settled) throw new InvalidOrderStateTransitionException("Only settled orders can update tags or notes.");
            _tags.Clear();
            _tags.AddRange(distinctTags);
            UserNote = userNote;
        }
    }
    /// <summary>فقط سفارش Pending را لغو و OrderCancelledDomainEvent را ثبت می‌کند.</summary>
    public void Cancel(DateTimeOffset? cancelledAt = null)
    {
        lock (_gate)
        {
            if (Status != OrderStatus.Pending) throw new InvalidOrderStateTransitionException("Only pending orders can be cancelled.");
            Status = OrderStatus.Cancelled;
            _domainEvents.Add(new OrderCancelledDomainEvent(Id, UserId, cancelledAt ?? DateTimeOffset.UtcNow));
        }
    }
    /// <summary>رویدادها را به صورت snapshot تحویل می‌دهد و فهرست داخلی aggregate را خالی می‌کند.</summary>
    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
    {
        lock (_gate)
        {
            var events = _domainEvents.ToArray();
            _domainEvents.Clear();
            return events;
        }
    }
    /// <summary>داده ذخیره‌شده را با بررسی شناسه، status و سازگاری زمان تسویه به aggregate بازمی‌گرداند.</summary>
    public static GoldOrder Restore(Guid id, Guid userId, OrderType type, GoldWeight weight, Money unitPrice, Money fee, IEnumerable<Tag> tags, OrderStatus status, string? userNote, DateTimeOffset createdAt, DateTimeOffset? settledAt)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Order id is required.");
        if (tags is null) throw new DomainRuleViolationException("Order tags are required.");
        if (userNote?.Length > 500) throw new DomainRuleViolationException("Order notes cannot exceed 500 characters.");
        if ((status == OrderStatus.Settled) != settledAt.HasValue)
            throw new DomainRuleViolationException("Only settled orders can have a settlement timestamp.");

        var order = new GoldOrder(userId, type, weight, unitPrice, fee, tags, createdAt, status)
        { Id = id, UserNote = userNote, SettledAt = settledAt };
        order._domainEvents.Clear();
        return order;
    }
    /// <summary>براساس نوع سفارش، GoldPurchased یا GoldSold را به صف رویدادهای aggregate اضافه می‌کند.</summary>
    private void RaiseSettlementEvent(DateTimeOffset occurredAt)
    {
        _domainEvents.Add(Type == OrderType.Buy
            ? new GoldPurchasedDomainEvent(Id, UserId, Weight, occurredAt)
            : new GoldSoldDomainEvent(Id, UserId, Weight, occurredAt));
    }
}
