namespace TemperatureMonitor.Messages
{
    public class RequestTemperature
    {
        public int RequestId { get; }

        public RequestTemperature(int requestId)
        {
            RequestId = requestId;
        } 
    }
}