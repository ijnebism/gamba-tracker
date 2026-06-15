public class Drop
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public Guid EventId { get; set; }
    public Event? Event { get; set; }
}