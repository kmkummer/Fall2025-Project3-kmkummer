using System.Diagnostics;
using Fall2025_Project3_kmkummer.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fall2025_Project3_kmkummer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            var secretKey = _config["ChatGPT:GPT_ApiKey"] ?? throw new InvalidOperationException("ChatGPT:GPT_ApiKey does not exist in the current Configuration");
            var secretUri = _config["ChatGPT:GPT_URI"] ?? throw new InvalidOperationException("ChatGPT:GPT_ApiKey does not exist in the current Configuration");
            var secretName = _config["ChatGPT:DeploymentName"] ?? throw new InvalidOperationException("ChatGPT:DeploymentName does not exist in the current Configuration");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
