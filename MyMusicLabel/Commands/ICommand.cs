namespace MyMusicLabel.Commands;

internal interface ICommand
{
    void Invoke(string[] args, CommandContext context);
    
    string Help { get; }
    
    public string Name => GetType().CustomAttributes
        .First(
            attr => attr.AttributeType == typeof(CommandName)
        ).ConstructorArguments.First().Value?.ToString() ?? "";
}