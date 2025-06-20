using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace parking_zone_serverside
{
    class Program
    {
        public static Dictionary<string, StreamWriter> connectedClients = new Dictionary<string, StreamWriter>();
        public static readonly object clientsLock = new object();

        static void Main(string[] args)
        {
            List<ParkingSpot> parkingSpots = new List<ParkingSpot>();
            for (int i = 1; i <= 100; i++)
            {
                parkingSpots.Add(new ParkingSpot
                {
                    SpotID = i,
                    IsAvailable = true
                });
            }
            IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(ipaddress, 3333);
            listener.Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    lock (parkingSpots)
                    {
                        var now = DateTime.Now;
                        foreach (var spot in parkingSpots)
                        {
                            if (!spot.IsAvailable)
                            {
                                var timeLeft = spot.EndTime - now;

                                if (timeLeft <= TimeSpan.FromMinutes(10) && timeLeft > TimeSpan.FromMinutes(9))
                                {
                                    lock (Program.clientsLock)
                                    {
                                        if (Program.connectedClients.TryGetValue(spot.ReservedBy, out var writer))
                                        {
                                            writer.WriteLine($"Notification: Your spot {spot.SpotID} will expire in 10 minutes.");
                                        }
                                    }
                                }
                                else if (timeLeft <= TimeSpan.Zero)
                                {
                                    spot.IsAvailable = true;
                                    lock (Program.clientsLock)
                                    {
                                        if (Program.connectedClients.TryGetValue(spot.ReservedBy, out var writer))
                                        {
                                            writer.WriteLine($"Notification: Your spot {spot.SpotID} has expired and is now released.");
                                        }
                                    }
                                    spot.ReservedBy = null;
                                    spot.StartTime = DateTime.MinValue;
                                    spot.EndTime = DateTime.MinValue;
                                }
                            }
                        }
                    }
                    await Task.Delay(60000);
                }
            });

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("client connected");
                Task.Run(() =>
                {
                    ParkingSpot.handleclient(client, parkingSpots);
                });
            }
        }
    }

    class ParkingSpot
    {
        public int SpotID;
        public bool IsAvailable;
        public string ReservedBy;
        public DateTime StartTime;
        public DateTime EndTime;

        public static void handleclient(TcpClient client, List<ParkingSpot> parkingSpots)
        {
            string clientName = null;
            try
            {
                NetworkStream stream = client.GetStream();
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                writer.WriteLine("Welcome to Parking Ssytem");
                writer.WriteLine("Please enter your good name:");
                clientName = reader.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(clientName))
                {
                    writer.WriteLine("Invalid name. Disconnecting.");
                    return;
                }

                lock (Program.clientsLock)
                {
                    if (!Program.connectedClients.ContainsKey(clientName))
                    {
                        Program.connectedClients.Add(clientName, writer);
                    }
                }

                while (true)
                {
                    string command = reader.ReadLine();
                    if (command == null)
                        break;
                    if (command.ToUpper() == "VIEW")
                    {
                        int availablecount = 0;
                        lock (parkingSpots)
                        {
                            availablecount = parkingSpots.Count(s => s.IsAvailable);
                        }
                        writer.WriteLine($"Available spots: {availablecount}");
                    }
                    else if (command.StartsWith("BOOK"))
                    {
                        string[] parts = command.Split(' ');
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int hours))
                        {
                            writer.WriteLine("Invalid command format. Use: BOOK <hours>");
                            return;
                        }
                        lock (parkingSpots)
                        {
                            var spot = parkingSpots.FirstOrDefault(s => s.IsAvailable == true);
                            if (spot != null)
                            {
                                spot.IsAvailable = false;
                                spot.StartTime = DateTime.Now;
                                spot.EndTime = spot.StartTime.AddHours(hours);
                                spot.ReservedBy = clientName;
                                writer.WriteLine($"Spot {spot.SpotID} booked from {spot.StartTime} to {spot.EndTime}");
                            }
                            else
                                writer.WriteLine("No available spots.");
                        }
                    }
                    else if (command.ToUpper() == "CHECK")
                    {
                        bool found = false;
                        lock (parkingSpots)
                        {
                            foreach (var spot in parkingSpots)
                            {
                                if (spot.ReservedBy == clientName)
                                {
                                    writer.WriteLine($"spot{spot.SpotID} reserved form {spot.StartTime} to {spot.EndTime}");
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                writer.WriteLine("no reserved spots");
                            }
                        }
                    }
                    else if (command.ToUpper() == "RELEASE")
                    {
                        bool released = false;
                        lock (parkingSpots)
                        {
                            foreach (var spot in parkingSpots)
                            {
                                if (spot.ReservedBy == clientName)
                                {
                                    spot.IsAvailable = true;
                                    spot.ReservedBy = null;
                                    spot.StartTime = DateTime.MinValue;
                                    spot.EndTime = DateTime.MinValue;
                                    writer.WriteLine($"Spot {spot.SpotID} has been released.");
                                    released = true;
                                    break;
                                }
                            }
                        }
                        if (!released)
                        {
                            writer.WriteLine("You have no reservation to release.");
                        }
                    }
                    else if (command.ToUpper() == "EXIT")
                    {
                        writer.WriteLine("goodbye");
                        break;
                    }
                    else
                    {
                        writer.WriteLine("Invalid command. Try: VIEW or EXIT");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error handling client: " + e.Message);
            }
            finally
            {
                lock (Program.clientsLock)
                {
                    if (!string.IsNullOrEmpty(clientName) && Program.connectedClients.ContainsKey(clientName))
                    {
                        Program.connectedClients.Remove(clientName);
                    }
                }
                client.Close();
                Console.WriteLine("Client disconnected");
            }
        }
    }
}
