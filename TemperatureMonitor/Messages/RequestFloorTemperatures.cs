namespace TemperatureMonitor.Messages
{
    public class RequestFloorTemperatures
    {
        public int RequestId { get; }

        public RequestFloorTemperatures(int requestId)
        {
            RequestId = requestId;
        } 
    }
}