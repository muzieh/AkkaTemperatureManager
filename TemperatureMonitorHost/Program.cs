using System;
using System.Threading;
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
                        /*Console.WriteLine("Press key to cycle");
                        var key = Console.ReadLine();
                        if (key.ToUpperInvariant() == "Q")
                        {
                                                
                            Console.WriteLine("end"); 
                            Environment.Exit(0);
                        }*/
                        Thread.Sleep(TimeSpan.FromMilliseconds(300));
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
            var hasReading = 0;
            var noReadingsYet = 0;
            var timeout = 0;
            var notAvailable = 0;
            Console.SetCursorPosition(0,0);
            foreach (var readingKV in temps.TemperatureReadings)
            {
                switch (readingKV.Value)
                {
                   case TemperatureAvailable t:
                       hasReading++;
                       Console.WriteLine($"{readingKV.Key} - {t.Temperature}");
                       break;
                   case NoTemperatureAvailable t:
                       noReadingsYet++;
                       break;
                   case SensorTimedOut t:
                       timeout++;
                       break;
                   case SensorNotAvailable t:
                       notAvailable++;
                       break;
                } 
            }
            
            Console.WriteLine($"all: {temps.TemperatureReadings.Count} temp: {hasReading} notemp: {noReadingsYet} timedOut: {timeout} notAvailable: {notAvailable}");
        }

        private static async Task CreateSimulatedSensors(IActorRef floorManager)
        {
            for (var i = 1; i <= 25; i++)
            {
                var sensor = new SensorSimulator("basement", i.ToString(), floorManager);
                await sensor.Connect();
                if (i != 100)
                {
                    sensor.StartSendingSimulatedReadings();
                }
            }
        }

    }
}