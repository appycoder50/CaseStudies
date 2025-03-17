using System;
using System.Collections.Generic;
using System.Linq;

class Flight
{
    public string Destination { get; set; }
    public double Distance { get; set; }
    public double BasePrice => Distance * 0.1;
    public int AvailableSeats { get; set; }
    public TimeSpan Duration { get; set; }

    public double GetCurrentPrice(DateTime bookingTime)
    {
        double timeFactor = (bookingTime.Hour >= 8 && bookingTime.Hour <= 20) ? 1.2 : 1.0; // Peak hour surcharge
        double availabilityFactor = (AvailableSeats < 20) ? 1.5 : 1.0; // Higher price when seats are low
        return BasePrice * timeFactor * availabilityFactor;
    }
}

class FlightRoutePlanner
{
    private Dictionary<string, List<Flight>> AllFlights;
    public FlightRoutePlanner()
    {
        AllFlights = new Dictionary<string, List<Flight>>();
    }

    public void AddFlight(string from, string to, double distance, int seats, TimeSpan duration)
    {
        if (!AllFlights.ContainsKey(from))
            AllFlights[from] = new List<Flight>();

        AllFlights[from].Add(new Flight { Destination = to, Distance = distance, AvailableSeats = seats, Duration = duration });
    }

    public List<string> FindCheapestRoute(string start, string end, DateTime bookingTime, int maxConnections = int.MaxValue, TimeSpan? maxDuration = null)
    {
        var pq = new SortedSet<(double, string, List<string>, int, TimeSpan)>(Comparer<(double, string, List<string>, int, TimeSpan)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));

        pq.Add((0, start, new List<string> { start }, 0, TimeSpan.Zero));

        var minCost = new Dictionary<string, double> { { start, 0 } };

        while (pq.Any())
        {
            var (cost, city, path, connections, totalDuration) = pq.First();
            pq.Remove(pq.First());

            if (city == end) return path;

            if (!AllFlights.ContainsKey(city)) continue;

            foreach (var flight in AllFlights[city])
            {
                double newCost = cost + flight.GetCurrentPrice(bookingTime);
                TimeSpan newDuration = totalDuration + flight.Duration;
                int newConnections = connections + 1;

                if (newConnections > maxConnections || (maxDuration.HasValue && newDuration > maxDuration))
                    continue;

                if (!minCost.ContainsKey(flight.Destination) || newCost < minCost[flight.Destination])
                {
                    minCost[flight.Destination] = newCost;
                    var newPath = new List<string>(path) { flight.Destination };
                    pq.Add((newCost, flight.Destination, newPath, newConnections, newDuration));
                }
            }
        }
        return new List<string>(); // No route found
    }

    public List<string> FindRoundTrip(string start, string end, DateTime bookingTime, int maxConnections = int.MaxValue, TimeSpan? maxDuration = null)
    {
        var onwardRoute = FindCheapestRoute(start, end, bookingTime, maxConnections, maxDuration);
        if (onwardRoute.Count == 0) return new List<string>();
        var returnRoute = FindCheapestRoute(end, start, bookingTime.AddDays(1), maxConnections, maxDuration);
        if (returnRoute.Count == 0) return new List<string>();

        onwardRoute.AddRange(returnRoute.Skip(1)); // Merge routes for a full round trip
        return onwardRoute;
    }
}

class Program
{
    static void Main()
    {
        FlightRoutePlanner planner = new FlightRoutePlanner();
        planner.AddFlight("City_A", "City_B", 500, 50, TimeSpan.FromHours(1));
        planner.AddFlight("City_B", "City_C", 300, 30, TimeSpan.FromHours(1));
        planner.AddFlight("City_A", "City_C", 800, 20, TimeSpan.FromHours(2));
        planner.AddFlight("City_C", "City_B", 600, 20, TimeSpan.FromHours(1));
        planner.AddFlight("City_B", "City_A", 300, 20, TimeSpan.FromHours(2));

        DateTime bookingTime = DateTime.Now;
        var route = planner.FindCheapestRoute("City_A", "City_C", bookingTime, maxConnections: 1, maxDuration: TimeSpan.FromHours(3));
        Console.WriteLine("Cheapest Route: " + string.Join(" -> ", route));

        var roundTrip = planner.FindRoundTrip("City_A", "City_C", bookingTime, maxConnections: 2, maxDuration: TimeSpan.FromHours(5));
        Console.WriteLine("Round Trip Route: " + string.Join(" -> ", roundTrip));
    }
}
