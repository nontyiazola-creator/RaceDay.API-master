
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels.Participant;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    private IActionResult RequireParticipant()
    {
        if (HttpContext.Session.GetInt32("UserID") == null)
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Index", "Home");

        return null!;
    }

    public async Task<IActionResult> Index()
    {
        var access = RequireParticipant();
        if (access != null)
            return access;

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/events");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not load events.";
            return View(new List<ParticipantEventViewModel>());
        }

        var events = await _api.ReadAsync<List<ParticipantEventViewModel>>(response)
                     ?? new List<ParticipantEventViewModel>();

        var upcomingEvents = events
            .Where(e => e.EventDate.Date >= DateTime.Today)
            .OrderBy(e => e.EventDate)
            .ToList();

        return View(upcomingEvents);
    }

    public async Task<IActionResult> EventDetails(int id)
    {
        var access = RequireParticipant();
        if (access != null)
            return access;

        var eventResponse = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}");

        if (!eventResponse.IsSuccessStatusCode)
            return NotFound();

        var eventDetails =
            await _api.ReadAsync<ParticipantEventViewModel>(eventResponse);

        if (eventDetails == null)
            return NotFound();

        var categoryResponse = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}/categories");

        var categories = await _api.ReadAsync<List<ParticipantCategoryViewModel>>(
            categoryResponse) ?? new List<ParticipantCategoryViewModel>();

        var model = new ParticipantEventDetailsViewModel
        {
            Event = eventDetails,
            Categories = categories
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var access = RequireParticipant();
        if (access != null)
            return access;

        var response = await _api.SendAsync(
            HttpMethod.Post,
            "api/enrolments",
            new
            {
                EventID = eventId,
                CategoryID = categoryId
            });

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Enrolment failed. You may already be enrolled in this category.";
            return RedirectToAction(nameof(EventDetails), new { id = eventId });
        }

        TempData["Success"] = "You have successfully enrolled!";
        return RedirectToAction(nameof(MyEvents));
    }

    public async Task<IActionResult> MyEvents()
    {
        var access = RequireParticipant();
        if (access != null)
            return access;

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/enrolments/mine");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not load your enrolments.";
            return View(new List<ParticipantEnrolmentViewModel>());
        }

        var enrolments =
            await _api.ReadAsync<List<ParticipantEnrolmentViewModel>>(response)
            ?? new List<ParticipantEnrolmentViewModel>();

        return View(enrolments);
    }

    public async Task<IActionResult> MyResults()
    {
        var access = RequireParticipant();
        if (access != null)
            return access;

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/results/mine");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not load your results.";
            return View(new List<ParticipantResultViewModel>());
        }

        var results =
            await _api.ReadAsync<List<ParticipantResultViewModel>>(response)
            ?? new List<ParticipantResultViewModel>();

        return View(results);
    }
}
