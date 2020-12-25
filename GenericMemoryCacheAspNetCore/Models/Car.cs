namespace GenericMemoryCacheAspNetCore.Models
{

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
