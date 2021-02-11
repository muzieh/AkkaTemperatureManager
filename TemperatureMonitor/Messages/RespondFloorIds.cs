using System.Collections.Immutable;

namespace TemperatureMonitor.Messages
{
    public sealed class RespondFloorIds
    {
        public RespondFloorIds(int requestId, IImmutableSet<string> floorIds)
        {
            RequestId = requestId;
            FloorIds = floorIds;
        }

        public int RequestId { get; }
        public IImmutableSet<string> FloorIds { get; }
    }
}