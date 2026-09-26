namespace MyMusicLabel.Domain.Contracts;

public class OneTimeContract
{
    private IContractImplementation _imp;
    /// <summary>
    /// Считает отчисления артисту на основе дохода от треков
    /// </summary>
    /// <param name="totalIncome">Полный доход за все треки, выпущенные согласно контракту</param>
    /// <param name="startDate">Начало периода, за который рассчитывается выплата</param>
    /// <param name="endDate">Конец периода, за который рассчитывается выплата (включительно)</param>
    /// <returns>Общий объем выплат артисту на основе полученных сумм.</returns>
    public decimal CalculatePayment(decimal totalIncome, DateOnly startDate, DateOnly endDate) 
        => _imp.CalculatePayments(totalIncome, startDate, endDate);

    public OneTimeContract(IContractImplementation imp, DateOnly date)
    {
        _imp = imp;
        _imp.AddOccurenceOfPaymentConditionDate(date);
    }
}