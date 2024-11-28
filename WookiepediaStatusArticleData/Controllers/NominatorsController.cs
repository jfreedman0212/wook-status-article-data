using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Nominators;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("nominators")]
public class NominatorsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var nominators = await db.Set<Nominator>()
            .Include(nominator => nominator.Attributes!
                .Where(attr => attr.EffectiveEndAt == null || attr.EffectiveEndAt >= now)
            )
            .OrderBy(nominator => nominator.Name)
            .ToListAsync(cancellationToken);

        return View(new NominatorsViewModel { Nominators = nominators });
    }

    [HttpGet("new-attribute")]
    public IActionResult NewAttribute([FromQuery] NominatorForm form)
    {
        form.Attributes.Add(
            new NominatorAttributeViewModel
            {
                AttributeName = NominatorAttributeType.AcMember,
                EffectiveAt = DateOnly.FromDateTime(DateTime.UtcNow),
            }
        );

        return PartialView("_AttributesList", form);
    }

    [HttpGet("delete-attribute/{index:int}")]
    public IActionResult RemoveAttribute(
        [FromQuery] NominatorForm form,
        [FromRoute] int index
    )
    {
        // we _could_ just delete the element on the UI and avoid another network call.
        // however, the names have the index in them. removing them wouldn't fix the subsequent
        // rows. to make it easier, just re-render the whole attribute list and send back
        form.Attributes.RemoveAt(index);
        return PartialView("_AttributesList", form);
    }

    [HttpGet("new")]
    public IActionResult AddForm()
    {
        return View(new NominatorForm { Name = "", Attributes = [] });
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromForm] NominatorForm form,
        [FromServices] EditNominatorAction action,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("AddForm", form);
        }

        try
        {
            await action.ExecuteAsync(null, form, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            return RedirectToAction("Index");
        }
        catch (ValidationException validationException)
        {
            foreach (var issue in validationException.Issues)
            {
                ModelState.AddModelError(issue.Name, issue.Message);
            }

            Response.StatusCode = 400;
            return View("AddForm", form);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> EditForm(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes)
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null)
            return NotFound();

        return View(
            new NominatorForm
            {
                Id = nominator.Id,
                Name = nominator.Name,
                Attributes = nominator.Attributes?
                    .Select(it => new NominatorAttributeViewModel
                    {
                        Id = it.Id,
                        AttributeName = it.AttributeName,
                        EffectiveAt = DateOnly.FromDateTime(it.EffectiveAt),
                        EffectiveUntil = it.EffectiveEndAt != null
                            ? DateOnly.FromDateTime(it.EffectiveEndAt.Value)
                            : null
                    })
                    .ToList() ?? [],
            }
        );
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] NominatorForm form,
        [FromServices] EditNominatorAction action,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("EditForm", form);
        }

        try
        {
            var nominator = await action.ExecuteAsync(id, form, cancellationToken);

            if (nominator == null)
                return NotFound();

            await db.SaveChangesAsync(cancellationToken);
            return RedirectToAction("Index");
        }
        catch (ValidationException validationException)
        {
            foreach (var issue in validationException.Issues)
            {
                ModelState.AddModelError(issue.Name, issue.Message);
            }

            Response.StatusCode = 400;
            return View("EditForm", form);
        }
    }
}