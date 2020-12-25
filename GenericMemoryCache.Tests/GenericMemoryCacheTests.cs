using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using SomeAcme.SomeUtilNamespace;
using System;

namespace GenericMemoryCache.Tests
{

    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public class GenericMemoryCacheTests
    {

        private static GenericMemoryCache<Car> _carsCache;

       
        [OneTimeSetUp]
        public void TestInitialize()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            _carsCache = new GenericMemoryCache<Car>(memoryCache, "CARS", 0);

        }


        [Test]
        [TestCase("AUDI_A4", "Audi", "A4")]
        public void Add_Get_Remove_Does_Not_Fail(string cacheKey, string make, string model)
        {
            _carsCache.AddItem(cacheKey, new Car { Make = make, Model = model, NumberOfWheels = 4 });
            var existingCacheItem = _carsCache.GetItem(cacheKey);
            existingCacheItem.Should().NotBeNull();
            existingCacheItem.Make.Should().Be(make);
            existingCacheItem.Model.Should().Be(model);
            _carsCache.ClearAll();
            existingCacheItem = _carsCache.GetItem(cacheKey);
            Assert.IsNull(existingCacheItem);
        }

        [Test]
        [TestCase("Porsche_911", "Porsche", "911", "911 GT")]
        [TestCase("Porsche_911_Carrera", "Porsche", "911 Carrera", "911 Carrera 4S")]
        [TestCase("Porsche_Panamera_4E", "Porsche", "4 E-Hybrid", "4 E-Hybrid Elite")]
        public void Add_Get_Update_Remove_Does_Not_Fail(string cacheKey, string make, string model, string updateModel)
        {
            _carsCache.AddItem(cacheKey, new Car { Make = make, Model = model, NumberOfWheels = 4 });
            var existingCacheItem = _carsCache.GetItem(cacheKey);
            existingCacheItem.Should().NotBeNull();
            existingCacheItem.Make.Should().Be(make);
            existingCacheItem.Model.Should().Be(model);
            existingCacheItem.Model = updateModel;
            _carsCache.UpdateItem(cacheKey, existingCacheItem);
            var existingCacheItemUpdated = _carsCache.GetItem(cacheKey);
            existingCacheItemUpdated.Model.Should().Be(updateModel);
            _carsCache.ClearAll();
            existingCacheItem = _carsCache.GetItem(cacheKey);
            Assert.IsNull(existingCacheItem);
        }

        [Test]
        [TestCase("Porsche_911", "Porsche", "911")]
        public void Get_Values_Returns_Expected(string cacheKey, string make, string model)
        {
            for (int i = 0; i < 10; i++)
            {
                _carsCache.AddItem($"{cacheKey}_{i}",
                 new Car { Make = make, Model = model, NumberOfWheels = 4 });
            }


            var existingCars = _carsCache.GetValues<string>();
            existingCars.Count.Should().Be(10);
        }





        public class Car
        {
            public Car()
            {
                NumberOfWheels = 4; 
            }

            public string Make { get; set; }
            public string Model { get; set; }
            public int NumberOfWheels { get; set; }
        }
    }
}
