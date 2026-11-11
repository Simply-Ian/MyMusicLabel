using MyMusicLabel.Domain.Exceptions;

namespace MyMusicLabel.Domain.Contracts;

internal record PersistentContract
{
    private IContractImplementation Imp;
    public DateOnly SignDate { get; }
    public DateOnly? BreakDate { get; private init; }
    public bool IsActiveToDate(DateOnly date) => 
        SignDate <= date 
        && date < (BreakDate ?? DateOnly.MaxValue) 
        && _plannedReleases.Count(rel => rel.Date <= date) < ReleasesByContractNumber;
    
    // Количество релизов, которые исполнитель обязан выпустить на лейбле по условиям договора
    private uint ReleasesByContractNumber { get; }

    private readonly List<ReleasePlan> _plannedReleases = [];

    public void AddNewRelease(Guid releaseId, DateOnly releaseDate)
    {
        if (IsActiveToDate(releaseDate))
        {
            _plannedReleases.Add(new ReleasePlan(releaseId, releaseDate));
            Imp.AddOccurenceOfPaymentConditionDate(releaseDate);
        }
        else throw new ContractInactiveException(releaseDate);
    }

    /* Явно прописанный конструктор вместо сгенерированного, т. к. нужно инициализировать приватное
    поле Imp, а PersistentContract - это record (т. е. поле первичного конструктора будет публичным свойством) */
    public PersistentContract(IContractImplementation imp, DateOnly signDate, uint plannedReleaseCount)
    {
        SignDate = signDate;
        Imp = imp;
        ReleasesByContractNumber = plannedReleaseCount;
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

    public PersistentContract Break(DateOnly date)
    {
        if (!IsActiveToDate(date)) throw new ContractInactiveException(date);
        var releasesAfterAttemptedDate = _plannedReleases
            .Where(rel => rel.Date > date)
            .ToList();
        if (releasesAfterAttemptedDate.Any()) throw new ContractNotFulfilledException(date, releasesAfterAttemptedDate);
        return this with { BreakDate = date };
    }
}

public record struct ReleasePlan(Guid Id, DateOnly Date);