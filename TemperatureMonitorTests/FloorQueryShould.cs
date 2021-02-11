using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using TemperatureMonitor;
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

            var query = Sys.ActorOf(FloorQuery.Props(
               actorRefToSensorId: new Dictionary<IActorRef, string>
               {
                   [temperatureSensor1.Ref] = "s1",
                   [temperatureSensor2.Ref] = "s2"
               },
               requestId: 12,
               requester: queryRequester.Ref,
               timeout: TimeSpan.FromSeconds(3)
            ), "floor-query");
        }
    }
}