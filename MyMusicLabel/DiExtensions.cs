using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Commands;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Infrastructure.Services;

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
    
    public static IServiceCollection RegisterAppServices(this IServiceCollection services) => 
        services.AddSingleton<AppRunner>();
}