using Microsoft.EntityFrameworkCore;
using BookStoreMVC.Data;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;

namespace BookStoreMVC.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string userId, OrderCreateViewModel model);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByIdAsync(int orderId, string userId);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus status);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task<string> GenerateOrderNumberAsync();
        Task<(int TotalOrders, decimal TotalRevenue, decimal AverageOrderValue)> GetOrderStatisticsAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            ICartService cartService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<Order?> CreateOrderAsync(string userId, OrderCreateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get cart items
                var cart = await _cartService.GetCartAsync(userId);
                if (cart.IsEmpty)
                {
                    return null;
                }

                // Validate stock for all items
                foreach (var item in cart.Items)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null || book.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for book: {item.Book.Title}");
                    }
                }

                // Generate order number
                var orderNumber = await GenerateOrderNumberAsync();

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,

                    SubTotal = cart.SubTotal,
                    ShippingCost = cart.ShippingCost,
                    Tax = cart.Tax,
                    Discount = cart.PromoDiscount,
                    Total = cart.Total,

                    ShippingFirstName = model.ShippingFirstName,
                    ShippingLastName = model.ShippingLastName,
                    ShippingAddress = model.ShippingAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingPostalCode = model.ShippingPostalCode,
                    ShippingCountry = model.ShippingCountry,
                    ShippingPhone = model.ShippingPhone,
                    Notes = model.Notes,

                    OrderDate = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items and update stock
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        BookId = cartItem.BookId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Book.FinalPrice,
                        Total = cartItem.TotalPrice,
                        BookTitle = cartItem.Book.Title,
                        BookAuthor = cartItem.Book.Author,
                        BookImageUrl = cartItem.Book.ImageUrl
                    };

                    orderItems.Add(orderItem);

                    // Update stock
                    var book = await _context.Books.FindAsync(cartItem.BookId);
                    if (book != null)
                    {
                        book.StockQuantity -= cartItem.Quantity;
                        book.UpdatedAt = DateTime.UtcNow;
                    }
                }

                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();

                // Clear cart
                await _cartService.ClearCartAsync(userId);

                await transaction.CommitAsync();

                _logger.LogInformation("Order created successfully: {OrderNumber} for user {UserId}",
                    orderNumber, userId);

                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return false;
                }

                order.Status = status;

                // Update relevant dates based on status
                switch (status)
                {
                    case OrderStatus.Shipped:
                        order.ShippedDate = DateTime.UtcNow;
                        break;
                    case OrderStatus.Delivered:
                        order.DeliveredDate = DateTime.UtcNow;
                        if (!order.ShippedDate.HasValue)
                        {
                            order.ShippedDate = DateTime.UtcNow;
                        }
                        break;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return false;
                }

                order.PaymentStatus = status;

                // If payment is successful, update order status to processing
                if (status == PaymentStatus.Paid && order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Processing;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} payment status updated to {PaymentStatus}",
                    orderId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Book)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null || order.Status != OrderStatus.Pending)
                {
                    return false;
                }

                // Restore stock for all order items
                foreach (var orderItem in order.OrderItems)
                {
                    if (orderItem.Book != null)
                    {
                        orderItem.Book.StockQuantity += orderItem.Quantity;
                        orderItem.Book.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Update order status
                order.Status = OrderStatus.Cancelled;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, userId);
                return false;
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.UtcNow;
            var dateString = date.ToString("yyyyMMdd");

            // Get the count of orders for today
            var ordersToday = await _context.Orders
                .Where(o => o.OrderDate.Date == date.Date)
                .CountAsync();

            var sequenceNumber = (ordersToday + 1).ToString("D4");

            return $"ORD-{dateString}-{sequenceNumber}";
        }

        public async Task<(int TotalOrders, decimal TotalRevenue, decimal AverageOrderValue)> GetOrderStatisticsAsync()
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .ToListAsync();

                var totalOrders = orders.Count;
                var totalRevenue = orders.Sum(o => o.Total);
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                return (totalOrders, totalRevenue, averageOrderValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return (0, 0, 0);
            }
        }
    }
}