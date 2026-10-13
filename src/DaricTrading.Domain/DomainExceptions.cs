namespace DaricTrading.Domain;

public class DomainRuleViolationException(string message) : Exception(message);
public sealed class InsufficientGoldBalanceException() : DomainRuleViolationException("Insufficient gold balance.");
