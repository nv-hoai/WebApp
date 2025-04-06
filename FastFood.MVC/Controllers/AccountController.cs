using Microsoft.AspNetCore.Mvc;

namespace FastFood.MVC.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(string id)
        {
            return View();
        }

        public IActionResult Delete(string id)
        {
            return View();
        }
    }
}
