using PizzaDeliverySimulator.Server.Models;
using PizzaDeliverySimulator.Server.Utils;
using PizzaDeliverySimulator.Common;
using System.Net.Sockets;
using System.Text;

namespace PizzaDeliverySimulator.Server.Services
{
    /// <summary>
    /// Handles individual client connections with async operations
    /// Updated to integrate with ServerState and OrderManager
    /// </summary>
    public class ClientHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private byte[] buffer;
        private bool isConnected;
        private string clientEndpoint;

        private ServerState state;
        private OrderManager orderManager;

        // Client type tracking
        private bool isDriver;
        private bool isCustomer;
        private string entityId; // DriverId or CustomerId

        /// <summary>
        /// Constructor
        /// </summary>
        public ClientHandler(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();

            // Initialize StreamReader/StreamWriter for message handling
            this.reader = new StreamReader(stream, Encoding.ASCII);
            this.writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            this.buffer = new byte[1024];
            this.isConnected = true;
            this.clientEndpoint = client.Client.RemoteEndPoint.ToString();

            this.state = ServerState.Instance;
            this.orderManager = new OrderManager();

            this.isDriver = false;
            this.isCustomer = false;
            this.entityId = null;
        }

        /// <summary>
        /// Start handling this client
        /// </summary>
        public void Start()
        {
            BeginReceive();
        }

        /// <summary>
        /// Begin async receive operation
        /// </summary>
        private void BeginReceive()
        {
            try
            {
                if (!isConnected) return;

                stream.BeginRead(buffer, 0, buffer.Length,
                    new AsyncCallback(OnReceive), null);
            }
            catch (Exception ex)
            {
                Logger.Error($"[{clientEndpoint}] BeginReceive error: {ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Callback when data is received (async)
        /// </summary>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int bytesRead = stream.EndRead(ar);

                if (bytesRead == 0)
                {
                    Logger.Warning($"[{clientEndpoint}] Client disconnected");
                    Disconnect();
                    return;
                }

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                // Don't log LOCATION messages to avoid spam
                if (!message.StartsWith($"{Protocol.LOCATION}:"))
                {
                    Logger.Info($"[{clientEndpoint}] Received: {message}");
                }

                ProcessMessage(message);

                if (isConnected)
                {
                    BeginReceive();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[{clientEndpoint}] OnReceive error: {ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Process received message
        /// </summary>
        private void ProcessMessage(string message)
        {
            try
            {
                string[] parts = message.Split(Protocol.DELIMITER);

                if (parts.Length == 0)
                {
                    SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Empty message");
                    return;
                }

                string command = parts[0];

                switch (command)
                {
                    case Protocol.REGISTER:
                        // Driver registration
                        if (parts.Length < 2)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Missing driver name");
                            return;
                        }
                        HandleDriverRegister(parts[1]);
                        break;

                    case Protocol.REGISTER_CUSTOMER:
                        // Customer registration
                        if (parts.Length < 2)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Missing customer name");
                            return;
                        }
                        HandleCustomerRegister(parts[1]);
                        break;

                    case Protocol.ORDER:
                        // Customer order - drivers cannot order
                        if (isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Drivers cannot place orders");
                            return;
                        }

                        if (parts.Length < 3)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Invalid order format (need: ORDER:PizzaType:Address)");
                            return;
                        }

                        // Check if customer is registered
                        if (!isCustomer)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Please register first: REGISTER_CUSTOMER:YourName");
                            return;
                        }

                        HandleCustomerOrder(parts[1], parts[2]);
                        break;

                    case Protocol.READY:
                        // Driver ready
                        if (!isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Not registered as driver");
                            return;
                        }
                        HandleDriverReady();
                        break;

                    case Protocol.NOTREADY:
                        // Driver not ready (pause/break)
                        if (!isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Not registered as driver");
                            return;
                        }
                        HandleDriverNotReady();
                        break;

                    case Protocol.OUTFORDELIVERY:
                        // Driver picked up order and going to deliver
                        if (!isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Not registered as driver");
                            return;
                        }
                        if (parts.Length < 2)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Missing order ID");
                            return;
                        }
                        HandleOutForDelivery(parts[1]);
                        break;

                    case Protocol.ARRIVED:
                        // Driver arrived at customer location
                        if (!isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Not registered as driver");
                            return;
                        }
                        if (parts.Length < 4)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Missing order ID");
                            return;
                        }
                        HandleArrived(parts[1], parts[2], parts[3]);
                        break;

                    case Protocol.DELIVERED:
                        // Driver delivered order
                        if (!isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Not registered as driver");
                            return;
                        }
                        if (parts.Length < 2)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Missing order ID");
                            return;
                        }
                        HandleOrderDelivered(parts[1]);
                        break;

                    case Protocol.LOCATION:
                        // Driver location update
                        if (!isDriver)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Not registered as driver");
                            return;
                        }
                        if (parts.Length < 3)
                        {
                            SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Invalid location format");
                            return;
                        }
                        HandleLocationUpdate(parts[1], parts[2]);
                        break;

                    case Protocol.DISCONNECT:
                        Logger.Info($"[{clientEndpoint}] Disconnect requested");
                        Disconnect();
                        break;

                    default:
                        Logger.Warning($"[{clientEndpoint}] Unknown command: {command}");
                        SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Unknown command: {command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[{clientEndpoint}] ProcessMessage error: {ex.Message}");
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Processing error");
            }
        }

        /// <summary>
        /// Handle driver registration
        /// </summary>
        private void HandleDriverRegister(string driverName)
        {
            Driver driver = new Driver(driverName, client);
            state.AddDriver(driver, clientEndpoint);

            isDriver = true;
            entityId = driver.DriverId;

            Logger.Success($"[{clientEndpoint}] Driver registered: {driverName} → {driver.DriverId} @ ({driver.X}, {driver.Y})");

            // Send driver ID and starting coordinates
            string message = $"{Protocol.REGISTERED}{Protocol.DELIMITER}" +
                $"{driver.DriverId}{Protocol.DELIMITER}" +
                $"{driver.X}{Protocol.DELIMITER}" +
                $"{driver.Y}{Protocol.DELIMITER}" +
                $"{driver.AssignedBranch.BranchId}{Protocol.DELIMITER}" +
                $"{driver.AssignedBranch.Name}";

            SendMessage(message);
        }

        /// <summary>
        /// Handle customer registration
        /// </summary>
        private void HandleCustomerRegister(string customerName)
        {
            Customer customer = new Customer(customerName, client);
            state.AddCustomer(customer, clientEndpoint);

            isCustomer = true;
            entityId = customer.CustomerId;

            Logger.Success($"[{clientEndpoint}] Customer registered: {customerName} → {customer.CustomerId} @ ({customer.X}, {customer.Y})");

            // Send customer ID and coordinates
            SendMessage($"{Protocol.REGISTERED}{Protocol.DELIMITER}{customer.CustomerId}{Protocol.DELIMITER}{customer.X}{Protocol.DELIMITER}{customer.Y}");
        }

        /// <summary>
        /// Handle customer order
        /// </summary>
        private void HandleCustomerOrder(string pizzaType, string address)
        {
            Customer customer = state.GetCustomerByEndpoint(clientEndpoint);
            if (customer == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Customer not registered");
                return;
            }

            Order order = orderManager.CreateOrder(customer.CustomerId, pizzaType, address);

            if (order != null)
            {
                Logger.Success($"[{clientEndpoint}] Order created: {order.OrderId}");
                SendMessage($"{Protocol.ORDER_CREATED}{Protocol.DELIMITER}{order.OrderId}");
            }
            else
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Failed to create order");
            }
        }

        /// <summary>
        /// Handle driver ready
        /// </summary>
        private void HandleDriverReady()
        {
            Driver driver = state.GetDriverByEndpoint(clientEndpoint);
            if (driver == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Driver not found");
                return;
            }

            driver.IsReady = true;
            Logger.Info($"[{clientEndpoint}] Driver {driver.Name} is ready");
            SendMessage($"{Protocol.ACCEPTED}");

            // Try to assign orders that are in Preparing status (restaurant finished preparing)
            foreach (var order in state.Orders.Values)
            {
                if (order.Status == OrderStatus.Preparing && string.IsNullOrEmpty(order.DriverId))
                {
                    orderManager.TryAssignOrder(order);
                    break; // Assign one at a time
                }
            }
        }

        /// <summary>
        /// Handle driver not ready (pause/break)
        /// </summary>
        private void HandleDriverNotReady()
        {
            Driver driver = state.GetDriverByEndpoint(clientEndpoint);
            if (driver == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Driver not found");
                return;
            }

            // Check if driver has active order
            if (driver.HasActiveOrder())
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Cannot pause while you have an active order. Complete delivery first.");
                Logger.Warning($"[{clientEndpoint}] Driver {driver.Name} tried to pause with active order {driver.CurrentOrderId}");
                return;
            }

            driver.IsReady = false;
            Logger.Info($"[{clientEndpoint}] Driver {driver.Name} is now on break (not ready)");
            SendMessage($"{Protocol.ACCEPTED}");
        }

        /// <summary>
        /// Handle driver starting delivery (picked up order)
        /// </summary>
        private void HandleOutForDelivery(string orderId)
        {
            Driver driver = state.GetDriverByEndpoint(clientEndpoint);
            if (driver == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Driver not found");
                return;
            }

            // Get the order
            Order order = state.GetOrder(orderId);
            if (order == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Order not found");
                return;
            }

            // Check if order is assigned to THIS driver
            if (order.DriverId != driver.DriverId)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}This order is not assigned to you");
                Logger.Warning($"[{clientEndpoint}] Driver {driver.Name} tried to start delivery for order {orderId} (not assigned to them)");
                return;
            }

            // Update order status
            order.Status = OrderStatus.OutForDelivery;

            // Notify customer with driver location and distance info
            orderManager.NotifyCustomerOutForDelivery(order, driver);

            // NOW send estimated delivery time to customer (driver is starting delivery)
            orderManager.NotifyCustomerEstimatedTime(order);

            Logger.Success($"[{clientEndpoint}] Driver {driver.Name} started delivery for {orderId}");
            SendMessage($"{Protocol.ACCEPTED}");
        }

        /// <summary>
        /// Handle driver arrived at customer location
        /// </summary>
        private void HandleArrived(string orderId, string xStr, string yStr)
        {
            Driver driver = state.GetDriverByEndpoint(clientEndpoint);
            if (driver == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Driver not found");
                return;
            }

            // Get the order
            Order order = state.GetOrder(orderId);
            if (order == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Order not found");
                return;
            }

            // Check if order is assigned to THIS driver
            if (order.DriverId != driver.DriverId)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}This order is not assigned to you");
                Logger.Warning($"[{clientEndpoint}] Driver {driver.Name} tried to mark arrived for order {orderId} (not assigned to them)");
                return;
            }

            // Update driver position to final arrival position
            if (int.TryParse(xStr, out int x) && int.TryParse(yStr, out int y))
            {
                driver.X = x;
                driver.Y = y;
            }

            // Set driver as arrived
            driver.IsArrived = true;

            // Notify customer that driver has arrived
            orderManager.NotifyCustomerArrived(order, driver);

            Logger.Success($"[{clientEndpoint}] Driver {driver.Name} arrived at customer for {orderId}");
            SendMessage($"{Protocol.ACCEPTED}");
        }

        /// <summary>
        /// Handle order delivered
        /// </summary>
        private void HandleOrderDelivered(string orderId)
        {
            Driver driver = state.GetDriverByEndpoint(clientEndpoint);
            if (driver == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Driver not found");
                return;
            }

            // Get the order
            Order order = state.GetOrder(orderId);
            if (order == null)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Order not found");
                return;
            }

            // Check if order is assigned to THIS driver
            if (order.DriverId != driver.DriverId)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}This order is not assigned to you");
                Logger.Warning($"[{clientEndpoint}] Driver {driver.Name} tried to deliver order {orderId} (not assigned to them)");
                return;
            }

            // Check if order is OutForDelivery (driver must call OUTFORDELIVERY first)
            if (order.Status != OrderStatus.OutForDelivery)
            {
                SendMessage($"{Protocol.ERROR}{Protocol.DELIMITER}Order must be OutForDelivery before marking as Delivered. Current status: {order.Status}");
                return;
            }

            orderManager.CompleteOrder(orderId);

            // Reset arrived flag
            driver.IsArrived = false;
            SendMessage($"{Protocol.ACCEPTED}");
        }

        /// <summary>
        /// Handle location update from driver
        /// </summary>
        private void HandleLocationUpdate(string xStr, string yStr)
        {
            Driver driver = state.GetDriverByEndpoint(clientEndpoint);
            if (driver == null) return;

            if (int.TryParse(xStr, out int x) && int.TryParse(yStr, out int y))
            {
                driver.X = x;
                driver.Y = y;
                // Location updates no longer logged to avoid console spam
            }
        }

        /// <summary>
        /// Send message to client using StreamWriter
        /// </summary>
        public void SendMessage(string message)
        {
            try
            {
                if (!isConnected) return;

                // Use StreamWriter.WriteLine() instead of byte array conversion
                writer.WriteLineAsync(message);
                // AutoFlush = true, so no need to call Flush()
            }
            catch (Exception ex)
            {
                Logger.Error($"[{clientEndpoint}] SendMessage error: {ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Disconnect client and cleanup
        /// </summary>
        private void Disconnect()
        {
            if (!isConnected) return;

            isConnected = false;

            try
            {
                // Remove from state
                if (isDriver && entityId != null)
                {
                    state.RemoveDriver(entityId);
                    Logger.Warning($"[{clientEndpoint}] Driver {entityId} removed");
                }
                else if (isCustomer && entityId != null)
                {
                    state.RemoveCustomer(entityId);
                    Logger.Warning($"[{clientEndpoint}] Customer {entityId} removed");
                }

                // Dispose StreamReader/Writer first (they close underlying stream)
                reader?.Close();
                writer?.Close();
                stream?.Close();
                client?.Close();
                Logger.Warning($"[{clientEndpoint}] Client disconnected and cleaned up");
            }
            catch (Exception ex)
            {
                Logger.Error($"[{clientEndpoint}] Disconnect error: {ex.Message}");
            }
        }
    }
}
