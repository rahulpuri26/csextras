using RoadReady.Models;
using RoadReady.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using RoadReady.Exceptions;
using Microsoft.Extensions.Logging;

namespace RoadReady.Controllers
{
    [EnableCors("Policy")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CarController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly ILogger<CarController> _logger;

        public CarController(ICarService carService, ILogger<CarController> logger)
        {
            _carService = carService;
            _logger = logger;
        }

        
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                _logger.LogInformation("Retrieving all cars.");
                return Ok(_carService.GetAllCars());
            }
            catch (CarNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to retrieve all cars.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetCarById(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving car with ID: {id}", id);
                var car = _carService.GetCarById(id);
                if (car == null)
                {
                    _logger.LogWarning($"Car with ID: {id} not found.", id);
                    return NotFound("Car not found");
                }
                return Ok(car);
            }
            catch (CarNotFoundException ex)
            {
                _logger.LogInformation($"Retrieving car with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")] // Only admins can create a car
        [HttpPost]
        public IActionResult Post(Car car)
        {
            try
            {
                _logger.LogInformation("Posting new Car.");
                var result = _carService.AddCar(car);
                return CreatedAtAction(nameof(GetCarById), new { id = result }, car);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create new car.");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public IActionResult Put(Car car)
        {
            try
            {
                _logger.LogInformation($"Updating the car");
                var result = _carService.UpdateCar(car);
                if (result == "Car not found")
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (CarNotFoundException ex)
            {
                _logger.LogError(ex, $"Failed to update car");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting car with ID: {id}", id);
                var result = _carService.DeleteCar(id);
                if (result == "Car not found")
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete car with ID: {id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
