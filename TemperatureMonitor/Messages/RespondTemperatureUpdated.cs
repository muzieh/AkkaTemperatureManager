namespace TemperatureMonitor.Messages
{
    public sealed class RespondTemperatureUpdated
    {
        public int RequestId { get; }

        public RespondTemperatureUpdated(int requestId)
        {
            RequestId = requestId;
        } 
    }
}