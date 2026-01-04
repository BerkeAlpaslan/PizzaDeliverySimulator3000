using PizzaDeliverySimulator.Common;
using PizzaDeliverySimulator.Customer.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PizzaDeliverySimulator.Customer.Services
{
    /// <summary>
    /// Manages network communication with server
    /// </summary>
    public class NetworkManager
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private CancellationTokenSource cts;

        // UDP listener for driver location updates
        private UdpClient udpClient;
        private CancellationTokenSource udpCts;

        private CustomerState state;

        // Events for UI updates
        public event Action<string> OnMessageReceived;
        public event Action<string> OnCustomerRegistered;
        public event Action<string> OnOrderCreated;
        public event Action<string> OnDriverAssigned;
        public event Action<string> OnDriverLocationUpdate;
        public event Action<string> OnDriverArrived;
        public event Action<string> OnError;
        public event Action<string> OnSatisfactionReceived;

        public NetworkManager(CustomerState customerState)
        {
            state = customerState;
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        public async Task<bool> ConnectAsync(string host = Protocol.DEFAULT_HOST, int port = Protocol.TCP_PORT)
        {
            try
            {
                OnMessageReceived?.Invoke($"Connecting to server at {Protocol.DEFAULT_HOST}:{Protocol.TCP_PORT}...");

                client = new TcpClient();
                await client.ConnectAsync(host, port);

                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.ASCII);
                writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                OnMessageReceived?.Invoke("Connected to server!");
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connection failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send command to server
        /// </summary>
        public async Task SendCommandAsync(string command)
        {
            try
            {
                if (writer != null)
                {
                    await writer.WriteLineAsync(command);
                    OnMessageReceived?.Invoke($">> {command}");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Send error: {ex.Message}");
            }
        }

        /// <summary>
        /// Start receiving TCP messages in background
        /// </summary>
        public async Task StartReceivingAsync()
        {
            cts = new CancellationTokenSource();

            try
            {
                while (!cts.Token.IsCancellationRequested && client.Connected)
                {
                    string message = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(message))
                    {
                        OnMessageReceived?.Invoke("[WARNING] Server disconnected");
                        break;
                    }

                    ProcessMessage(message);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Receive error: {ex.Message}");
            }
        }

        /// <summary>
        /// Start UDP listener for driver location updates
        /// </summary>
        public void StartUdpListener(int port = Protocol.UDP_PORT)
        {
            try
            {
                udpClient = new UdpClient();
                // CRITICAL: Allow multiple clients on same port
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.ExclusiveAddressUse = false;
                // Create UDP endpoint with address reuse enabled
                IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, port);
                udpClient.Client.Bind(localEndpoint);

                udpCts = new CancellationTokenSource();

                OnMessageReceived?.Invoke($"[INFO] UDP listener started on port {port}");

                _ = UdpReceiveLoop();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"UDP start error: {ex.Message}");
            }
        }

        /// <summary>
        /// UDP receive loop
        /// </summary>
        private async Task UdpReceiveLoop()
        {
            try
            {
                while (!udpCts.Token.IsCancellationRequested)
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    string message = Encoding.ASCII.GetString(result.Buffer);

                    ProcessUdpMessage(message);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"UDP receive error: {ex.Message}");
            }
        }

        /// <summary>
        /// Process incoming TCP message from server
        /// </summary>
        private void ProcessMessage(string message)
        {
            OnMessageReceived?.Invoke($"<< {message}");

            if (message.StartsWith($"{Protocol.REGISTERED}:"))
            {
                OnCustomerRegistered?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.ORDER_CREATED}:"))
            {
                OnOrderCreated?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.DRIVER_ASSIGNED}:"))
            {
                OnDriverAssigned?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.ESTIMATED}:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    OnMessageReceived?.Invoke($"[INFO] Estimated delivery time: {parts[2]} seconds");
                }
            }
            else if (message.StartsWith($"{Protocol.DRIVER_ARRIVED}:"))
            {
                OnDriverArrived?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.SATISFACTION}:"))
            {
                OnSatisfactionReceived?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.STATUS_UPDATE}:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    string statusPart = parts[2];

                    // Enable tracking when status contains "OutForDelivery"
                    if (statusPart.Contains($"OutForDelivery"))
                    {
                        state.IsDriverEnRoute = true;
                        OnMessageReceived?.Invoke($"[INFO] Driver is on the way!");
                    }

                    OnMessageReceived?.Invoke($"[STATUS] {statusPart}");
                }
            }
            else if (message.StartsWith($"{Protocol.ACCEPTED}:"))
            {
                OnMessageReceived?.Invoke("[INFO] Command accepted by server");
            }
            else if (message.StartsWith($"{Protocol.ERROR}:"))
            {
                OnError?.Invoke(message.Substring(6));
            }
        }

        /// <summary>
        /// Process UDP location update
        /// </summary>
        private void ProcessUdpMessage(string message)
        {
            // Parse: UDP_LOCATION:DriverId:X:Y:DriverName:OrderId
            if (message.StartsWith("UDP_LOCATION:"))
            {
                string[] parts = message.Split(':');

                if (parts.Length >= 6)
                {
                    string orderId = parts[5];

                    // Only process if this is OUR order
                    if (!string.IsNullOrEmpty(state.CurrentOrderId) &&
                        orderId == state.CurrentOrderId &&
                        state.IsDriverEnRoute)
                    {
                        OnDriverLocationUpdate?.Invoke(message);
                    }
                }
            }
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Disconnect()
        {
            try
            {
                cts?.Cancel();
                udpCts?.Cancel();

                reader?.Close();
                writer?.Close();
                stream?.Close();
                client?.Close();
                udpClient?.Close();

                OnMessageReceived?.Invoke("[INFO] Disconnected from server");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
