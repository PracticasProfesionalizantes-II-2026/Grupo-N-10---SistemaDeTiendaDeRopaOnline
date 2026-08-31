using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FRFront.Models;

namespace FRFront.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly TiendaConfig _config;

        // Inyección de dependencias de la configuración global
        public AdministradorController(TiendaConfig config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ConfigurarTienda()
        {
            return View(_config);
        }

        [HttpPost]
        public IActionResult ConfigurarTienda(IFormCollection form)
        {
            if (!string.IsNullOrEmpty(form["tickerText"]))
                _config.TickerText = form["tickerText"]!;

            if (!string.IsNullOrEmpty(form["bannerText"]))
                _config.BannerText = form["bannerText"]!;

            if (!string.IsNullOrEmpty(form["phone"]))
                _config.Phone = form["phone"]!;

            if (!string.IsNullOrEmpty(form["email"]))
                _config.Email = form["email"]!;

            if (!string.IsNullOrEmpty(form["location"]))
                _config.Location = form["location"]!;

            if (!string.IsNullOrEmpty(form["social"]))
                _config.Social = form["social"]!;

            TempData["SuccessMessage"] = "¡Configuración guardada y actualizada en la tienda!";

            return RedirectToAction("ConfigurarTienda");
        }
    }
}