using System;
using Akka.Actor;
using Akka.Event;
using Akka.TestKit.Xunit2;
using TemperatureMonitor;
using TemperatureMonitor.Messages;
using Xunit;

namespace TemperatureMonitorTests
{
    public class TemperatureSensorShould : TestKit
    {
        [Fact]
        public void ReturnTemperatureReadingWhenRequested()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestMetadata(1), probe.Ref);
            var received = probe.ExpectMsg<ResponseMetadata>();
            Assert.Equal(1, received.RequestId);
            Assert.Equal("a", received.Floor);
            Assert.Equal("1", received.SensorId);
        }

        [Fact]
        public void StartWithNoTemperature()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestTemperature(1), probe.Ref);
            var received = probe.ExpectMsg<RespondTemperature>();
            Assert.Null(received.Temperature);
            Assert.Equal(1, received.RequestId);
        }

        [Fact]
        public void ConfirmTemperatureUpdate()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestUpdateTemperature(1, 12.4m), probe.Ref);
            probe.ExpectMsg<RespondTemperatureUpdated>(m =>
            {
                Assert.Equal(1, m.RequestId);
             });
        }
        
        [Fact]
        public void RespondWithNewTemperatureAfterTemperatureUpdate()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestUpdateTemperature(1, 12.4m));
            sensor.Tell(new RequestTemperature(2), probe.Ref);
            var response = probe.ExpectMsg<RespondTemperature>();
            Assert.Equal(2,response.RequestId);
            Assert.Equal(12.4m, response.Temperature);
        }

        [Fact]
        public void RegisterSensor()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestSensorRegister(43, "a", "1"), probe.Ref);

            probe.ExpectMsg<RespondSensorRegistered>((m) =>
            {
               Assert.Equal(43, m.RequestId); 
               Assert.Equal(sensor, m.SensorRef);
            });
        }
        
        [Fact]
        public void NotRegisterSensorWhenFloorDoesntMatch()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();
            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestSensorRegister(43, "b", "1"), probe.Ref);
            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();
            Assert.IsType<RequestSensorRegister>(unhandled.Message);
        }
        
        [Fact]
        public void NotRegisterSensorWhenSensorIdDoesntMatch()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();
            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestSensorRegister(43, "a", "4"), probe.Ref);
            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();
            Assert.IsType<RequestSensorRegister>(unhandled.Message);
        }
    }
}