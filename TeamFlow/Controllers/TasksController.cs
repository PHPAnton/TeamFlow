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
        var tasks = await _db.TaskItems
            .Include(t => t.Project)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskItem model)
    {
        model.Id = Guid.NewGuid();

        // Проверка, что проект существует
        var project = await _db.Projects.FindAsync(model.ProjectId);
        if (project == null) return BadRequest("Проект не найден");

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
        task.Deadline = model.Deadline;
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
