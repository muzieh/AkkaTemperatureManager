using System;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using TemperatureMonitor;
using TemperatureMonitor.Messages;

namespace TemperatureMonitorHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("started");
            try
            {
                using (var system = ActorSystem.Create("iot-temperature-monitor"))
                {
                    var floorManager = system.ActorOf(FloorManager.Props(), "floor-manager");
                    await CreateSimulatedSensors(floorManager);
                    while (true)
                    {
                        Console.WriteLine("Press key to cycle");
                        var key = Console.ReadLine();
                        if (key.ToUpperInvariant() == "Q")
                        {
                                                
                            Console.WriteLine("end"); 
                            Environment.Exit(0);
                        }

                        await DisplayTemperatures(system);

                    }

                }
            }
            catch (Exception excp)
            {
                Console.WriteLine("error");
            }
        }

        private static async Task DisplayTemperatures(ActorSystem system)
        {
            var temps = await system.ActorSelection("akka://iot-temperature-monitor/user/floor-manager/floor-basement")
                .Ask<RespondFloorTemperatures>(new RequestFloorTemperatures(32));
            
            Console.WriteLine(temps.TemperatureReadings.Count);
        }

        private static async Task CreateSimulatedSensors(IActorRef floorManager)
        {
            for (var i = 1; i <= 100; i++)
            {
                var sensor = new SensorSimulator("basement", i.ToString(), floorManager);
                await sensor.Connect();
                sensor.StartSendingSimulatedReadings();
            }
        }
    }
}