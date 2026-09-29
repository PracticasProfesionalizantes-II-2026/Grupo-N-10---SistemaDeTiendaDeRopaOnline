using System.Net.Http.Json;
using FRFront.Models;
using Microsoft.AspNetCore.Mvc;

namespace FRFront.Controllers;

public class NotificacionController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _environment;

    public NotificacionController(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Difusion()
    {
        if (!EsPersonalAutorizado())
        {
            return RedirectToAction("Index", "Home");
        }

        return View("~/Views/Shared/Difusion.cshtml", new DifusionNotificacionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Difusion(DifusionNotificacionViewModel model)
    {
        if (!EsPersonalAutorizado())
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Shared/Difusion.cshtml", model);
        }

        string? imagenUrl = null;
        if (model.Imagen is { Length: > 0 })
        {
            var extension = Path.GetExtension(model.Imagen.FileName).ToLowerInvariant();
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Imagen), "La imagen debe ser JPG, PNG o WEBP.");
                return View("~/Views/Shared/Difusion.cshtml", model);
            }

            var carpeta = Path.Combine(_environment.WebRootPath, "uploads", "notificaciones");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var ruta = Path.Combine(carpeta, nombreArchivo);
            await using var stream = System.IO.File.Create(ruta);
            await model.Imagen.CopyToAsync(stream);
            imagenUrl = $"/uploads/notificaciones/{nombreArchivo}";
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/notificaciones/difusion", new
            {
                model.Mensaje,
                ImagenUrl = imagenUrl
            });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "No se pudo enviar la difusión a los clientes.");
                return View("~/Views/Shared/Difusion.cshtml", model);
            }

            TempData["SuccessMessage"] = "La notificación fue enviada a todos los clientes activos.";
            return RedirectToAction("Difusion");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "No se pudo conectar con la API.");
            return View("~/Views/Shared/Difusion.cshtml", model);
        }
    }

    private bool EsPersonalAutorizado()
    {
        var rol = HttpContext.Session.GetString("RolSesion");
        return string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rol, "Empleado", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rol, "CAJERO", StringComparison.OrdinalIgnoreCase);
    }
}
