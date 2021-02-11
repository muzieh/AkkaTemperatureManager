namespace TemperatureMonitor.Messages
{
    public sealed class RequestSensorIdsList
    {
        public int RequestId { get; }
        public string FloorId { get; }

        public RequestSensorIdsList(int requestId, string floorId)
        {
            RequestId = requestId;
            FloorId = floorId;
        }
    }
}