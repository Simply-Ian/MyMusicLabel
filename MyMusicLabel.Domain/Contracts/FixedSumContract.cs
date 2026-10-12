namespace MyMusicLabel.Domain.Contracts;

internal record FixedSumContract(decimal Price) : IContractImplementation
{
    private readonly List<DateOnly> _occurenceDates = [];
    public decimal CalculatePayments(decimal totalIncome, DateOnly startDate, DateOnly endDate)
        => Price * _occurenceDates.Count(date => startDate <= date && date <= endDate);

    // Проверка на уникальность не нужна, т. к. в теории ничто не мешает двум релизам выйти в один день.
    public void AddOccurenceOfPaymentConditionDate(DateOnly date) => _occurenceDates.Add(date);
}