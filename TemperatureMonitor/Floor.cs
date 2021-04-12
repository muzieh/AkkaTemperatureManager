using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitor
{
    public class Floor : UntypedActor
    {
        public string FloorId { get; }
        private Dictionary<string, IActorRef> sensorIdToActorRef = new Dictionary<string, IActorRef>();

        public Floor(string floorId)
        {
            FloorId = floorId;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestSensorRegister m when FloorId == m.Floor:
                    if (sensorIdToActorRef.TryGetValue(m.SensorId, out IActorRef existingTemperatureSensor))
                    {
                        existingTemperatureSensor.Forward(m);
                    }
                    else
                    {
                        var sensor = Context.ActorOf(TemperatureSensor.Props(m.Floor, m.SensorId),$"temperature-sensor-{m.SensorId}" );
                        sensorIdToActorRef.Add(m.SensorId, sensor);
                        Context.Watch(sensor);
                        sensor.Forward(m);
                    }
                    break;
                case RequestSensorIdsList m when m.FloorId == FloorId:
                    Sender.Tell(new RespondSensorIdsList(m.RequestId, sensorIdToActorRef.Keys.ToImmutableHashSet()));
                    break;
                case RequestFloorTemperatures m:
                    var actorRefToSensorId = new Dictionary<IActorRef, string>();
                    foreach (var sensorActorKeyValue in sensorIdToActorRef)
                    {
                        actorRefToSensorId[sensorActorKeyValue.Value] = sensorActorKeyValue.Key;
                    }
                    Context.ActorOf(FloorQuery.Props(actorRefToSensorId, m.RequestId, Sender, TimeSpan.FromMilliseconds(500)));
                    break;
                case Terminated m:
                    var sensorId = sensorIdToActorRef.First(kv => kv.Value == m.ActorRef).Key;
                    sensorIdToActorRef.Remove(sensorId);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Props(string floor) => Akka.Actor.Props.Create(() => new Floor(floor));
    }
}