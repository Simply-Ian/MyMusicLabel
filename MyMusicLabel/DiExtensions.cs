using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Commands;
using MyMusicLabel.Infrastructure.Services.Logging;
using MyMusicLabel.Services;

namespace MyMusicLabel;

public static class DiExtensions
{
    public static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        var commandTypes = typeof(DiExtensions).Assembly.GetTypes()
            .Where(type => type.GetInterface(nameof(ICommand)) != null);
        foreach (var commandType in commandTypes)
            services.AddKeyedTransient(typeof(ICommand), ExtractKey(commandType), commandType);

        return services;

        string ExtractKey(Type commandType) => commandType.CustomAttributes
            .First(
                attr => attr.AttributeType == typeof(CommandName)
            ).ConstructorArguments.First().Value?.ToString() ?? "";
    }

    public static IServiceCollection RegisterAppServices(
        this IServiceCollection services,
        Dictionary<string, Guid> platformAliases,
        IConfiguration configuration,
        ConsoleCustomLogger launchLogger
    )
    {
        services.AddSingleton<AppRunner>()
            .AddSingleton<IArtistResolver, DictionaryArtistResolver>()
            .AddSingleton<ITrackResolver, DictionaryTrackResolver>()
            .AddSingleton<IArtistLookup, ArtistLookup>()
            .AddSingleton<IPlatformResolver>(new DictionaryPlatformResolver(platformAliases))
            .AddCustomLogging(configuration, launchLogger);
        return services;
    }

    private static IServiceCollection AddCustomLogging(this IServiceCollection services, IConfiguration configuration,
        ConsoleCustomLogger launchLogger)
    {
        try
        {
            var loggingSection = configuration.GetRequiredSection("Logging");
            var loggingMode = loggingSection.GetSection("Mode").Value;
            
            if (loggingMode == "File") _addFileLogging(services, loggingSection);
            else _addConsoleLogging(services, launchLogger, loggingMode);
        }
        catch (InvalidOperationException)
        {
            launchLogger.LogWarning("Не указаны настройки метода логирования в appsettings.json. " +
                                    "Лог будет выводиться в консоль");
            services.AddTransient<ICustomLogger, ConsoleCustomLogger>();
        }
        catch (ArgumentException)
        {
            launchLogger.LogWarning("Не указан путь к файлу логов. Лог будет выводиться в консоль");
            services.AddTransient<ICustomLogger, ConsoleCustomLogger>();
        }

        return services;
    }

    private static void _addConsoleLogging(IServiceCollection services, ConsoleCustomLogger launchLogger,
        string? loggingMode)
    {
        services.AddTransient<ICustomLogger, ConsoleCustomLogger>();
        if (loggingMode != "Console")
            launchLogger.LogWarning($"Указан неизвестный метод логирования {loggingMode}. " +
                                    "Лог будет выводиться в консоль");
    }

    private static void _addFileLogging(IServiceCollection services, IConfigurationSection loggingSection)
    {
        var filepath = loggingSection.GetSection("Filepath").Value;
        if (filepath is null) throw new ArgumentException(filepath);
        services.AddTransient<ICustomLogger>(_ => new FileCustomLogger(filepath));
    }
}