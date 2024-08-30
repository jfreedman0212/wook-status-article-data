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
        var nominators = await db.Set<Nominator>()
            .OrderBy(nominator => nominator.Name)
            .ToListAsync(cancellationToken);

        return View(new NominatorsViewModel { Nominators = nominators });
    }

    [HttpGet("new")]
    public IActionResult AddForm()
    {
        return View();
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
        var nominator = await db.Set<Nominator>().SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return NotFound();

        return View(new NominatorForm
        {
            Id = nominator.Id,
            Name = nominator.Name,
            // TODO this will need to be updated soon
            Attributes = []
        });
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

            if (nominator == null) return NotFound();

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