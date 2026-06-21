namespace MauiApp16.Models;

public class TagItem
{
    public string Name { get; set; }
    public string Emoji { get; set; }
    public bool IsSelected { get; set; }

    public string DisplayName => $"{Emoji} {Name}";
}
