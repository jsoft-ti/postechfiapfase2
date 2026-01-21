using Domain.Enums;

namespace Domain.ValuesObjects;

public sealed class Promotion : ValueObject
{
    public PromotionType Type { get; }
    public decimal Value { get; }
    public DateTime StartOf { get; }
    public DateTime EndOf { get; }

    private Promotion(PromotionType type, decimal value, DateTime startOf, DateTime endOf)
    {
        Type = type;
        Value = value;
        StartOf = startOf;
        EndOf = endOf;
    }

    public static Promotion None => new Promotion(PromotionType.None, 0, DateTime.MinValue, DateTime.MinValue);

    public static Promotion Create(PromotionType tipo, decimal valor, DateTime inicio, DateTime fim)
    {
        if (tipo != PromotionType.None && fim <= inicio)
            throw new ArgumentException("Data de fim deve ser posterior à data de início.");

        return new Promotion(tipo, valor, inicio, fim);
    }

    public bool IsActive(DateTime data) => Type != PromotionType.None && data >= StartOf && data <= EndOf;

    public decimal ApplyDiscount(decimal precoOriginal)
    {
        if (!IsActive(DateTime.UtcNow)) return precoOriginal;

        return Type switch
        {
            PromotionType.FixedDiscount => precoOriginal - Value,
            PromotionType.PercentageDiscount => precoOriginal * (1 - Value / 100),
            _ => precoOriginal
        };
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return Value;
        yield return StartOf;
        yield return EndOf;
    }
}