using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageSales.Web.Controllers;

[Authorize]
public class MySalesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}