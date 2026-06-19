using Microsoft.AspNetCore.Mvc;
using GambaTracker.Core.Interfaces;
using GambaTracker.Api.Services;
using GambaTracker.Core.DTOs;

namespace GambaTracker.Api.Controllers
{
  public class AuthController : ControllerBase
  {
      private readonly IUserRepository _userRepository;
      private readonly TokenService _tokenService;

      public AuthController(IUserRepository userRepository, TokenService tokenService)
      {
          _userRepository = userRepository;
          _tokenService = tokenService;
      }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var user = await _userRepository.GetUserByEmailAsync(login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new AuthResponse(token, user.Email, user.Name));
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(register.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already in use.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = register.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                Name = register.Name
            };

            var createdUser = await _userRepository.CreateUserAsync(newUser);
            var token = _tokenService.GenerateToken(createdUser);

            return Ok(new AuthResponse(token, createdUser.Email, createdUser.Name));
        }
      
  }
}