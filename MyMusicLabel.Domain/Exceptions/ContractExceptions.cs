using MyMusicLabel.Domain.Contracts;

namespace MyMusicLabel.Domain.Exceptions;

public class ContractInactiveException(DateOnly date) 
    : InvalidOperationException($"Контракт с артистом не действует на дату {date}")
{
    public DateOnly Date => date;
}

public class ContractNotFulfilledException(DateOnly date, IEnumerable<ReleasePlan> plannedReleases)
    : InvalidOperationException("У артиста есть релизы, публикация которых запланирована на более позднюю дату")
{
    public IEnumerable<ReleasePlan> PlannedReleases => plannedReleases;
}