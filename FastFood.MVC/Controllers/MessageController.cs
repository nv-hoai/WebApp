using FastFood.MVC.Models;
using FastFood.MVC.Services;
using FastFood.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FastFood.MVC.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class MessageController : Controller
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _messageService.GetAllMessagesAsync();
            var viewModel = new MessageViewModel
            {
                Messages = messages
            };
            
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            
            if (message == null)
                return NotFound();

            return View(message);
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            
            if (message == null)
                return NotFound();

            var viewModel = new ReplyMessageViewModel
            {
                Id = message.Id,
                SenderName = message.SenderName,
                Email = message.Email,
                Content = message.Content
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(ReplyMessageViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Nhân viên";
            
            var result = await _messageService.ReplyToMessageAsync(
                viewModel.Id, 
                viewModel.Reply, 
                userName);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _messageService.DeleteMessageAsync(id);
            
            TempData["SuccessMessage"] = "Tin nhắn đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
