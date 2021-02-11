using Akka.Actor;

namespace TemperatureMonitor.Messages
{
    public sealed class ResponseMetadata
    {
        public int RequestId { get; }
        public string Floor { get; }
        public string SensorId { get; }
        public IActorRef SensorRef { get; }

        public ResponseMetadata(int requestId, string floor, string sensorId, IActorRef sensorRef)
        {
            RequestId = requestId;
            Floor = floor;
            SensorId = sensorId;
            SensorRef = sensorRef;
        } 
    }
}