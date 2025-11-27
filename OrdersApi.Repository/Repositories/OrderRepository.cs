using Microsoft.EntityFrameworkCore;
using OrdersApi.Data.Context;
using OrdersApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersApi.Repository.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Order> CreateAsync(Order order, CancellationToken ct = default)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);
            return order;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id, ct);
        }

        public async Task<List<Order>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt).ToListAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _db.Orders.FindAsync(new object[] { id }, ct);
            if (existing != null)
            {
                _db.Orders.Remove(existing);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
