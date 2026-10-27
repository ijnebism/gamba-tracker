using System.ComponentModel.DataAnnotations;

namespace GambaTracker.Core.DTOs
{
    // e.g. { "itemName": "Wheat", "quantity": 3 }
    public record DropDto([Required, StringLength(100)] string ItemName, [Range(1, int.MaxValue)] int Quantity);

    // DateTimeOffset so timestamps without a timezone can't be stored ambiguously; they're saved as UTC
    public record CreateEventRequest(DateTimeOffset Timestamp, [Required, MinLength(1)] List<DropDto> Drops);

    public record EventResponse(Guid Id, DateTime Timestamp, IEnumerable<DropDto> Drops);
}
