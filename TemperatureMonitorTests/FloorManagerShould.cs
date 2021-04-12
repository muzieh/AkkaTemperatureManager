using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using TemperatureMonitor;
using TemperatureMonitor.Messages;
using Xunit;

namespace TemperatureMonitorTests
{
    public class FloorManagerShould : TestKit
    {
        [Fact]
        public void RegisterNewSensor()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props(), "floor-manager");
            floorManager.Tell(new RequestSensorRegister(54, "a", "2"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
        }
        
        [Fact]
        public void ReturnListOfFloorIds()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props(), "floor-manager");
            floorManager.Tell(new RequestSensorRegister(54, "a", "2"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            floorManager.Tell(new RequestSensorRegister(55, "b", "5"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            
            floorManager.Tell(new RequestFloorIds(56), probe);
            var response = probe.ExpectMsg<RespondFloorIds>();
            response.RequestId.Should().Be(56);
            response.FloorIds.Should().BeEquivalentTo(new string[] { "a", "b"});
        }
        
        [Fact]
        public void ReturnSingleFloorGivenMultipleSensorsAddedToTheSameFloor()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props(), "floor-manager");
            floorManager.Tell(new RequestSensorRegister(54, "a", "2"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            floorManager.Tell(new RequestSensorRegister(55, "a", "5"), probe);
            probe.ExpectMsg<RespondSensorRegistered>();
            
            floorManager.Tell(new RequestFloorIds(56), probe);
            var response = probe.ExpectMsg<RespondFloorIds>();
            response.RequestId.Should().Be(56);
            response.FloorIds.Should().BeEquivalentTo(new[] { "a"});
        }
        
        [Fact]
        public async Task NotReturnStoppedFloorActor()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props(), "floor-manager");
            floorManager.Tell(new RequestSensorRegister(54, "a", "2"));
            floorManager.Tell(new RequestSensorRegister(55, "b", "5"));
            
            //find floor "a" and stop it 
            var firstFloor = await Sys.ActorSelection("akka://test/user/floor-manager/floor-a")
                .ResolveOne(TimeSpan.FromSeconds(3));
            probe.Watch(firstFloor);
            firstFloor.Tell(PoisonPill.Instance, probe);
            probe.ExpectTerminated(firstFloor);
            
            floorManager.Tell(new RequestFloorIds(56), probe);
            var response = probe.ExpectMsg<RespondFloorIds>();
            
            response.RequestId.Should().Be(56);
            response.FloorIds.Should()
                .HaveCount(1)
                .And.BeEquivalentTo(new[] { "b"});
        }
        
    }
}