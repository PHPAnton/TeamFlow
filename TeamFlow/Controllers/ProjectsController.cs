using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        var memberProjectIds = await _db.ProjectMembers
            .Where(pm => pm.UserId == guid)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var projects = await _db.Projects
            .Where(p => memberProjectIds.Contains(p.Id))
            .ToListAsync();

        return Ok(projects);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Project model)
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        model.Id = Guid.NewGuid();
        var guid = Guid.Parse(userId);
        model.OwnerId = guid;
        _db.Projects.Add(model);

        // 👇 Автоматически делаем владельца участником (роль Owner)
        var ownerMember = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = model.Id,
            UserId = guid,
            Role = ProjectRole.Owner // или твой enum/string
        };
        _db.ProjectMembers.Add(ownerMember);

        await _db.SaveChangesAsync();

        // Вернем только нужные поля!
        return Ok(new
        {
            model.Id,
            model.Title,
            model.Description
        });
    }

}
