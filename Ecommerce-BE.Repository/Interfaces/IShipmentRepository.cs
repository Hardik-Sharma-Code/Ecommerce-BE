using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IShipmentRepository
{
    Task<Shipment?> GetByOrderIdAsync(int orderId);
    Task<Shipment> CreateAsync(Shipment shipment);
    Task<Shipment> UpdateAsync(Shipment shipment);
}
