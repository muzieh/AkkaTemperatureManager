namespace TemperatureMonitor.Messages
{
    public sealed class RequestUpdateTemperature
    {
        public int RequestId { get; }
        public decimal Temperature { get; }

        public RequestUpdateTemperature(int requestId, decimal temperature)
        {
            RequestId = requestId;
            Temperature = temperature;
        } 
    }
}