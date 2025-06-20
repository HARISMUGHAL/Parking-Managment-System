using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace parking_zone_client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 3333);
                NetworkStream stream = client.GetStream();
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                // Show welcome + read server intro lines
                Console.WriteLine(reader.ReadLine());
                Console.WriteLine(reader.ReadLine());

                // Send client name
                Console.Write("Enter your client name: ");
                string name = Console.ReadLine();
                writer.WriteLine(name);

                // Background thread to read notifications or responses
                Thread readThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            string line = reader.ReadLine();
                            if (line != null)
                                Console.WriteLine("\n[SERVER]: " + line);
                        }
                        catch { break; }
                    }
                });
                readThread.IsBackground = true;
                readThread.Start();

                // Show menu
                while (true)
                {
                    Console.WriteLine("\n--- Parking Menu ---");
                    Console.WriteLine("1. View Available Spots");
                    Console.WriteLine("2. Book a Spot");
                    Console.WriteLine("3. Check My Reservation");
                    Console.WriteLine("4. Release My Spot");
                    Console.WriteLine("5. Exit");
                    Console.Write("Enter your choice: ");
                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        writer.WriteLine("VIEW");
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Enter number of hours to book: ");
                        string hours = Console.ReadLine();
                        writer.WriteLine("BOOK " + hours);
                    }
                    else if (choice == "3")
                    {
                        writer.WriteLine("CHECK");
                    }
                    else if (choice == "4")
                    {
                        writer.WriteLine("RELEASE");
                    }
                    else if (choice == "5")
                    {
                        writer.WriteLine("EXIT");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Try again.");
                    }

                    Thread.Sleep(300);  // Slight delay for readability
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client Error: " + ex.Message);
            }
        }
    }
}
