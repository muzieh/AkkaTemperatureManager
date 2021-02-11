namespace TemperatureMonitor.Messages
{
    public interface ITemperatureQueryReading
    {
        
    }

    public class TemperatureAvailable : ITemperatureQueryReading
    {
        public decimal Temperature { get; }

        public TemperatureAvailable(decimal temperature)
        {
            Temperature = temperature;
        }
    }

    public class NoTemperatureAvailable : ITemperatureQueryReading
    {
        public static NoTemperatureAvailable Instance { get; }= new NoTemperatureAvailable();
        private NoTemperatureAvailable() { }
    }

    public class SensorNotAvailable : ITemperatureQueryReading
    {
        public static SensorNotAvailable Instance { get; } = new SensorNotAvailable();
        private SensorNotAvailable() {}
    }

    public class SensorTimedOut : ITemperatureQueryReading
    {
        public static SensorTimedOut Instance { get; } = new SensorTimedOut();
        private SensorTimedOut() { }
    }
}