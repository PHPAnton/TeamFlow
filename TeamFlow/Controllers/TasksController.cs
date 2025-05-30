using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Models;

namespace TeamFlow.Controllers;

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

        var tasks = await _db.TaskItems
            .Include(t => t.Project)
            .Where(t => t.Project.OwnerId == guid)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskItem model)
    {
        if (!ModelState.IsValid)
        {
            foreach (var kv in ModelState)
            {
                Console.WriteLine($"Key: {kv.Key}");
                foreach (var err in kv.Value.Errors)
                {
                    Console.WriteLine($"  Error: {err.ErrorMessage}");
                }
            }
            return BadRequest(ModelState);
        }

        // Генерация ID и даты создания
        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;

        // Преобразуем дедлайн в UTC
        if (model.Deadline.HasValue)
            model.Deadline = DateTime.SpecifyKind(model.Deadline.Value, DateTimeKind.Utc);

        // Логирование
        Console.WriteLine("==== Полученные данные задачи ====");
        Console.WriteLine($"Title: {model.Title}");
        Console.WriteLine($"ProjectId: {model.ProjectId}");
        Console.WriteLine($"Priority: {model.Priority}");
        Console.WriteLine($"Status: {model.Status}");
        Console.WriteLine($"Tags: {string.Join(", ", model.Tags ?? new List<string>())}");

        // Проверка проекта
        var project = await _db.Projects.FindAsync(model.ProjectId);
        if (project == null)
        {
            Console.WriteLine("❌ Проект не найден");
            return BadRequest("Проект не найден");
        }

        // Убираем Project чтобы избежать ошибок EF
        model.Project = null;

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
