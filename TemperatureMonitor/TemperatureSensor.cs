using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitor
{
    public class TemperatureSensor : UntypedActor 
    {
        public string Floor { get; }
        public string SensorId { get; }

        private decimal? _temperature;

        public TemperatureSensor(string floor, string sensorId)
        {
            Floor = floor;
            SensorId = sensorId;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
               case RequestMetadata req:
                  Sender.Tell(new ResponseMetadata(req.RequestId, Floor, SensorId, Self)); 
                   break;
               case RequestTemperature m:
                   Sender.Tell(new RespondTemperature(m.RequestId, _temperature));
                   break;
               case RequestUpdateTemperature m:
                   _temperature = m.Temperature;
                   Sender.Tell(new RespondTemperatureUpdated(m.RequestId));
                   break;
               case RequestSensorRegister m
                   when m.Floor == Floor && m.SensorId == SensorId:
                   
                   Sender.Tell(new RespondSensorRegistered(m.RequestId, Context.Self));
                   break;
               default:
                   Unhandled(message);
                   break;
            }
        }

        public static Props Prop(string floor, string sensorId) =>
            Akka.Actor.Props.Create(() => new TemperatureSensor(floor, sensorId));
    }
}