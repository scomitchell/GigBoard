using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GigBoardBackend.Models;
using GigBoardBackend.Services;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveryController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddDelivery([FromBody] Delivery delivery)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                var result = await _deliveryService.AddDeliveryAsync(userId.Value, delivery);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-deliveries")]
        public async Task<IActionResult> GetDeliveries()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var deliveries = await _deliveryService.GetDeliveriesAsync(userId.Value);
            return Ok(deliveries);
        }

        [HttpGet("unassigned-deliveries")]
        public async Task<IActionResult> GetUnassignedDeliveries()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var deliveries = await _deliveryService.GetUnassignedDeliveriesAsync(userId.Value);
            return Ok(deliveries);
        }

        [HttpGet("filtered-deliveries")]
        public async Task<IActionResult> GetDeliveriesByApp(
            [FromQuery] DeliveryApp? app, [FromQuery] double? basePay, [FromQuery] double? tipPay,
            [FromQuery] double? totalPay, [FromQuery] double? mileage, [FromQuery] string? restaurant,
            [FromQuery] string? customerNeighborhood)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var deliveries = await _deliveryService.GetFilteredDeliveriesAsync(userId.Value, app, basePay, tipPay, totalPay, mileage, restaurant, customerNeighborhood);
            return Ok(deliveries);
        }

        [HttpGet("delivery-neighborhoods")]
        public async Task<IActionResult> GetUserDeliveryNeighborhoods()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var neighborhoods = await _deliveryService.GetUserDeliveryNeighborhoodsAsync(userId.Value);
            return Ok(neighborhoods);
        }

        [HttpGet("delivery-apps")]
        public async Task<IActionResult> GetUserDeliveryApps()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var apps = await _deliveryService.GetUserDeliveryAppsAsync(userId.Value);
            return Ok(apps);
        }

        [HttpGet("{deliveryId:int}")]
        public async Task<IActionResult> GetDeliveryById(int deliveryId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var delivery = await _deliveryService.GetDeliveryByIdAsync(userId.Value, deliveryId);
            if (delivery == null) return NotFound("Delivery not found");

            return Ok(delivery);
        }

        [HttpDelete("{deliveryId}")]
        public async Task<IActionResult> DeleteDelivery(int deliveryId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                await _deliveryService.DeleteDeliveryAsync(userId.Value, deliveryId);
                return Ok("Delivery removed");
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDelivery([FromBody] Delivery delivery)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                var result = await _deliveryService.UpdateDeliveryAsync(userId.Value, delivery);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            return null;
        }
    }
}