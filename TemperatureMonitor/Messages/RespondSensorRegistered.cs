using Akka.Actor;

namespace TemperatureMonitor.Messages
{
    public sealed class RespondSensorRegistered
    {
        public int RequestId { get; }
        public IActorRef SensorRef { get; }

        public RespondSensorRegistered(int requestId, IActorRef sensorRef)
        {
            RequestId = requestId;
            SensorRef = sensorRef;
        }
    }
}