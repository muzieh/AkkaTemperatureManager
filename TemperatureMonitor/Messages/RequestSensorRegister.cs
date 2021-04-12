namespace TemperatureMonitor.Messages
{
    public class RequestSensorRegister
    {
        public int RequestId { get; }
        public string Floor { get; }
        public string SensorId { get; }

        public RequestSensorRegister(int requestId, string floor, string sensorId)
        {
            RequestId = requestId;
            Floor = floor;
            SensorId = sensorId;
        }
    }
}