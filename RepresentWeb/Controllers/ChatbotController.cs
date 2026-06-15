using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepresentWeb.Data;
using RepresentWeb.Models;

namespace RepresentWeb.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatbotController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Status([FromBody] ChatbotOrderStatusRequest request)
        {
            if (!TryParseOrderNumber(request?.OrderNumber, out var orderId))
            {
                return Json(new { success = false, message = "Please enter a valid order number, for example 12." });
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return Json(new { success = false, message = "We could not find an order with that number." });
            }

            return Json(new
            {
                success = true,
                message = $"Order #{order.Id} is currently {order.Status}.",
                order = new
                {
                    order.Id,
                    order.Status,
                    order.OrderDate,
                    order.TotalAmount,
                    itemCount = order.Items.Count
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel([FromBody] ChatbotCancelOrderRequest request)
        {
            if (!TryParseOrderNumber(request?.OrderNumber, out var orderId))
            {
                return Json(new { success = false, message = "Please enter a valid order number." });
            }

            if (string.IsNullOrWhiteSpace(request?.Reason))
            {
                return Json(new { success = false, message = "Please provide a cancellation reason." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "We could not find an order with that number." });
            }

            if (order.Status == "Cancelled")
            {
                return Json(new { success = false, message = "This order is already cancelled." });
            }

            if (order.Status == "Delivered")
            {
                return Json(new { success = false, message = "Delivered orders cannot be cancelled through chatbot." });
            }

            var supportRequest = new SupportRequest
            {
                RequestType = "Cancellation",
                OrderId = order.Id,
                CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Guest" : request.CustomerName,
                CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? "Guest" : request.CustomerEmail,
                Reason = request.Reason,
                Message = $"Cancellation requested for order #{order.Id}. Reason: {request.Reason}",
                Status = "New",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.SupportRequests.Add(supportRequest);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cancellation request sent to admin. Admin will review it and cancel the order if eligible.",
                requestId = supportRequest.Id
            });
        }

        [HttpPost]
        public async Task<IActionResult> NotifyAdmin([FromBody] ChatbotAdminChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return Json(new { success = false, message = "Please type a message for admin." });
            }

            var supportRequest = new SupportRequest
            {
                RequestType = "AdminChat",
                OrderId = TryParseOrderNumber(request.OrderNumber, out var orderId) ? orderId : null,
                CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Guest" : request.CustomerName,
                CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? "Guest" : request.CustomerEmail,
                Reason = string.Empty,
                Message = request.Message,
                Status = "New",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.SupportRequests.Add(supportRequest);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Admin has been notified. They will respond from the admin panel.",
                requestId = supportRequest.Id
            });
        }

        private static bool TryParseOrderNumber(string? value, out int orderId)
        {
            orderId = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim();
            if (normalized.StartsWith("#", StringComparison.Ordinal))
            {
                normalized = normalized[1..];
            }

            return int.TryParse(normalized, out orderId);
        }
    }

    public class ChatbotOrderStatusRequest
    {
        public string? OrderNumber { get; set; }
    }

    public class ChatbotCancelOrderRequest
    {
        public string? OrderNumber { get; set; }
        public string? Reason { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }

    public class ChatbotAdminChatRequest
    {
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Message { get; set; }
    }
}
