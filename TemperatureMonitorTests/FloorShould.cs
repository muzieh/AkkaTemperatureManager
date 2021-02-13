using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using TemperatureMonitor;
using TemperatureMonitor.Messages;
using Xunit;

namespace TemperatureMonitorTests
{
    public class FloorShould : TestKit
    {
        [Fact]
        public void RegisterSensor()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"), "floor-a");
            floor.Tell(new RequestSensorRegister(54, "a", "2"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>(m =>
            {
                m.RequestId.Should().Be(54);
                m.SensorRef.Should().Be(probe.LastSender);
            });
        } 
        
        [Fact]
        public void ReturnTheSameTemperatureSensorGivenThatRegisterTwiceTheSameSensor()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"), "floor-a");
            floor.Tell(new RequestSensorRegister(54, "a", "1"), probe.Ref);
            var firstSensor = probe.ExpectMsg<RespondSensorRegistered>().SensorRef;
            floor.Tell(new RequestSensorRegister(54, "a", "1"), probe.Ref);
            var secondSensor = probe.ExpectMsg<RespondSensorRegistered>().SensorRef;
            
            secondSensor.Should().Be(firstSensor);
        }

        [Fact]
        public void NotRegisterSensorWhenFloorIdDoesntMatch()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();
            Sys.EventStream.Subscribe(eventStreamProbe.Ref, typeof(Akka.Event.UnhandledMessage));
            var floor = Sys.ActorOf(Floor.Props("a"), "floor-a");
            floor.Tell(new RequestSensorRegister(54, "b", "1"), probe.Ref);
            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<UnhandledMessage>();
            unhandled.Message.Should().BeOfType<RequestSensorRegister>();
        }

        [Fact]
        public void ReturnListOfSensorIds()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"), "floor-a");
            floor.Tell(new RequestSensorRegister(54, "a", "1"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            floor.Tell(new RequestSensorRegister(54, "a", "4"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            floor.Tell(new RequestSensorIdsList(43, "a"), probe.Ref);
            
            var response = probe.ExpectMsg<RespondSensorIdsList>();
            response.RequestId.Should().Be(43);
            response.SensorIds.Should().NotBeEmpty().And.Contain(new string[] {"1", "4"});
        }

        [Fact]
        public void NotContainStoppedTemperatureSensor()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"), "floor-a");
            floor.Tell(new RequestSensorRegister(54, "a", "1"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            var firstSensor = probe.LastSender;
            floor.Tell(new RequestSensorRegister(54, "a", "4"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            probe.Watch(firstSensor);
            firstSensor.Tell(PoisonPill.Instance, probe);
            probe.ExpectTerminated(firstSensor);
            
            floor.Tell(new RequestSensorIdsList(43, "a"), probe);
            var response = probe.ExpectMsg<RespondSensorIdsList>();
            
            response.RequestId.Should().Be(43);
            response.SensorIds.Should()
                .NotBeEmpty()
                .And.HaveCount(1)
                .And.Contain(new string[] { "4"});
        }

        [Fact]
        public void CreateFloorQuery()
        {
            var probe = CreateTestProbe();

            var floor = Sys.ActorOf(Floor.Props("a"), "floor-a");
            
            floor.Tell(new RequestSensorRegister(12, "a", "1"), probe);
            var temp1 = probe.ExpectMsg<RespondSensorRegistered>().SensorRef;
            
            floor.Tell(new RequestSensorRegister(42, "a", "2"), probe);
            var temp2 = probe.ExpectMsg<RespondSensorRegistered>().SensorRef;
            
            floor.Tell(new RequestSensorRegister(2, "a", "4"), probe);
            var temp3 = probe.ExpectMsg<RespondSensorRegistered>().SensorRef;
            
            temp1.Should().NotBeNull();
            temp2.Should().NotBeNull();
            temp3.Should().NotBeNull();

            temp1.Tell(new RequestUpdateTemperature(32, 12.55m), probe);
            probe.ExpectMsg<RespondTemperatureUpdated>((m) =>
            {
                m.RequestId.Should().Be(32);
            });
            temp2.Tell(new RequestUpdateTemperature(12, 16.15m), probe);
            probe.ExpectMsg<RespondTemperatureUpdated>((m) =>
            {
                m.RequestId.Should().Be(12);
            });
            
            floor.Tell(new RequestFloorTemperatures(43), probe);
            probe.ExpectMsg<RespondFloorTemperatures>((m, sender) =>
            {
                m.RequestId.Should().Be(43);
                m.TemperatureReadings.Should().HaveCount(3);
            });
        }
    }
}