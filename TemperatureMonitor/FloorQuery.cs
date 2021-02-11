using System;
using System.Collections.Generic;
using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitor
{
    public class FloorQuery : UntypedActor
    {
        public Dictionary<IActorRef, string> ActorRefToSensorId { get; }
        public int RequestId { get; }
        public IActorRef Requester { get; }
        public TimeSpan Timeout { get; }
        public static int TemperatureRequestCorrelactionId { get; } = 1;

        public FloorQuery(Dictionary<IActorRef,string> actorRefToSensorId, int requestId, IActorRef requester, TimeSpan timeout)
        {
            ActorRefToSensorId = actorRefToSensorId;
            RequestId = requestId;
            Requester = requester;
            Timeout = timeout;
        }

        protected override void PreStart()
        {
            foreach (var sensorActor in ActorRefToSensorId.Keys)
            {
                var sensorId = ActorRefToSensorId[sensorActor];
                Context.Watch(sensorActor);
                sensorActor.Tell(new RequestTemperature(FloorQuery.TemperatureRequestCorrelactionId));
            }
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RespondTemperature m when m.RequestId == TemperatureRequestCorrelactionId:
                    var temperatureAvailable = new TemperatureAvailable(m.Temperature.Value);
                    RecordTemperatureReading(Context.Sender, temperatureAvailable);
                    break;
               default:
                   Unhandled(message);
                   break;
            }
        }

        private void RecordTemperatureReading(IActorRef sensor, ITemperatureQueryReading temperatureReading)
        {
            Context.Unwatch(sensor);
            ActorRefToSensorId.Remove(sensor);
        }

        public static Props Props(
            Dictionary<IActorRef, string> actorRefToSensorId,
            int requestId,
            IActorRef requester,
            TimeSpan timeout) =>
            Akka.Actor.Props.Create(
                () => new FloorQuery(
                   actorRefToSensorId, requestId, requester, timeout 
                ));
    }
}