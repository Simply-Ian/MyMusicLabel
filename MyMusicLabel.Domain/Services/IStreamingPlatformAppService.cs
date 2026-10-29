using System.ComponentModel;
using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Services;

/// <summary>
/// Реализации этого интерфейса описывают чисто технические детали взаимодействия с API платформы. Вся бизнес-логика
/// находится в StreamingPlatform.
/// Если бы мы делали интеграции с настоящими стримингами, то креды для API хранились бы в реализациях
/// </summary>
public interface IStreamingPlatformAppService
{
    void Publish(Release release, Guid artistId, DateOnly date);
    IEnumerable<int> GetTrackListeningsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate);

    /*
     * Без GUI непросто придумать, куда приткнуть событие, поэтому я вспомнил, что был такой довольно уже древний способ
     * реализации асинхронных операций, как Event-based Asynchronous Pattern
     * (https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-overview)
     */
    
    // Не то что бы этот делегат тут так сильно нужен: я его оставил только ради осмысленных имен параметров
    public delegate void GetTrackListeningsCompletedEventHandler(object sender, GetTrackListeningsCompletedArgs e);
    
    /*
     * В https://habr.com/ru/companies/jugru/articles/491236/ кратко описывается вариант, в котором событие определяется
     * внутри класса MethodNameCompletedArgs, и в нем же, соответственно - обработчики. Вызывающий код создает экземпляр
     * MethodNameCompletedArgs, настраивает обработчики и передает его в MethodNameAsync(), а тот вызывает событие,
     * по-видимому, через отдельный публичный метод MethodNameCompletedArgs.Complete(). Этот вариант сильно отличается
     * от эталонного примера от MS (https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/implementing-the-event-based-asynchronous-pattern),
     * который я скопировал. В варианте с Хабра становится ненужным userState, но непонятно, как реализуется отмена.
     * Собственно, я переписал пример от MS потому, что конкурентные вызовы мне не грозят. 
     */
    public event GetTrackListeningsCompletedEventHandler? GetTrackListeningsCompleted;
    public class GetTrackListeningsCompletedArgs(
        IEnumerable<int>? listenings,
        Exception? exc,
        bool cancelled,
        object? userState) : AsyncCompletedEventArgs(exc, cancelled, userState)
    {
        public IEnumerable<int>? Listenings { get; set; } = listenings;
    }
    void GetTrackListeningsInPeriodAsync( Guid trackId, DateOnly startDate, DateOnly endDate );

    public void GetTrackListeningsInPeriodAsyncCancel();
    IEnumerable<decimal> GetTrackPaymentsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate);
    void Revoke(Guid releaseId, Guid artistId);
}