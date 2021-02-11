using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitor
{
    public class FloorManager : UntypedActor
    {
        private Dictionary<string, IActorRef> floorIdToActorRefs = new Dictionary<string, IActorRef>();
        public FloorManager()
        {
            
        }
        
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestSensorRegister m:
                    if (floorIdToActorRefs.TryGetValue(m.Floor, out IActorRef floorActor))
                    {
                       floorActor.Forward(m); 
                    }
                    else
                    {
                        var newFloor = Context.ActorOf(Floor.Prop(m.Floor), $"floor-{m.Floor}");
                        floorIdToActorRefs.Add(m.Floor, newFloor);
                        Context.Watch(newFloor);
                        newFloor.Forward(m);
                    }
                    break;
                case RequestFloorIds m:
                    Sender.Tell(new RespondFloorIds(m.RequestId, floorIdToActorRefs.Keys.ToImmutableHashSet()));
                    break;
                case Terminated m:
                    var floorId = floorIdToActorRefs.First(kv => kv.Value == m.ActorRef).Key;
                    floorIdToActorRefs.Remove(floorId);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Prop() => Akka.Actor.Props.Create(() => new FloorManager());
    }
}