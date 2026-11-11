using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMusicLabel;
using MyMusicLabel.Commands.Validators;
using MyMusicLabel.Domain;
using MyMusicLabel.Infrastructure;
using MyMusicLabel.Infrastructure.Services.Logging;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings{DisableDefaults = true});
var launchLogger = new ConsoleCustomLogger();
Dictionary<string, Guid> aliases;
try
{
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    aliases = ReadAliases(builder, launchLogger);
}
catch (FileNotFoundException exc)
{
    launchLogger.LogCritical("Не найден конфигурационный файл appsettings.json; приложение будет остановлено", exc);
    return 1;
}
catch (Exception exc) when (exc is InvalidDataException or FormatException)
{
    launchLogger.LogCritical("Ошибка чтения конфигурационного файла", exc);
    return 1;
}

builder.Services
    .RegisterRepositories()
    .RegisterPlatformAdapters(aliases)
    .RegisterAppServices(aliases, builder.Configuration, launchLogger)
    .RegisterPlatformDomainServices(aliases)
    .RegisterDomainServices()
    .RegisterCommands()
    .AddTransient<IValidator<CreateArtistInput>, CreateArtistValidator>();

using var app = builder.Build();
app.Start();
app.Services.GetRequiredService<AppRunner>().Run();
return 0;

Dictionary<string, Guid> ReadAliases(HostApplicationBuilder hostApplicationBuilder, ConsoleCustomLogger consoleCustomLogger)
{
    var platformNames = hostApplicationBuilder.Configuration.GetSection("Platforms").Get<string[]>();
    if (platformNames is null)
        throw new FormatException("Конфигурационный файл не содержит списка платформ");
    if (!platformNames.Any())
        consoleCustomLogger.LogWarning("Список платформ в файле appsettings.json пуст; " +
                                       "приложение запустится, но публиковать релизы и получать статистику будет невозможно");
    return new Dictionary<string, Guid>(
        platformNames.Select(alias => new KeyValuePair<string, Guid>(alias, Guid.NewGuid())),
        StringComparer.OrdinalIgnoreCase
    );
}