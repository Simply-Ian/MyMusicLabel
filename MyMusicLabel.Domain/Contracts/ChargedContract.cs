namespace MyMusicLabel.Domain.Contracts;

/// <summary>
/// 
/// </summary>
/// <param name="Charge">Коэффициент авторских отчислений. Должен находиться в диапазоне [0; 1]</param>
public record ChargedContract(decimal Charge) : IContractImplementation
{
    public IReadOnlyList<DateOnly> OccurenceDates => [];
    public decimal CalculatePayments(decimal totalIncome, DateOnly startDate, DateOnly endDate) => totalIncome * Charge;

    public void AddOccurenceOfPaymentConditionDate(DateOnly date)
    {
    }
}