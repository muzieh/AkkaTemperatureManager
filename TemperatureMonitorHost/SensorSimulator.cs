using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitorHost
{
    public class SensorSimulator
    {
        private readonly string _floorId;
        private readonly string _sensorId;
        private IActorRef _temperatureSensor;
        private IActorRef _floorManager;
        private Random _random;
        private Timer _timer;

        public SensorSimulator(string floorId, string sensorId, IActorRef floorManager)
        {
            _floorId = floorId;
            _sensorId = sensorId;
            _floorManager = floorManager;
            _random = new Random(int.Parse(sensorId));
        }

        public async Task Connect()
        {
            var response = await _floorManager.Ask<RespondSensorRegistered>(
                new RequestSensorRegister(1, _floorId, _sensorId));
            _temperatureSensor = response.SensorRef;
        }

        public void StartSendingSimulatedReadings()
        {
            _timer = new Timer(SimulateUpdateTemperature, null, 0, _random.Next(5000, 9000));
        }

        private void SimulateUpdateTemperature(object state)
        {
            _temperatureSensor.Tell(new RequestUpdateTemperature(0, (decimal)_random.NextDouble() * 100.0m));
        }
    }
}