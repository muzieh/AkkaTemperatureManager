using System.Collections.Generic;
using System.Collections.Immutable;

namespace TemperatureMonitor.Messages
{
    public sealed class RespondSensorIdsList
    {
        public int RequestId { get; }
        public IImmutableSet<string> SensorIds;

        public RespondSensorIdsList(int requestId, IImmutableSet<string> sensorIds)
        {
            RequestId = requestId;
            SensorIds = sensorIds;
        }
    }
}