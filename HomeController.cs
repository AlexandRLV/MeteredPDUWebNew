using System.Diagnostics;
using MeteredPDUWebNew.Models;
using MeteredPDUWebNew.SNMP;
using Microsoft.AspNetCore.Mvc;

namespace MeteredPDUWebNew;

public class HomeController : Controller
{
    private readonly SNMPDeviceRepository _deviceRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(SNMPDeviceRepository deviceRepository, ILogger<HomeController> logger)
    {
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogDebug("Received Index page in home controller");
        return View(new LoginViewModel());
    }

    public IActionResult List()
    {
        _logger.LogDebug("Received List page in home controller");
        return View(_deviceRepository.Devices);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        _logger.LogDebug($"Received device {id} info page in home controller");
        var device = _deviceRepository.Devices[id];
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (device == null)
            return NotFound();

        return View(device);
    }

    [HttpPost]
    public IActionResult CreateDevice(CreateDeviceViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("List");

        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.IpAddress) || model.Port == 0)
            return RedirectToAction("List");

        if (!System.Net.IPAddress.TryParse(model.IpAddress, out _))
            return RedirectToAction("List");

        _deviceRepository.AddDevice(model);
        return RedirectToAction("List");
    }

    [HttpGet]
    public IActionResult UpdateState(int id)
    {
        var device = _deviceRepository.Devices[id];
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (device == null)
            return NotFound();

        if (device.OnlineStatus == SNMP.OnlineStatus.Online)
        {
            device.OnlineStatus = SNMP.OnlineStatus.Offline;
            device.SetToZero();
        }
        else
        {
            device.OnlineStatus = SNMP.OnlineStatus.Online;
            device.UpdateValues();
        }

        return RedirectToAction("Details", new { id = id });
    }

    [HttpGet]
    public IActionResult DeleteDevice(int id)
    {
        _deviceRepository.DeleteDevice(id);
        return RedirectToAction("List");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.Email == "admin@example.com" && model.Password == "8l5mdxrr")
            {
                return RedirectToAction("List");
            }

            ModelState.AddModelError("", "Неправильный логин и (или) пароль");
        }
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}