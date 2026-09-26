using MyMusicLabel.Domain.Contracts;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Domain.Services;

public class TrackProductionDomainService(ITrackRepository trackRepo)
{
    /// <summary>
    /// Позволяет с помощью Fluent API добавить к треку музыку или текст и одновременно указать условия оплаты
    /// </summary>
    /// <param name="trackId">Id трека, над которым выполняются операции</param>
    /// <returns>Объект builder, через который можно добавить музыку/текст/пометить как записанный</returns>
    public ProducerBuilder ProduceTrack(Guid trackId)
    {
        var track = trackRepo.GetTrack(trackId);
        return new ProducerBuilder(track, trackRepo);
    }

    public void RecordTrack(Guid trackId, Guid performerId, DateOnly date)
    {
        var track = trackRepo.GetTrack(trackId);
        track.MarkRecorded(performerId);
        trackRepo.StoreTrack(track);
    }
}

public delegate void TrackAction(Track track, OneTimeContract contract);

/// <summary>
/// Создает и настраивает объект TrackProducer, который выполняет операции над треком.
/// ВНИМАНИЕ: вызов метода AddMusic() / AddLyrics() не выполняет никаких действий над треком! Чтобы действительно
/// обновить состояние трека, нужно вызвать ForCharge() / ForFixedSum() у результата
/// </summary>
public class ProducerBuilder(Track track, ITrackRepository trackRepo)
{
    public TrackProducer AddMusic(Guid composerId, int durationSeconds)
    {
        return new TrackProducer(track, ProductionAction, trackRepo);

        void ProductionAction(Track target, OneTimeContract contract) => 
            target.AddMusic(composerId, contract, TimeSpan.FromSeconds(durationSeconds));
    }

    public TrackProducer AddLyrics(Guid lyricistId)
    {
        return new TrackProducer(track, ProductionAction, trackRepo);

        void ProductionAction(Track target, OneTimeContract contract) => target.AddLyrics(lyricistId, contract);
    }
}

public class TrackProducer(Track track, TrackAction action, ITrackRepository trackRepo)
{
    public void ForCharge(decimal charge, DateOnly date) => _applyProductionStep(new ChargedContract(charge), date);
    
    public void ForFixedSum(decimal price, DateOnly date) => _applyProductionStep(new FixedSumContract(price), date);

    private void _applyProductionStep(IContractImplementation contractImp, DateOnly date)
    {
        OneTimeContract contract = new(contractImp, date);
        action(track, contract);
        trackRepo.StoreTrack(track);
    }
}