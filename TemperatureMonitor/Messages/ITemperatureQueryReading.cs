namespace TemperatureMonitor.Messages
{
    public interface ITemperatureQueryReading
    {
        
    }

    public class TemperatureQueryReading : ITemperatureQueryReading
    {
        public decimal Temperature { get; }

        public TemperatureQueryReading(decimal temperature)
        {
            Temperature = temperature;
        }
    }

    public class NoTemperatureQueryAvailable : ITemperatureQueryReading
    {
        public static NoTemperatureQueryAvailable Instance { get; }= new NoTemperatureQueryAvailable();
        private NoTemperatureQueryAvailable() { }
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