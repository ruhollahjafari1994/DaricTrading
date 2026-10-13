namespace DaricTrading.Domain;

public class DomainRuleViolationException(string message) : Exception(message);
public sealed class InsufficientGoldBalanceException() : DomainRuleViolationException("Insufficient gold balance.");
public sealed class InvalidOrderStateTransitionException(string message) : DomainRuleViolationException(message);
public sealed class ConcurrencyConflictException(string message) : DomainRuleViolationException(message);
