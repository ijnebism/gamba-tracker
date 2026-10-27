public class Folder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public Guid UserId { get; set; }
    public User? User { get; set; }

    // Null when the folder is at the user's root
    public Guid? ParentFolderId { get; set; }

    public List<Tracker> Trackers { get; set; } = new();
}