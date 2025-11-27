using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Data.Models;
using OrdersApi.Repository.Repositories;
using OrdersApi.Services.Services;

namespace OrdersApi.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<OrdersController> _logger;
        private static readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(5);

        public OrdersController(IOrderRepository repo, ICacheService cache, ILogger<OrdersController> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        // POST /orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                order.OrderId = Guid.NewGuid();
                order.CreatedAt = DateTime.UtcNow;

                var created = await _repo.CreateAsync(order);
                await _cache.SetAsync($"order:{created.OrderId}", created, CacheTTL);

                return CreatedAtAction(nameof(GetOrder), new { id = created.OrderId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // GET /orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            try
            {
                var cached = await _cache.GetAsync<Order>($"order:{id}");
                if (cached != null) return Ok(cached);

                var order = await _repo.GetByIdAsync(id);
                if (order == null) return NotFound();

                await _cache.SetAsync($"order:{id}", order, CacheTTL);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        // GET /orders
        [HttpGet]
        public async Task<IActionResult> ListOrders()
        {
            try
            {
                var orders = await _repo.ListAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing orders");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // DELETE /orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                await _repo.DeleteAsync(id);
                await _cache.RemoveAsync($"order:{id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }
    
}
}
