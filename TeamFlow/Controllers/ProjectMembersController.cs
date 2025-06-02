using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Models;
using TeamFlow.Services;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectMembersController : ControllerBase
    {
        private readonly TeamFlowContext _db;
        private readonly IEmailService _emailService;
        public ProjectMembersController(TeamFlowContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetMembers(Guid projectId)
        {
            var members = await _db.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.User)
                .Select(m => new {
                    m.Id,
                    m.UserId,
                    m.Role,
                    Username = m.User.Username,
                    Email = m.User.Email
                })
                .ToListAsync();

            return Ok(members);
        }

        [HttpPost("{projectId}/add")]
        public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user != null)
            {
                var exists = await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == user.Id);
                if (exists) return BadRequest("Пользователь уже в проекте");

                var member = new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = user.Id,
                    Role = req.Role
                };

                _db.ProjectMembers.Add(member);
                await _db.SaveChangesAsync();

                // Отправляем уведомление о добавлении в проект
                await _emailService.SendAsync(req.Email, "Вы добавлены в проект TeamFlow",
                    $"Вас добавили в проект. Войдите в TeamFlow для участия!");

                return Ok(new { message = "Участник добавлен" });
            }
            else
            {
                // Пользователь не найден — отправляем приглашение
                var invite = new ProjectInvite
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Email = req.Email,
                    Role = req.Role,
                    CreatedAt = DateTime.UtcNow,
                    Accepted = false
                };

                _db.ProjectInvites.Add(invite);
                await _db.SaveChangesAsync();

                // Ссылка на приглашение
                var inviteUrl = $"https://localhost:7143/invite/{invite.Id}";
                var mailBody = $"Вас пригласили в проект TeamFlow!\nПерейдите по ссылке для регистрации и вступления в проект:\n{inviteUrl}";

                await _emailService.SendAsync(req.Email, "Приглашение в TeamFlow", mailBody);

                return Ok(new { message = "Приглашение отправлено!" });
            }
        }

        [HttpPost("accept-invite/{inviteId}")]
        public async Task<IActionResult> AcceptInvite(Guid inviteId, [FromBody] AcceptInviteRequest req)
        {
            var invite = await _db.ProjectInvites.FirstOrDefaultAsync(x => x.Id == inviteId && !x.Accepted);
            if (invite == null)
                return BadRequest("Приглашение не найдено или уже использовано");

            // Проверяем, есть ли уже пользователь
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == invite.Email);
            bool isNew = false;
            if (user == null)
            {
                var token = Guid.NewGuid().ToString("N");
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = invite.Email,
                    Username = req.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                    IsEmailConfirmed = false,
                    EmailConfirmationToken = token,
                    EmailTokenExpires = DateTime.UtcNow.AddDays(2)
                };
                _db.Users.Add(user);
                isNew = true;
                await _db.SaveChangesAsync();

                // Отправить письмо с подтверждением
                var confirmUrl = $"https://localhost:7143/confirm?token={token}";
                var mailBody = $"Вы зарегистрировались через приглашение! Пожалуйста, подтвердите свою почту:\n{confirmUrl}";
                await _emailService.SendAsync(user.Email, "Подтверждение почты TeamFlow", mailBody);
            }

            // Добавляем в проект
            var memberExists = await _db.ProjectMembers.AnyAsync(m => m.ProjectId == invite.ProjectId && m.UserId == user.Id);
            if (!memberExists)
            {
                var member = new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    ProjectId = invite.ProjectId,
                    UserId = user.Id,
                    Role = invite.Role
                };
                _db.ProjectMembers.Add(member);
            }

            invite.Accepted = true;
            await _db.SaveChangesAsync();

            return Ok("Регистрация прошла успешно! Проверьте почту и подтвердите e-mail.");
        }

        [HttpGet("/api/invites/{inviteId}")]
        public async Task<IActionResult> GetInvite(Guid inviteId)
        {
            var invite = await _db.ProjectInvites.FirstOrDefaultAsync(i => i.Id == inviteId && !i.Accepted);
            if (invite == null) return NotFound();
            return Ok(new { email = invite.Email });
        }

        [HttpPost("{projectMemberId}/role")]
        public async Task<IActionResult> ChangeRole(Guid projectMemberId, [FromBody] ChangeRoleRequest req)
        {
            var member = await _db.ProjectMembers.FindAsync(projectMemberId);
            if (member == null) return NotFound();

            member.Role = req.Role;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{projectMemberId}")]
        public async Task<IActionResult> RemoveMember(Guid projectMemberId)
        {
            var member = await _db.ProjectMembers.FindAsync(projectMemberId);
            if (member == null) return NotFound();

            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync();
            return Ok();
        }

        public class AddMemberRequest
        {
            public string Email { get; set; }
            public ProjectRole Role { get; set; }
        }

        public class ChangeRoleRequest
        {
            public ProjectRole Role { get; set; }
        }

        public class AcceptInviteRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
