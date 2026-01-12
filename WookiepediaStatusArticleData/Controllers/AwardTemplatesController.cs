using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("award-templates")]
public class AwardTemplatesController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var templates = await db.Set<AwardTemplate>()
            .Include(t => t.Criteria!)
                .ThenInclude(c => c.Project)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return View(new AwardTemplatesViewModel { Templates = templates });
    }

    [HttpGet("new")]
    public async Task<IActionResult> CreateForm(CancellationToken cancellationToken)
    {
        var projects = await db.Set<Project>()
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return View(new AwardTemplateFormViewModel { Projects = projects });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromForm] AwardTemplateForm form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var projects = await db.Set<Project>()
                .Where(p => !p.IsArchived)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            Response.StatusCode = 400;
            return View("CreateForm", new AwardTemplateFormViewModel { Form = form, Projects = projects });
        }

        var now = DateTime.UtcNow;
        var template = new AwardTemplate
        {
            Name = form.Name,
            Description = form.Description,
            Heading = form.Heading,
            Subheading = form.Subheading,
            Type = form.Type,
            CountMode = form.CountMode,
            IsActive = form.IsActive,
            SortOrder = form.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
            Criteria = []
        };

        // Add criteria based on form data
        if (form.NominationType.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                NominationType = form.NominationType,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (form.Continuity.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                Continuity = form.Continuity,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (form.PanelistFilter.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                PanelistFilter = form.PanelistFilter,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (form.ProjectFilter.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                ProjectFilter = form.ProjectFilter,
                ProjectId = form.ProjectFilter == AwardProjectFilter.SpecificProject ? form.ProjectId : null,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        db.Add(template);
        await db.SaveChangesAsync(cancellationToken);

        return RedirectToAction("Index");
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> EditForm([FromRoute] int id, CancellationToken cancellationToken)
    {
        var template = await db.Set<AwardTemplate>()
            .Include(t => t.Criteria!)
                .ThenInclude(c => c.Project)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
        {
            return NotFound();
        }

        var projects = await db.Set<Project>()
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var form = new AwardTemplateForm
        {
            Name = template.Name,
            Description = template.Description,
            Heading = template.Heading,
            Subheading = template.Subheading,
            Type = template.Type,
            CountMode = template.CountMode,
            IsActive = template.IsActive,
            SortOrder = template.SortOrder,
            NominationType = template.Criteria?.FirstOrDefault(c => c.NominationType.HasValue)?.NominationType,
            Continuity = template.Criteria?.FirstOrDefault(c => c.Continuity.HasValue)?.Continuity,
            PanelistFilter = template.Criteria?.FirstOrDefault(c => c.PanelistFilter.HasValue)?.PanelistFilter,
            ProjectFilter = template.Criteria?.FirstOrDefault(c => c.ProjectFilter.HasValue)?.ProjectFilter,
            ProjectId = template.Criteria?.FirstOrDefault(c => c.ProjectId.HasValue)?.ProjectId
        };

        return View(new AwardTemplateFormViewModel { Form = form, Projects = projects });
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] int id,
        [FromForm] AwardTemplateForm form,
        CancellationToken cancellationToken)
    {
        var template = await db.Set<AwardTemplate>()
            .Include(t => t.Criteria!)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var projects = await db.Set<Project>()
                .Where(p => !p.IsArchived)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            Response.StatusCode = 400;
            return View("EditForm", new AwardTemplateFormViewModel { Form = form, Projects = projects });
        }

        var now = DateTime.UtcNow;
        template.Name = form.Name;
        template.Description = form.Description;
        template.Heading = form.Heading;
        template.Subheading = form.Subheading;
        template.Type = form.Type;
        template.CountMode = form.CountMode;
        template.IsActive = form.IsActive;
        template.SortOrder = form.SortOrder;
        template.UpdatedAt = now;

        // Clear existing criteria and rebuild
        template.Criteria!.Clear();

        if (form.NominationType.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                NominationType = form.NominationType,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (form.Continuity.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                Continuity = form.Continuity,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (form.PanelistFilter.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                PanelistFilter = form.PanelistFilter,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (form.ProjectFilter.HasValue)
        {
            template.Criteria.Add(new AwardCriteria
            {
                ProjectFilter = form.ProjectFilter,
                ProjectId = form.ProjectFilter == AwardProjectFilter.SpecificProject ? form.ProjectId : null,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return RedirectToAction("Index");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var template = await db.Set<AwardTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
        {
            return NotFound();
        }

        db.Remove(template);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
