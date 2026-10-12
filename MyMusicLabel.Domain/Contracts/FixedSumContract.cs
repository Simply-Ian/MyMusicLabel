using MyMusicLabel.Domain.Collections.Chain;

namespace MyMusicLabel.Domain.Contracts;

internal record FixedSumContract(decimal Price) : IContractImplementation
{
    private readonly Chain<DateOnly> _occurenceDates = [];
    public decimal CalculatePayments(decimal totalIncome, DateOnly startDate, DateOnly endDate)
        => Price * _occurenceDates.CountInInterval(startDate, endDate);

    // Проверка на уникальность не нужна, т. к. в теории ничто не мешает двум релизам выйти в один день.
    public void AddOccurenceOfPaymentConditionDate(DateOnly date) => _occurenceDates.Add(date);
}