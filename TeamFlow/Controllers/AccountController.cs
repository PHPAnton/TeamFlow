
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFlow.Data;
using TeamFlow.Models;
using TeamFlow.Services;

namespace TeamFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly TeamFlowContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _email;

    public AccountController(TeamFlowContext db, IConfiguration config, IEmailService email)
    {
        _db = db;
        _config = config;
        _email = email;
    }

    [HttpPost("register")]
    

    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
            return BadRequest("Email уже зарегистрирован");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = model.Username,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            EmailConfirmationToken = Guid.NewGuid().ToString()
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var confirmLink = $"https://localhost:5173/confirm?token={user.EmailConfirmationToken}";
        await _email.SendAsync(model.Email, "Подтверждение Email", $"Перейдите по ссылке: {confirmLink}");

        return Ok(new { message = "Пользователь создан. Подтвердите почту." });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == model.Token);
        if (user == null) return BadRequest("Неверный токен");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Email подтверждён" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            return Unauthorized("Неверные данные");

        if (!user.IsEmailConfirmed)
            return Unauthorized("Email не подтвержден");

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    [HttpPost("reset-request")]
    public async Task<IActionResult> ResetRequest(ResetRequest model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null) return Ok(); // безопасно ничего не говорить

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpiration = DateTime.UtcNow.AddMinutes(30);
        await _db.SaveChangesAsync();

        var link = $"https://localhost:5173/reset?token={user.PasswordResetToken}";
        await _email.SendAsync(model.Email, "Сброс пароля", $"Сбросить пароль: {link}");

        return Ok(new { message = "Ссылка отправлена" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == model.Token &&
            u.PasswordResetTokenExpiration > DateTime.UtcNow);

        if (user == null) return BadRequest("Недействительный или просроченный токен");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiration = null;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Пароль изменён" });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12), // ✅ важный момент!
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
