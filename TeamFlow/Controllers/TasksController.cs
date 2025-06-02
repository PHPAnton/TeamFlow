using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TeamFlowContext _db;

    public TasksController(TeamFlowContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();
        var guid = Guid.Parse(userId);

        // Задачи всех проектов, где юзер участник
        var userProjectIds = await _db.ProjectMembers
            .Where(pm => pm.UserId == guid)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var tasks = await _db.TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Where(t => userProjectIds.Contains(t.ProjectId))
            .Select(t => new {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.Deadline,
                t.CreatedAt,
                t.Tags,
                Project = new
                {
                    t.Project.Id,
                    t.Project.Title
                },
                AssignedUser = t.AssignedUser == null ? null : new
                {
                    t.AssignedUser.Id,
                    t.AssignedUser.Username,
                    t.AssignedUser.Email
                },
                t.AssignedUserId
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskItem model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;

        // Дедлайн в UTC
        if (model.Deadline.HasValue)
            model.Deadline = DateTime.SpecifyKind(model.Deadline.Value, DateTimeKind.Utc);

        // Проверка проекта
        var project = await _db.Projects.FindAsync(model.ProjectId);
        if (project == null) return BadRequest("Проект не найден");

        model.Project = null; // чтобы EF не ругался

        // --- если AssignedUserId не передан, назначаем текущего пользователя ---
        if (model.AssignedUserId == null || model.AssignedUserId == Guid.Empty)
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId != null) model.AssignedUserId = Guid.Parse(userId);
        }

        _db.TaskItems.Add(model);
        await _db.SaveChangesAsync();

        return Ok(model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskItem model)
    {
        var task = await _db.TaskItems.FindAsync(id);
        if (task == null) return NotFound();

        task.Title = model.Title;
        task.Description = model.Description;
        task.Status = model.Status;
        task.Priority = model.Priority;

        if (model.Deadline.HasValue)
            task.Deadline = DateTime.SpecifyKind(model.Deadline.Value, DateTimeKind.Utc);
        else
            task.Deadline = null;

        task.Tags = model.Tags;

        // --- Обновление AssignedUserId ---
        task.AssignedUserId = model.AssignedUserId;

        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _db.TaskItems.FindAsync(id);
        if (task == null) return NotFound();

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
