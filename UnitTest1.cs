using NUnit.Framework;
using Moq;
using RoadReady.Controllers;
using RoadReady.Models;
using RoadReady.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace RoadReady.Test
{
    [TestFixture]
    public class CarsControllerTests
    {
        private Mock<ICarService> _mockCarService;
        private CarController _controller;

        [SetUp]
        public void Setup()
        {
            _mockCarService = new Mock<ICarService>();
            _controller = new CarController(_mockCarService.Object);
        }

        [Test]
        public void GetAllCars_ReturnsOkResult_WithListOfCars()
        {
            // Arrange
            var cars = new List<Car> {
                new Car { CarId = 1, Model = "Model S", Make = "Tesla", Year = 2020 },
                new Car { CarId = 2, Model = "Mustang", Make = "Ford", Year = 2021 }
            };
            _mockCarService.Setup(service => service.GetAllCars()).Returns(cars);

            // Act
            var result = _controller.GetAll();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(cars, okResult?.Value);
        }

        [Test]
        public void GetCarById_CarExists_ReturnsOkResult_WithCar()
        {
            // Arrange
            var carId = 1;
            var car = new Car { CarId = carId, Model = "Model S", Make = "Tesla", Year = 2020 };
            _mockCarService.Setup(service => service.GetCarById(carId)).Returns(car);

            // Act
            var result = _controller.GetCarById(carId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(car, okResult?.Value);
        }

        [Test]
        public void AddCar_ValidCar_ReturnsCreatedAtAction()
        {
            // Arrange
            var newCar = new Car { Model = "Civic", Make = "Honda", Year = 2022 };
            _mockCarService.Setup(service => service.AddCar(newCar)).Returns(newCar.CarId);

            // Act
            var result = _controller.Post(newCar);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.AreEqual(newCar.CarId, createdResult?.RouteValues["id"]);
            Assert.AreEqual(newCar, createdResult?.Value);
        }

        [Test]
        public void UpdateCar_ValidIdAndCar_ReturnsOkResult()
        {
            // Arrange
            var carId = 1;
            var updatedCar = new Car { CarId = carId, Model = "Model X", Make = "Tesla", Year = 2022 };
            _mockCarService.Setup(service => service.UpdateCar(updatedCar)).Returns("Car updated successfully");

            // Act
            var result = _controller.Put(updatedCar);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Car updated successfully", okResult?.Value?.ToString());
        }

        [Test]
        public void DeleteCar_ValidId_ReturnsOkResult()
        {
            // Arrange
            var carId = 1;
            _mockCarService.Setup(service => service.DeleteCar(carId)).Returns("Car deleted successfully");

            // Act
            var result = _controller.Delete(carId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Car deleted successfully", okResult?.Value?.ToString());
        }
    }
}
