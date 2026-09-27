namespace MyMusicLabel.Domain.Contracts;

internal record PersistentContract
{
    private IContractImplementation Imp;
    public DateOnly SignDate { get; }
    private DateOnly? BreakDate { get; init; }
    public bool IsActiveToDate(DateOnly date) => 
        SignDate <= date 
        && date <= BreakDate 
        && _releaseDates.Count(relDate => relDate <= date) < PlannedReleaseCount;
    
    // Количество релизов, которые исполнитель обязан выпустить на лейбле по условиям договора
    private uint PlannedReleaseCount { get; }

    private readonly List<DateOnly> _releaseDates = [];

    public void AddNewRelease(DateOnly releaseDate)
    {
        if (IsActiveToDate(releaseDate)) _releaseDates.Add(releaseDate);
        else throw new BrokenContractException();
    }

    /* Явно прописанный конструктор вместо сгенерированного, т. к. нужно инициализировать приватное
    поле Imp, а PersistentContract - это record (т. е. поле первичного конструктора будет публичным свойством) */
    public PersistentContract(IContractImplementation imp, DateOnly signDate, uint plannedReleaseCount)
    {
        SignDate = signDate;
        Imp = imp;
        PlannedReleaseCount = plannedReleaseCount;
    }

    /// <summary>
    /// Считает отчисления артисту на основе дохода от треков
    /// </summary>
    /// <param name="totalIncome">Полный доход за все треки, выпущенные согласно контракту</param>
    /// <param name="startDate">Начало периода, за который рассчитывается выплата</param>
    /// <param name="endDate">Конец периода, за который рассчитывается выплата (включительно)</param>
    /// <returns>Общий объем выплат артисту на основе полученных сумм.</returns>
    public decimal CalculatePayments(decimal totalIncome, DateOnly startDate, DateOnly endDate)
        => Imp.CalculatePayments(totalIncome, startDate, endDate);

    public void AddOccurenceOfPaymentConditionDate(DateOnly date) => Imp.AddOccurenceOfPaymentConditionDate(date);

    public PersistentContract Break(DateOnly date)
    {
        if (date < SignDate) 
            throw new InvalidOperationException("Contract cannot be broken before it was signed. " +
                                                "Make sure you've specified the right break date");
        if (_releaseDates.Any(plannedDate => plannedDate >= date))
            throw new InvalidOperationException("Cannot break a persistent contract with artist until all the planned " +
                                                "releases he performs are published");
        return this with { BreakDate = date };
    }
}

public class BrokenContractException() : Exception("Contract is broken");