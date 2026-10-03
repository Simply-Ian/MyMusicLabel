namespace MyMusicLabel.Commands;

/// <summary>Именование команд через атрибут для удобства регистрации в DI: в отличие от свойства, не нужно
/// создавать объект</summary>
public class CommandName(string uniqueName) : Attribute
{
}