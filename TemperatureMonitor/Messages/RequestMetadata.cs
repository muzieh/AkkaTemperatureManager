namespace TemperatureMonitor.Messages
{
    public sealed class RequestMetadata
    {
        public int RequestId { get; }

        public RequestMetadata(int requestId)
        {
            RequestId = requestId;
        } 
    }
}