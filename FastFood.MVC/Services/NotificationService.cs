using FastFood.MVC.Data;
using FastFood.MVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastFood.MVC.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotification(string userID, string message, string link = null, string iconClass = "fa-bell")
        {
            var notification = new Notification
            {
                UserID = userID,
                Message = message,
                Link = link,
                IconClass = iconClass,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotifications(string userID, int count = 5)
        {
            return await _context.Notifications
                .Where(n => n.UserID == userID)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCount(string userID)
        {
            return await _context.Notifications
                .Where(n => n.UserID == userID && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsRead(int notificationID)
        {
            var notification = await _context.Notifications.FindAsync(notificationID);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsRead(string userID)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserID == userID && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
