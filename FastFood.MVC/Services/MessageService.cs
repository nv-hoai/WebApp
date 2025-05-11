using FastFood.MVC.Data;
using FastFood.MVC.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastFood.MVC.Services
{
    public class MessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;

        public MessageService(ApplicationDbContext context, IEmailSender emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<Message>> GetAllMessagesAsync()
        {
            // Sort messages by status (UnReplied first) and then by creation date (newest first)
            return await _context.Messages
                .OrderBy(m => m.Status)
                .ThenByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<bool> CreateMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReplyToMessageAsync(int id, string reply, string repliedBy)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            message.Reply = reply;
            message.RepliedBy = repliedBy;
            message.RepliedAt = DateTime.Now;
            message.Status = MessageStatus.Replied;

            _context.Messages.Update(message);
            await _context.SaveChangesAsync();

            // Send email notification to the customer
            await _emailService.SendEmailAsync(
                message.Email,
                $"Phản hồi từ Burgz Fast Food - {message.Id}",
                $"Xin chào {message.SenderName},<br><br>" +
                $"Chúng tôi đã nhận được tin nhắn của bạn và xin phản hồi như sau:<br><br>" +
                $"<strong>Tin nhắn của bạn:</strong><br>" +
                $"{message.Content}<br><br>" +
                $"<strong>Phản hồi của chúng tôi:</strong><br>" +
                $"{message.Reply}<br><br>" +
                $"Cảm ơn bạn đã liên hệ với chúng tôi!<br>" +
                $"Đội ngũ Burgz Fast Food");

            return true;
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
