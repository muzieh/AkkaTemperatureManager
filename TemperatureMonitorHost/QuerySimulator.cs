using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitorHost
{
    public class QuerySimulator 
    {
        private readonly string _floorId;
        private IActorRef _temperatureSensor;
        private IActorRef _floor;
        private Random _random;
        private Timer _timer;

        public QuerySimulator(string floorId, IActorRef floor)
        {
            _floorId = floorId;
            _floor = floor;
            _random = new Random(100);
        }


        public void StartQuerying()
        {
            _timer = new Timer(SimulateQueryFloor, null, 0, _random.Next(3000, 3000));
        }

        private async void SimulateQueryFloor(object state)
        {
            var response = await _floor.Ask<RespondFloorTemperatures>(new RequestFloorTemperatures(32));
            var readings = response.TemperatureReadings;
            foreach (var reading in readings)
            {
               Console.WriteLine(reading); 
            }
        }
    }
}