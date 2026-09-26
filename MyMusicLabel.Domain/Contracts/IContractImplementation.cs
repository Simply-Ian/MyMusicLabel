namespace MyMusicLabel.Domain.Contracts;

public interface IContractImplementation
{
    IReadOnlyList<DateOnly> OccurenceDates { get; }
    
    /// <summary>
    /// Считает отчисления артисту на основе дохода от треков
    /// </summary>
    /// <param name="totalIncome">Полный доход за все треки, выпущенные согласно контракту</param>
    /// <param name="startDate">Начало периода, за который рассчитывается выплата</param>
    /// <param name="endDate">Конец периода, за который рассчитывается выплата (включительно)</param>
    /// <returns>Общий объем выплат артисту на основе полученных сумм.</returns>
    decimal CalculatePayments(decimal totalIncome, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Дата выполнения платежного условия - дата, когда наступает событие, по которому контрактом предусмотрена выплата -
    /// - в данном случае публикация релиза. Эффект оказывает только для контрактов с фиксированной суммой, контракты со
    /// ставкой этот вызов игнорируют.
    /// </summary>
    /// <param name="date"></param>
    void AddOccurenceOfPaymentConditionDate(DateOnly date);
}