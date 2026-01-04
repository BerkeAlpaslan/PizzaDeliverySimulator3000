using PizzaDeliverySimulator.Server.Models;
using PizzaDeliverySimulator.Server.Utils;
using PizzaDeliverySimulator.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PizzaDeliverySimulator.Server.Services
{
    /// <summary>
    /// Broadcasts driver locations via UDP to all listening clients
    /// </summary>
    public class LocationBroadcaster
    {
        private UdpClient udpClient;
        private IPEndPoint broadcastEndpoint;
        private Thread broadcastThread;
        private bool isRunning;
        private ServerState state;

        public LocationBroadcaster()
        {
            state = ServerState.Instance;

            // Create UDP client for broadcasting
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            // Broadcast to all on UDP port
            broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, Protocol.UDP_PORT);

            isRunning = false;
        }

        /// <summary>
        /// Start broadcasting driver locations
        /// </summary>
        public void Start()
        {
            if (isRunning) return;

            isRunning = true;
            broadcastThread = new Thread(BroadcastLoop);
            broadcastThread.IsBackground = true;
            broadcastThread.Start();

            Logger.Success($"UDP Location Broadcaster started on port {Protocol.UDP_PORT}");
        }

        /// <summary>
        /// Stop broadcasting
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            broadcastThread?.Join(1000); // Wait max 1 second
            udpClient?.Close();

            Logger.Warning("UDP Location Broadcaster stopped");
        }

        /// <summary>
        /// Main broadcast loop (runs in background thread)
        /// </summary>
        private void BroadcastLoop()
        {
            Logger.Info("Location broadcast loop started");

            while (isRunning)
            {
                try
                {
                    // Broadcast all driver locations
                    BroadcastAllDriverLocations();

                    // Wait 2 seconds before next broadcast
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Broadcast error: {ex.Message}");
                }
            }

            Logger.Info("Location broadcast loop stopped");
        }

        /// <summary>
        /// Broadcast locations of all active drivers
        /// </summary>
        private void BroadcastAllDriverLocations()
        {
            foreach (var driver in state.Drivers.Values)
            {
                // Only broadcast if driver has active order (is delivering)
                if (driver.HasActiveOrder())
                {
                    BroadcastDriverLocation(driver);
                }
            }
        }

        /// <summary>
        /// Broadcast single driver location via UDP
        /// </summary>
        public void BroadcastDriverLocation(Driver driver)
        {
            try
            {
                // Format: UDP_LOCATION:DriverId:X:Y:DriverName:OrderId
                string message = $"{Protocol.UDP_LOCATION}{Protocol.DELIMITER}" +
                                $"{driver.DriverId}{Protocol.DELIMITER}" +
                                $"{driver.X}{Protocol.DELIMITER}" +
                                $"{driver.Y}{Protocol.DELIMITER}" +
                                $"{driver.Name}{Protocol.DELIMITER}" +
                                $"{driver.CurrentOrderId}";

                byte[] data = Encoding.ASCII.GetBytes(message);

                // Send UDP broadcast
                udpClient.SendAsync(data, data.Length, broadcastEndpoint);

                // Log occasionally (every 5th broadcast to avoid spam)
                if (DateTime.Now.Second % 10 == 0)
                {
                    Logger.Info($"Broadcasting: {driver.Name} at ({driver.X}, {driver.Y})");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to broadcast location: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if broadcaster is running
        /// </summary>
        public bool IsRunning()
        {
            return isRunning;
        }
    }
}
