public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string GoogleId { get; set; } = null!;

    public List<Folder> Folders { get; set; } = new();
    
}