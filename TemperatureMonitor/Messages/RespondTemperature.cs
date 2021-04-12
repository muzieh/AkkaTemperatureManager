namespace TemperatureMonitor.Messages
{
    public sealed class RespondTemperature
    {
        public int RequestId { get; }
        public decimal? Temperature { get; }
        public RespondTemperature(int requestId, decimal? temperature)
        {
            RequestId = requestId;
            Temperature = temperature;
        }

    }
}