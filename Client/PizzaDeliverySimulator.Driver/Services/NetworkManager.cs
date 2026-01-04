using PizzaDeliverySimulator.Common;
using PizzaDeliverySimulator.Driver.Models;
using System.Net.Sockets;
using System.Text;

namespace PizzaDeliverySimulator.Driver.Services
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

        private DriverState state;

        // Events for UI updates
        public event Action<string> OnMessageReceived;
        public event Action<string> OnDriverRegistered;  // message
        public event Action<string> OnOrderAssigned;     // message
        public event Action<string> OnSatisfactionReceived; // message
        public event Action<string> OnError;

        public NetworkManager(DriverState driverState)
        {
            state = driverState;
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
        public async Task SendCommandAsync(string command, bool silent = false)
        {
            try
            {
                if (writer != null)
                {
                    await writer.WriteLineAsync(command);

                    // Don't show LOCATION commands (spam prevention)
                    if (!silent && !command.StartsWith($"{Protocol.LOCATION}:"))
                    {
                        OnMessageReceived?.Invoke($">> {command}");
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Send error: {ex.Message}");
            }
        }

        /// <summary>
        /// Start receiving messages in background
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
        /// Process incoming message from server
        /// </summary>
        private void ProcessMessage(string message)
        {
            OnMessageReceived?.Invoke($"<< {message}");

            if (message.StartsWith($"{Protocol.REGISTERED}:"))
            {
                // Parse extended format: REGISTERED:DRV123:X:Y:BranchId:BranchName
                OnDriverRegistered?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.ASSIGN}:"))
            {
                OnOrderAssigned?.Invoke(message);
            }
            else if (message.StartsWith($"{Protocol.ESTIMATED}:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    OnMessageReceived?.Invoke($"[INFO] Estimated delivery time: {parts[2]} seconds");
                }
            }
            else if (message.StartsWith($"{Protocol.SATISFACTION}:"))
            {
                OnSatisfactionReceived?.Invoke(message);
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
        /// Close all connections
        /// </summary>
        public void Disconnect()
        {
            try
            {
                cts?.Cancel();
                reader?.Close();
                writer?.Close();
                stream?.Close();
                client?.Close();

                OnMessageReceived?.Invoke("[INFO] Disconnected from server");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
