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

        public FloorQuery()
        {
            
        }

        private FloorQuery(Dictionary<IActorRef,string> actorRefToSensorId, int requestId, IActorRef requester, TimeSpan timeout)
        {
            ActorRefToSensorId = actorRefToSensorId;
            RequestId = requestId;
            Requester = requester;
            Timeout = timeout;
        }

        protected override void PreStart()
        {
            foreach (var actorSensor in ActorRefToSensorId)
            {
                var sensorActor = actorSensor.Key;
                var sensorId = actorSensor.Value;
                Context.Watch(sensorActor);
                sensorActor.Tell(new RequestTemperature());
            }
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
               default:
                   break;
            }
        }
        
        public static Props Props() => Akka.Actor.Props.Create(() => new FloorQuery());

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