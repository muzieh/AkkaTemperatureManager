using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitor
{
    public class FloorQuery : UntypedActor
    {
        private ICancelable _queryCancelationToken;
        public Dictionary<IActorRef, string> ActorRefToSensorId { get; }

        public Dictionary<string, ITemperatureQueryReading> Readings { get; } =
            new Dictionary<string, ITemperatureQueryReading>();
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
            
            _queryCancelationToken = Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, TemperatureQueryCanceled.Instance, Self);
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

        protected override void PostStop()
        {
            _queryCancelationToken.Cancel();
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RespondTemperature m when m.RequestId == TemperatureRequestCorrelactionId:
                    if (m.Temperature.HasValue )
                    {
                        var temperatureAvailable = new TemperatureAvailable(m.Temperature.Value);
                        RecordTemperatureReading(Context.Sender, temperatureAvailable);
                    }
                    else
                    {
                        RecordTemperatureReading(Context.Sender, NoTemperatureAvailable.Instance);
                    }
                    break;
                case Terminated m:
                    RecordTemperatureReading(Context.Sender, SensorNotAvailable.Instance);
                    break;
                case TemperatureQueryCanceled m:
                    foreach (var sensorActor in ActorRefToSensorId.Keys)
                    {
                        Readings.Add(ActorRefToSensorId[sensorActor], SensorTimedOut.Instance);
                    }
                    Requester.Tell(new RespondFloorTemperatures(RequestId, Readings.ToImmutableDictionary()));
                    Context.Stop(Self);
                    break;
               default:
                   Unhandled(message);
                   break;
            }
        }

        private void RecordTemperatureReading(IActorRef sensor, ITemperatureQueryReading temperatureReading)
        {
            var sensorId = ActorRefToSensorId[sensor];
            Context.Unwatch(sensor);
            ActorRefToSensorId.Remove(sensor);
            Readings.Add(sensorId, temperatureReading);
            if (ActorRefToSensorId.Count == 0)
            {
                Requester.Tell(new RespondFloorTemperatures(RequestId, Readings.ToImmutableDictionary()));
            }
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