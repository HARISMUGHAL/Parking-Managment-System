PARKING ZONE MANAGEMENT SYSTEM (C# TCP/IP)
==========================================

PROJECT OVERVIEW
----------------
This is a TCP/IP-based Parking Zone Management System implemented in C#. 
The system supports up to 100 parking spots and allows clients to:
- View available parking spots
- Book a spot for a specified number of hours
- Check their current reservation
- Release a reserved spot
- Receive real-time notifications (10 minutes before expiry & after expiry)

The project demonstrates:
- Multithreading
- Client-server communication
- Real-time notifications
- Synchronized access to shared resources
- Socket programming in C#

TECHNOLOGIES USED
-----------------
- C#
- .NET Console Applications
- TCP/IP (System.Net.Sockets)
- Multithreading (System.Threading)
- File I/O and Streams

PROJECT STRUCTURE
-----------------
parking_zone_serverside/
    Program.cs        --> TCP server logic
    ParkingSpot.cs    --> Spot reservation logic, command handling

parking_zone_client/
    Program.cs        --> TCP client that interacts with the server

HOW IT WORKS
------------
SERVER SIDE:
- Creates and manages 100 parking spots.
- Listens on 127.0.0.1:3333.
- Handles each client on a separate thread.
- Monitors reservations and sends alerts:
    • 10 minutes before expiry
    • Upon expiry (spot is released)

CLIENT SIDE:
- Connects to the server.
- User enters their name and interacts using menu:
    • View available spots
    • Book a spot
    • Check reservation
    • Release reservation
    • Exit
- Background thread receives notifications from server.

HOW TO RUN
----------
REQUIREMENTS:
- .NET SDK
- Any C# IDE or terminal

STEPS:
1. Run the server project (parking_zone_serverside).
2. Run one or more instances of the client project (parking_zone_client).
3. Enter unique client names and interact with the system.

FEATURES
--------
✓ 100 parking spot management  
✓ Multi-client support  
✓ Reservation with custom hours  
✓ Real-time alerts  
✓ Thread-safe operations  
✓ TCP/IP communication  
✓ Graceful disconnect

COMMANDS SUMMARY
----------------
VIEW       - Show number of available spots  
BOOK X     - Book a spot for X hours  
CHECK      - Show your current reservation  
RELEASE    - Release your spot  
EXIT       - Disconnect from server  

AUTHOR
------
Haris Mughal  
AI Student  
AI, Networks, & Systems enthusiast

NOTES
-----
- Each client must use a unique name.
- Notification checks run every 60 seconds.
- Designed for academic/demo purposes.

LICENSE
-------
This project is free to use for academic and learning purposes.
