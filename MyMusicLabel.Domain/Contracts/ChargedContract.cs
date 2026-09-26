namespace MyMusicLabel.Domain.Contracts;

/// <summary>
/// 
/// </summary>
/// <param name="charge">Коэффициент авторских отчислений. Должен находиться в диапазоне [0; 1]</param>
public class ChargedContract(decimal charge) : IContractImplementation
{
    public IReadOnlyList<DateOnly> OccurenceDates => [];
    public decimal CalculatePayments(decimal totalIncome, DateOnly startDate, DateOnly endDate) => totalIncome * charge;

    public void AddOccurenceOfPaymentConditionDate(DateOnly date)
    {
    }
}