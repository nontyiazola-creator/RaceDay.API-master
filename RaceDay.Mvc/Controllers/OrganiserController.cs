
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels.Organiser;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser"
            && HttpContext.Session.GetInt32("UserID") != null;
    }

    private IActionResult RequireOrganiser()
    {
        if (HttpContext.Session.GetInt32("UserID") == null)
            return RedirectToAction("Login", "Auth");

        return RedirectToAction("Index", "Home");
    }

    private async Task<string> GetError(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();

        try
        {
            using var document = JsonDocument.Parse(content);

            if (document.RootElement.TryGetProperty("message", out var message))
                return message.GetString() ?? "The request failed.";
        }
        catch
        {
            // The API may return plain text instead of JSON.
        }

        return string.IsNullOrWhiteSpace(content)
            ? $"The API returned {(int)response.StatusCode}."
            : content;
    }

    // Organiser dashboard

    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var response = await _api.SendAsync(
            HttpMethod.Get, "api/events");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(response);
            return View(new List<EventViewModel>());
        }

        var events = await _api.ReadAsync<List<EventViewModel>>(response)
                     ?? new List<EventViewModel>();

        int organiserId = HttpContext.Session.GetInt32("UserID")!.Value;

        return View(events.Where(e =>
            e.OrganiserID == organiserId).ToList());
    }

    // Event creation

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        return View(new EventViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventViewModel model)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        if (!ModelState.IsValid)
            return View(model);

        using var response = await _api.SendAsync(
            HttpMethod.Post, "api/events",
            new
            {
                model.EventName,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.EventType
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, await GetError(response));
            return View(model);
        }

        TempData["Success"] = "Event created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // Event editing

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var response = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(response);
            return RedirectToAction(nameof(Index));
        }

        var model = await _api.ReadAsync<EventViewModel>(response);

        return model == null
            ? RedirectToAction(nameof(Index))
            : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, EventViewModel model)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        if (!ModelState.IsValid)
            return View(model);

        using var response = await _api.SendAsync(
            HttpMethod.Put, $"api/events/{id}",
            new
            {
                model.EventName,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.EventType
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, await GetError(response));
            return View(model);
        }

        TempData["Success"] = "Event updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var response = await _api.SendAsync(
            HttpMethod.Delete, $"api/events/{id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(response);
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Event deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // Categories

    public async Task<IActionResult> Categories(int id)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var eventResponse = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}");

        if (!eventResponse.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(eventResponse);
            return RedirectToAction(nameof(Index));
        }

        var eventModel = await _api.ReadAsync<EventViewModel>(eventResponse);

        using var response = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}/categories");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(response);
            return RedirectToAction(nameof(Index));
        }

        var categories = await _api.ReadAsync<List<CategoryViewModel>>(response)
                         ?? new List<CategoryViewModel>();

        ViewBag.Event = eventModel;

        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(CategoryViewModel model)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a category name.";
            return RedirectToAction(nameof(Categories),
                new { id = model.EventID });
        }

        using var response = await _api.SendAsync(
            HttpMethod.Post,
            $"api/events/{model.EventID}/categories",
            new { model.CategoryName, model.Description });

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = await GetError(response);
        else
            TempData["Success"] = "Category added successfully.";

        return RedirectToAction(nameof(Categories),
            new { id = model.EventID });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryViewModel model)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var response = await _api.SendAsync(
            HttpMethod.Put,
            $"api/categories/{model.CategoryID}",
            new { model.CategoryName, model.Description });

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = await GetError(response);
        else
            TempData["Success"] = "Category updated successfully.";

        return RedirectToAction(nameof(Categories),
            new { id = model.EventID });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var response = await _api.SendAsync(
            HttpMethod.Delete, $"api/categories/{id}");

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = await GetError(response);
        else
            TempData["Success"] = "Category deleted successfully.";

        return RedirectToAction(nameof(Categories),
            new { id = eventId });
    }

    // Enrolments

    public async Task<IActionResult> Enrolments(int id)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        using var response = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}/enrolments");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(response);
            return RedirectToAction(nameof(Index));
        }

        var enrolments =
            await _api.ReadAsync<List<EnrolmentViewModel>>(response)
            ?? new List<EnrolmentViewModel>();

        ViewBag.EventID = id;

        return View(enrolments);
    }

    // Results

    public async Task<IActionResult> Results(int id)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        // Retrieve every enrolment for this event.
        using var enrolmentResponse = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}/enrolments");

        if (!enrolmentResponse.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(enrolmentResponse);
            return RedirectToAction(nameof(Index));
        }

        var enrolments =
            await _api.ReadAsync<List<EnrolmentViewModel>>(enrolmentResponse)
            ?? new List<EnrolmentViewModel>();

        // Retrieve results that have already been recorded.
        using var resultResponse = await _api.SendAsync(
            HttpMethod.Get, $"api/events/{id}/results");

        if (!resultResponse.IsSuccessStatusCode)
        {
            TempData["Error"] = await GetError(resultResponse);
            return RedirectToAction(nameof(Index));
        }

        var recordedResults =
            await _api.ReadAsync<List<ResultViewModel>>(resultResponse)
            ?? new List<ResultViewModel>();

        // Match existing results to enrolments.
        var resultsByEnrolment = recordedResults
            .GroupBy(r => r.EnrolmentID)
            .ToDictionary(g => g.Key, g => g.First());

        // Include every enrolled participant, even without a result.
        var combinedResults = enrolments.Select(enrolment =>
        {
            resultsByEnrolment.TryGetValue(
                enrolment.EnrolmentID, out var existingResult);

            return new ResultViewModel
            {
                ResultID = existingResult?.ResultID ?? 0,
                EnrolmentID = enrolment.EnrolmentID,
                ParticipantName = enrolment.ParticipantName,
                CategoryName = enrolment.CategoryName,
                FinishTime = existingResult?.FinishTime,
                FinishPosition = existingResult?.FinishPosition,
                RecordedAt = existingResult?.RecordedAt ?? default
            };
        }).ToList();

        ViewBag.EventID = id;

        return View(combinedResults);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(
        CaptureResultViewModel model, int eventId)
    {
        if (!IsOrganiser())
            return RequireOrganiser();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please enter a valid result.";
            return RedirectToAction(nameof(Results),
                new { id = eventId });
        }

        TimeSpan? finishTime = null;

        if (!string.IsNullOrWhiteSpace(model.FinishTime))
        {
            if (!TimeSpan.TryParse(model.FinishTime, out var parsedTime)
                || parsedTime <= TimeSpan.Zero)
            {
                TempData["Error"] =
                    "Enter a valid finish time, for example 00:45:30.";

                return RedirectToAction(nameof(Results),
                    new { id = eventId });
            }

            finishTime = parsedTime;
        }

        if (finishTime == null && model.FinishPosition == null)
        {
            TempData["Error"] =
                "Enter a finish time, a position, or both.";

            return RedirectToAction(nameof(Results),
                new { id = eventId });
        }

        using var response = await _api.SendAsync(
            HttpMethod.Put,
            $"api/enrolments/{model.EnrolmentID}/result",
            new
            {
                FinishTime = finishTime,
                FinishPosition = model.FinishPosition
            });

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = await GetError(response);
        else
            TempData["Success"] = "Race result saved successfully.";

        return RedirectToAction(nameof(Results),
            new { id = eventId });
    }
}
