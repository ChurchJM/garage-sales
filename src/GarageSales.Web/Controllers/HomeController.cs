using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GarageSales.Web.Models;

namespace GarageSales.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        // var client = _httpClientFactory.CreateClient("GarageSalesAPI");
        // var sales = await client.GetFromJsonAsync<GarageSaleSummaryDTO>("/api/garagesales/3");
        return View();
    }

    // public IActionResult Index()
    // {
    //     return View();
    // }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
