using RoadReady.Models;
using RoadReady.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using RoadReady.Exceptions;
using RoadReady.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RoadReady.Controllers
{
    [EnableCors("Policy")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ReservationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(ApplicationDbContext context, IReservationService reservationService, ILogger<ReservationController> logger)
        {
            _context = context;
            _reservationService = reservationService;
            _logger = logger;
        }

      
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var reservations = _reservationService.GetAllReservations();
                return Ok(reservations);
            }
            catch (ReservationNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to retrieve all Reservations.");
                return StatusCode(500, "Internal server error");
            }
        }

       
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public IActionResult GetReservationById(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving Reservation with ID: {id}", id);
                var reservation = _reservationService.GetReservationById(id);
                if (reservation == null)
                    return NotFound("Reservation not found");

                return Ok(reservation);
            }
            catch (ReservationNotFoundException ex)
            {
                _logger.LogError(ex, $"Failed to retrieve Reservation with ID: {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("car/{carId}")]
        public async Task<IActionResult> GetReservationsByCarId(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return NotFound(new { message = "Car not found." });
            }

            var reservations = await _context.Reservations
                                              .Where(r => r.CarId == carId)
                                              .Include(r => r.Car)
                                              .ToListAsync();

            if (reservations == null || reservations.Count == 0)
            {
                return NotFound(new { message = "No reservations found for this car." });
            }

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(reservations, options);
            return Content(json, "application/json");
        }

        [HttpPost]
        public IActionResult Post(Reservation reservation)
        {
            try
            {
                _logger.LogInformation("Creating new reservation.");
                var result = _reservationService.AddReservation(reservation);
                return CreatedAtAction(nameof(GetReservationById), new { id = result }, reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add a Reservation.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public IActionResult Put(Reservation reservation)
        {
            try
            {
                _logger.LogInformation("Updating reservation");

                var result = _reservationService.UpdateReservation(reservation);
                if (result == null)
                {
                    _logger.LogWarning("Reservation not found.");
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (ReservationNotFoundException)
            {
                _logger.LogWarning($"Reservation not found.");
                return NotFound("Reservation not found.");
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to update reservation");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting reservation with ID: {id}");
                var result = _reservationService.DeleteReservation(id);
                if (result == null)
                {
                    _logger.LogWarning($"Reservation with ID: {id} not found.");
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to delete reservation with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
