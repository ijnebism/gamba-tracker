namespace GambaTracker.Core.DTOs
{
    public record Login(string Email, string Password);
    public record Register(string Email, string Password, string Name);
    public record AuthResponse(string Token, string Email, string Name);

}