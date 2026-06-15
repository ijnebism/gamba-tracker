public class Tracker
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;

    public Guid FolderId { get; set; }
    public Folder? Folder { get; set; }

    public List<Event> Events { get; set; } = new();
}