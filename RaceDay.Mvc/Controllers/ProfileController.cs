
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class ProfileController : Controller
{
    private readonly RaceDayApiClient _api;

    public ProfileController(RaceDayApiClient api)
    {
        _api = api;
    }

    private IActionResult? RequireLogin()
    {
        if (string.IsNullOrWhiteSpace(
            HttpContext.Session.GetString("UserID")))
        {
            return RedirectToAction("Login", "Auth");
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var access = RequireLogin();

        if (access != null)
            return access;

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/profile");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not retrieve your profile.";
            return RedirectToAction("Index", "Home");
        }

        var profile = await _api.ReadAsync<ProfileViewModel>(response);

        if (profile == null)
        {
            TempData["Error"] = "Your profile could not be loaded.";
            return RedirectToAction("Index", "Home");
        }

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var access = RequireLogin();

        if (access != null)
            return access;

        if (!ModelState.IsValid)
            return View(model);

        var request = new
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email
        };

        var response = await _api.SendAsync(
            HttpMethod.Put,
            "api/profile",
            request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    "Email",
                    "That email address is already being used.");
            }
            else
            {
                ModelState.AddModelError(
                    "",
                    $"Profile update failed: {error}");
            }

            return View(model);
        }

        HttpContext.Session.SetString("Email", model.Email);
        HttpContext.Session.SetString(
            "DisplayName",
            $"{model.FirstName} {model.LastName}".Trim());

        TempData["Success"] = "Your profile has been updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}
