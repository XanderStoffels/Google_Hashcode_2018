using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCode
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var files = new[]{ "a_example", "b_should_be_easy", "c_no_hurry", "d_metropolis", "e_high_bonus"};

            //Options
            foreach (var file in files)
            
                
             {
                Console.WriteLine("file:" + file);
                var filetext = File.ReadAllLines(@file+".in");
            var optionvars = filetext.First().Split(' ').Select(int.Parse).ToList();
                
            var Options = new Options()
            {
                Rows = optionvars[0],
                Columns = optionvars[1],
                FleetSize = optionvars[2],
                RideCount = optionvars[3],
                PreRideBonus = optionvars[4],
                FinalStep = optionvars[5],
                CurrentStep = 0
            };

            //Read Rides
            long id = 0;
            var rides = filetext.Skip(1)
                .Select(row => row.Split(' ').Select(int.Parse).ToList())
                .Select(rowsplit => new Ride
                {
                    Id = id++,
                    Start = new Point(rowsplit[0], rowsplit[1]),
                    Stop = new Point(rowsplit[2], rowsplit[3]),
                    Duration = StepsBetween(new Point(rowsplit[0], rowsplit[1]), new Point(rowsplit[2], rowsplit[3])),
                    StartStep = rowsplit[4],
                    Deadline = rowsplit[5],
                    InProgress = false
                })
                .ToList();


            //Order rides
            rides = new List<Ride>(rides.OrderBy(r => r.StartStep));
            rides.Reverse();
            //Vehicle creation
            var fleet = new List<Vehicle>();
            id = 0;
            for (var x = 0; x < Options.FleetSize; x++)
                fleet.Add(new Vehicle()
                {
                    CurrentLocation = new Point(0, 0),
                    Id =  id++
                });


                while (Options.CurrentStep < Options.FinalStep)
            {
              //  Console.WriteLine("\n{0}", Options.CurrentStep + "\n-----------");
               
                foreach (var availableCar in fleet.Where(c => c.CurrentRide == null))
                {
                    availableCar.PriorityList.Clear();
                    foreach (var ride in rides.Where(r => !r.InProgress).Where(r => Options.CurrentStep < r.Deadline))
                    {

                        if (availableCar.CurrentRide != null)
                            continue;

                        var steps = StepsBetween(availableCar.CurrentLocation, ride.Start);
                        var arriveAtStart = Options.CurrentStep + steps;

                        var waittime = ride.StartStep - arriveAtStart;
                        if (waittime < 0)
                            waittime = 0;


                        //Kan de car nog de deadline halen?
                        if (ride.Deadline - ride.Duration >= arriveAtStart)
                            availableCar.PriorityList.Add(ride, waittime + Convert.ToInt64(steps));
                    }

                    if (!availableCar.PriorityList.Any())
                        continue;
                    ;

                    var assignedRide = availableCar.PriorityList.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                    assignedRide.InProgress = true;
                    availableCar.CurrentRide = assignedRide;
                    availableCar.WillBeCompletedAt = Options.CurrentStep + availableCar.PriorityList[assignedRide] + assignedRide.Duration;
                 //   Console.WriteLine("Ride " + assignedRide.Id + " assigned voor auto " + availableCar.Id);

                }


                //Autotjes rijden
                foreach (var busyCar in fleet.Where(c => c.CurrentRide != null))
                {
                    if (busyCar.WillBeCompletedAt != Options.CurrentStep) continue;
                    busyCar.CurrentLocation = busyCar.CurrentRide.Stop;
                    busyCar.CompletedRides.Add(busyCar.CurrentRide);
                    rides.Remove(busyCar.CurrentRide);
                  //  Console.WriteLine("Ride " + busyCar.CurrentRide.Id + " has been completed");
                    busyCar.CurrentRide = null;
                    busyCar.WillBeCompletedAt = 0;
                  
                }

                Options.CurrentStep++;
            }

            //Wegschrijven
            var builder = new StringBuilder();
            foreach (var car in fleet)
                builder.Append(car.CompletedRides.Count + " " + string.Join(" ", car.CompletedRides.Select(c => c.Id).ToArray()) + "\n");
                File.WriteAllText(@/out/"+file+".out", builder.ToString());
            Console.WriteLine("done");
        }

            Console.ReadKey();
        }

        public static int StepsBetween(Point start, Point end)
        {
            return Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
        }
    }



    public class Options
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int FleetSize { get; set; }
        public int RideCount { get; set; }
        public int PreRideBonus { get; set; }
        public long FinalStep { get; set; }
        public long CurrentStep { get; set; }

    }

    public class Ride
    {
        public long Id { get; set; }
        public Point Start { get; set; }
        public Point Stop { get; set; }
        public long StartStep { get; set; }
        public long Deadline { get; set; }
        public bool InProgress { get; set; }
        public int Duration { get; set; }

    }

    public class Vehicle
    {
        public long Id { get; set; }
        public Point CurrentLocation { get; set; }
        public Ride CurrentRide { get; set; }
        public List<Ride> CompletedRides { get; set; } = new List<Ride>();
        public long WillBeCompletedAt { get; set; }


        public Dictionary<Ride, long> PriorityList { get; set; } = new Dictionary<Ride, long>();
    }
}
