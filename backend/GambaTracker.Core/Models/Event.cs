public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Timestamp { get; set; }

    public Guid TrackerId { get; set; }
    public Tracker? Tracker { get; set; }

    public List<Drop> Drops { get; set; } = new();
}