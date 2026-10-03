using System.Globalization;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

internal class TrackProductionParser(
    ITrackResolver trackResolver,
    IArtistLookup artistLookup,
    ITrackRepository trackRepo
)
{
    internal record ProductionRequest(Track Track, Artist Author, ContractTerms Terms, DateOnly Date);

    internal record ContractTerms(bool IsCharge, decimal Value)
    {
        public static string? TryParse(string typeArg, string valueArg, out ContractTerms terms)
        {
            terms = null!;
            var isCharge = typeArg.Equals("charge", StringComparison.OrdinalIgnoreCase);
            if (!isCharge && !typeArg.Equals("fixed", StringComparison.OrdinalIgnoreCase))
                return "Неизвестный тип контракта. Доступны: charge, fixed";

            if (!decimal.TryParse(valueArg, NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
                return $"Не удалось прочитать числовое значение \"{valueArg}\"";

            if (!isCharge && value < 0) return "Сумма выплаты не может быть отрицательной";

            terms = new ContractTerms(isCharge, value);
            return null;
        }
    }

    public string? TryResolve(string trackAlias, string artistAlias, out Track track, out Artist artist)
    {
        track = null!;
        artist = null!;
        try
        {
            track = trackRepo.GetTrack(trackResolver.Resolve(trackAlias));
        }
        catch (KeyNotFoundException)
        {
            return "Трек с указанным коротким именем не найден";
        }

        if (!artistLookup.TryGetArtist(artistAlias, out artist))
            return "Артист с указанным коротким именем не найден";

        return null;
    }

    public string? TryParseRequest(IReadOnlyList<string> args, DateOnly date, out ProductionRequest request)
    {
        request = null!;
        if (!args[^3].Equals("for", StringComparison.OrdinalIgnoreCase))
            return "Укажите условия оплаты: for charge <процент> либо for fixed <сумма>";

        var error = TryResolve(args[1], args[3], out var track, out var author);
        if (error is not null) return error;

        error = ContractTerms.TryParse(args[^2], args[^1], out var terms);
        if (error is not null) return error;

        request = new ProductionRequest(track, author, terms, date);
        return null;
    }

    public string? TryRun(string subject, Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException exc)
        {
            return $"Невозможно {subject}: {exc.Message}";
        }
        return null;
    }

    public string? TryProduce(string subject, ProductionRequest request, Func<TrackProducer> beginProduction)
        => TryRun($"добавить {subject}", () =>
        {
            var producer = beginProduction();
            if (request.Terms.IsCharge) producer.ForCharge(request.Terms.Value / 100, request.Date);
            else producer.ForFixedSum(request.Terms.Value, request.Date);
        });

    public string DescribeTerms(ContractTerms terms, string subject)
        => terms.IsCharge ? $"отчисления {terms.Value}% от дохода" : $"выплата {terms.Value} руб. единоразово";
}
