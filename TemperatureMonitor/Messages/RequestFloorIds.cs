namespace TemperatureMonitor.Messages
{
    public sealed class RequestFloorIds
    {
        public int RequestId { get; }

        public RequestFloorIds(int requestId)
        {
            RequestId = requestId;
        } 
    }
}