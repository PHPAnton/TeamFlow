using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Models;

namespace TeamFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly TeamFlowContext _db;

    public ProjectsController(TeamFlowContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        var guid = Guid.Parse(userId);
        var projects = await _db.Projects
            .Where(p => p.OwnerId == guid)
            .ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Project model)
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        model.Id = Guid.NewGuid();
        model.OwnerId = Guid.Parse(userId);
        _db.Projects.Add(model);
        await _db.SaveChangesAsync();

        return Ok(model);
    }
}
