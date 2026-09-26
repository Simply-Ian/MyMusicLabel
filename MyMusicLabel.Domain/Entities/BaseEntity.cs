namespace MyMusicLabel.Domain.Entities;

public class BaseEntity (Guid id)
{
    public Guid Id => id;
    
    // TODO: Implement equality by Id semantic
}