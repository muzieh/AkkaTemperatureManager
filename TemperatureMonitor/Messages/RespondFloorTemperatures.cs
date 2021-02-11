using System.Collections.Generic;
using System.Collections.Immutable;

namespace TemperatureMonitor.Messages
{
    public class RespondFloorTemperatures
    {
        public int RequestId { get; }
        public IImmutableDictionary<string, ITemperatureQueryReading> TemperatureReadings { get; }

        public RespondFloorTemperatures(int requestId, IImmutableDictionary<string, ITemperatureQueryReading> temperatureReadings)
        {
            RequestId = requestId;
            TemperatureReadings = temperatureReadings;
        }    
    }
}