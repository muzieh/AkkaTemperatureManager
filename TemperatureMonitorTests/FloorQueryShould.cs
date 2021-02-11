using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using TemperatureMonitor;
using TemperatureMonitor.Messages;
using Xunit;

namespace TemperatureMonitorTests
{
    public class FloorQueryShould : TestKit
    {
        [Fact]
        
        public void ReturnTemperatures()
        {
            var queryRequester = CreateTestProbe();
            var temperatureSensor1 = CreateTestProbe();
            var temperatureSensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Props(
               actorRefToSensorId: new Dictionary<IActorRef, string>
               {
                   [temperatureSensor1.Ref] = "s1",
                   [temperatureSensor2.Ref] = "s2"
               },
               requestId: 12,
               requester: queryRequester.Ref,
               timeout: TimeSpan.FromSeconds(3)
            ), "floor-query");

            temperatureSensor1.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                m.RequestId.Should().Be(FloorQuery.TemperatureRequestCorrelactionId);
                sender.Should().Be(floorQuery);
            });
            temperatureSensor2.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                m.RequestId.Should().Be(FloorQuery.TemperatureRequestCorrelactionId);
                sender.Should().Be(floorQuery);
            });
            
            floorQuery.Tell(new RespondTemperature(
                FloorQuery.TemperatureRequestCorrelactionId, 23.5m), temperatureSensor1.Ref);
            floorQuery.Tell(new RespondTemperature(
                FloorQuery.TemperatureRequestCorrelactionId, 13.2m), temperatureSensor2.Ref);

            queryRequester.ExpectMsg<RespondFloorTemperatures>((m) =>
            {
                m.RequestId.Should().Be(12);
                m.TemperatureReadings.Count.Should().Be(2);
                
                m.TemperatureReadings["s1"].Should().BeAssignableTo<TemperatureAvailable>();
                m.TemperatureReadings["s1"].As<TemperatureAvailable>().Temperature.Should().Be(23.5m);
                
                m.TemperatureReadings["s2"].Should().BeAssignableTo<TemperatureAvailable>();
                m.TemperatureReadings["s2"].As<TemperatureAvailable>().Temperature.Should().Be(13.2m);
            });

        }
        
    }
}