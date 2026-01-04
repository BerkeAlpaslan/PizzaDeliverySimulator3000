using PizzaDeliverySimulator.Server.Services;
using PizzaDeliverySimulator.Server.Utils;
using PizzaDeliverySimulator.Common;
using System.Net;
using System.Net.Sockets;

namespace PizzaDeliverySimulator.Server.Core
{
    /// <summary>
    /// Main server core - handles TCP listener and client connections
    /// </summary>
    public class ServerCore
    {
        private TcpListener tcpListener;
        private LocationBroadcaster locationBroadcaster;
        private bool isRunning;

        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            try
            {
                Logger.Header("PIZZA DELIVERY SERVER");

                // Create TCP listener on port 9050
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Protocol.TCP_PORT);
                tcpListener = new TcpListener(endpoint);
                tcpListener.Start();
                isRunning = true;

                Logger.Success($"TCP Server started on port {Protocol.TCP_PORT}");

                // Start UDP location broadcaster
                locationBroadcaster = new LocationBroadcaster();
                locationBroadcaster.Start();

                Logger.Info("Waiting for clients to connect...");
                Logger.Separator();

                // Begin accepting first client (async)
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), tcpListener);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to start server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Callback when a client connects (async)
        /// </summary>
        private void OnClientConnect(IAsyncResult ar)
        {
            try
            {
                // Get the listener from async state
                TcpListener listener = (TcpListener)ar.AsyncState;

                // Accept the client connection
                TcpClient client = listener.EndAcceptTcpClient(ar);

                // Get client endpoint info
                IPEndPoint clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                Logger.Info($"Client connected: {clientEndpoint.Address}:{clientEndpoint.Port}");

                // Create handler for this client
                ClientHandler handler = new ClientHandler(client);
                handler.Start();

                // IMPORTANT: Begin accepting next client immediately
                // This allows multiple clients to connect
                if (isRunning)
                {
                    listener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), listener);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error accepting client: {ex.Message}");

                // Continue accepting even if one connection fails
                if (isRunning)
                {
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), tcpListener);
                }
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            locationBroadcaster?.Stop();
            tcpListener?.Stop();
            Logger.Warning("Server stopped");
        }

        /// <summary>
        /// Check if server is running
        /// </summary>
        public bool IsRunning()
        {
            return isRunning;
        }
    }
}
