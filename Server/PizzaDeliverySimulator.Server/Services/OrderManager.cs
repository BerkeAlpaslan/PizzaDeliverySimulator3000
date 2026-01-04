using PizzaDeliverySimulator.Server.Models;
using PizzaDeliverySimulator.Server.Utils;
using PizzaDeliverySimulator.Common;
using System.Net.Sockets;
using System.Text;

namespace PizzaDeliverySimulator.Server.Services
{
    /// <summary>
    /// Manages order lifecycle: creation, assignment, status updates
    /// </summary>
    public class OrderManager
    {
        private ServerState state;

        public OrderManager()
        {
            state = ServerState.Instance;
        }

        /// <summary>
        /// Create new order from customer
        /// </summary>
        public Order CreateOrder(string customerId, string pizzaType, string address)
        {
            Customer customer = state.GetCustomer(customerId);
            if (customer == null)
            {
                Logger.Error($"Customer not found: {customerId}");
                return null;
            }

            // Create order
            Random random = new Random();
            Order order = new Order
            {
                PizzaType = pizzaType,
                Address = address,
                CustomerId = customerId,
                Status = OrderStatus.Pending,
                // Assign random customer location on 50x50 grid
                CustomerX = customer.X,  // 0-50
                CustomerY = customer.Y   // 0-50
            };

            // Add to state
            state.AddOrder(order);
            customer.PlaceOrder(order.OrderId);

            Logger.Success($"Order created: {order.OrderId} - {pizzaType} → {address} @ ({order.CustomerX}, {order.CustomerY})");

            // Quick transition to Preparing status (3-5 seconds)
            int initialDelay = random.Next(3000, 5001); // 3-5 seconds

            System.Threading.Tasks.Task.Delay(initialDelay).ContinueWith(_ =>
            {
                // Change status to Preparing
                order.Status = OrderStatus.Preparing;
                Logger.Info($"Order {order.OrderId} is now being prepared");

                // Notify customer that order is being prepared
                NotifyCustomerStatus(order, OrderStatus.Preparing);

                // Simulate actual preparation time (8-12 seconds)
                int preparingTime = random.Next(8000, 12001); // 8-12 seconds

                System.Threading.Tasks.Task.Delay(preparingTime).ContinueWith(__ =>
                {
                    Logger.Success($"Order {order.OrderId} is ready for pickup! (prepared in {preparingTime / 1000}s)");

                    // Notify customer order is ready, waiting for driver
                    NotifyCustomerOrderReady(order);

                    // Now try to assign to available driver
                    TryAssignOrder(order);
                });
            });

            return order;
        }

        /// <summary>
        /// Try to assign order to available driver
        /// </summary>
        public bool TryAssignOrder(Order order)
        {
            // Only assign orders that are in Preparing status
            if (order.Status != OrderStatus.Preparing)
            {
                Logger.Warning($"Cannot assign order {order.OrderId} - not in Preparing status (current: {order.Status})");
                return false;
            }

            // Get available driver
            Driver driver = state.GetAvailableDriver();

            if (driver == null)
            {
                Logger.Warning($"No available drivers for order {order.OrderId}");

                // Notify customer that order is waiting for driver
                NotifyCustomerNoDriver(order);

                return false;
            }

            // Assign order to driver (status stays Preparing)
            order.DriverId = driver.DriverId;
            driver.AssignOrder(order.OrderId);

            // Calculate estimated delivery time based on distance
            order.EstimatedDeliverySeconds = order.CalculateEstimatedTime(driver.X, driver.Y);

            Logger.Success($"Order {order.OrderId} assigned to {driver.Name} ({driver.DriverId}) - ETA: {order.EstimatedDeliverySeconds}s");

            // Notify driver about assignment
            NotifyDriverAssignment(driver, order);

            // Notify customer that driver has been assigned (NOT estimated time yet)
            NotifyCustomerDriverAssigned(order, driver);

            // Driver will manually set OutForDelivery when picking up order
            // ESTIMATED time will be sent when driver starts delivery

            return true;
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public void UpdateOrderStatus(string orderId, OrderStatus newStatus)
        {
            Order order = state.GetOrder(orderId);
            if (order == null)
            {
                Logger.Error($"Order not found: {orderId}");
                return;
            }

            order.Status = newStatus;
            Logger.Info($"Order {orderId} status updated: {newStatus}");

            // Notify customer
            NotifyCustomerStatus(order, newStatus);
        }

        /// <summary>
        /// Mark order as delivered
        /// </summary>
        public void CompleteOrder(string orderId)
        {
            Order order = state.GetOrder(orderId);
            if (order == null)
            {
                Logger.Error($"Order not found: {orderId}");
                return;
            }

            // Update order
            order.Status = OrderStatus.Delivered;
            order.DeliveryTime = DateTime.Now;

            // Get driver and customer
            Driver driver = state.GetDriver(order.DriverId);
            Customer customer = state.GetCustomer(order.CustomerId);

            if (driver != null)
            {
                driver.CompleteDelivery();
                Logger.Success($"{driver.Name} completed delivery of {orderId}");

                // Try to assign next pending order if any
                var pendingOrder = FindPendingOrder();
                if (pendingOrder != null)
                {
                    TryAssignOrder(pendingOrder);
                }
            }

            if (customer != null)
            {
                customer.CompleteOrder();
            }

            // Calculate satisfaction
            int satisfaction = order.GetSatisfactionScore();
            int deliveryTime = order.GetDeliveryTimeSeconds();

            Logger.Success($"Order {orderId} delivered! Time: {deliveryTime}s (Est: {order.EstimatedDeliverySeconds}s) | Satisfaction: {satisfaction}/5");

            // Notify customer about delivery status
            NotifyCustomerStatus(order, OrderStatus.Delivered);

            // Notify customer about satisfaction
            NotifyCustomerSatisfaction(order, satisfaction, deliveryTime);

            // Notify driver about satisfaction
            NotifyDriverSatisfaction(order, satisfaction, deliveryTime);
        }

        /// <summary>
        /// Find first pending order (waiting for driver)
        /// </summary>
        private Order FindPendingOrder()
        {
            foreach (var order in state.Orders.Values)
            {
                if (order.Status == OrderStatus.Pending)
                    return order;
            }
            return null;
        }

        /// <summary>
        /// Notify driver about order assignment
        /// </summary>
        private void NotifyDriverAssignment(Driver driver, Order order)
        {
            try
            {
                if (driver.Client != null && driver.Client.Connected)
                {
                    // Include customer coordinates in ASSIGN message
                    string message = $"{Protocol.ASSIGN}{Protocol.DELIMITER}" +
                                    $"{order.OrderId}{Protocol.DELIMITER}" +
                                    $"{order.PizzaType}{Protocol.DELIMITER}" +
                                    $"{order.Address}{Protocol.DELIMITER}" +
                                    $"{order.CustomerX}{Protocol.DELIMITER}" +
                                    $"{order.CustomerY}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = driver.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified driver {driver.DriverId} about order {order.OrderId} → ({order.CustomerX}, {order.CustomerY})");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify driver: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer about order status
        /// </summary>
        private void NotifyCustomerStatus(Order order, OrderStatus status)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    string statusStr = status.ToString();
                    string message = $"{Protocol.STATUS_UPDATE}{Protocol.DELIMITER}{order.OrderId}{Protocol.DELIMITER}{statusStr}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} about order {order.OrderId} status: {statusStr}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer when no driver is available
        /// </summary>
        private void NotifyCustomerNoDriver(Order order)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    string message = $"{Protocol.STATUS_UPDATE}{Protocol.DELIMITER}{order.OrderId}{Protocol.DELIMITER}WaitingForDriver\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} that order {order.OrderId} is waiting for driver");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer that order is ready for pickup, waiting for driver
        /// </summary>
        private void NotifyCustomerOrderReady(Order order)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    string message = $"{Protocol.STATUS_UPDATE}{Protocol.DELIMITER}{order.OrderId}{Protocol.DELIMITER}ReadyForPickup\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} that order {order.OrderId} is ready for pickup");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer about estimated delivery time
        /// </summary>
        public void NotifyCustomerEstimatedTime(Order order)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    string message = $"{Protocol.ESTIMATED}{Protocol.DELIMITER}{order.OrderId}{Protocol.DELIMITER}{order.EstimatedDeliverySeconds}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} - Estimated delivery: {order.EstimatedDeliverySeconds}s");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer that a driver has been assigned
        /// </summary>
        private void NotifyCustomerDriverAssigned(Order order, Driver driver)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    string message = $"{Protocol.DRIVER_ASSIGNED}{Protocol.DELIMITER}" +
                                        $"{order.OrderId}{Protocol.DELIMITER}" +
                                        $"{driver.Name}{Protocol.DELIMITER}" +
                                        $"{driver.AssignedBranch.BranchId}{Protocol.DELIMITER}" +
                                        $"{driver.AssignedBranch.Name}{Protocol.DELIMITER}" +
                                        $"{driver.AssignedBranch.X}{Protocol.DELIMITER}" +
                                        $"{driver.AssignedBranch.Y}\n"; ;
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} that driver {driver.Name} was assigned");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer that driver started delivery with location and distance info
        /// </summary>
        public void NotifyCustomerOutForDelivery(Order order, Driver driver)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    // Calculate distance between driver and customer
                    double distance = Math.Sqrt(
                        Math.Pow(order.CustomerX - driver.X, 2) +
                        Math.Pow(order.CustomerY - driver.Y, 2)
                    );

                    // Format: STATUS_UPDATE:OrderId:OutForDelivery from Driver Name (X, Y) - Distance: D units
                    string statusText = $"OutForDelivery from Driver {driver.Name} ({driver.X}, {driver.Y}) - Distance: {distance:F1} units";
                    string message = $"{Protocol.STATUS_UPDATE}{Protocol.DELIMITER}" +
                                    $"{order.OrderId}{Protocol.DELIMITER}" +
                                    $"{statusText}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} - OutForDelivery from {driver.Name} @ ({driver.X}, {driver.Y})");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer that driver has arrived at their location
        /// </summary>
        public void NotifyCustomerArrived(Order order, Driver driver)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    // Format: DRIVER_ARRIVED:OrderId:DriverName:X:Y
                    string message = $"{Protocol.DRIVER_ARRIVED}{Protocol.DELIMITER}" +
                                    $"{order.OrderId}{Protocol.DELIMITER}" +
                                    $"{driver.Name}{Protocol.DELIMITER}" +
                                    $"{driver.X}{Protocol.DELIMITER}" +
                                    $"{driver.Y}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} - Driver {driver.Name} has arrived");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer arrival: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify customer about satisfaction score after delivery
        /// </summary>
        private void NotifyCustomerSatisfaction(Order order, int score, int actualTime)
        {
            try
            {
                Customer customer = state.GetCustomer(order.CustomerId);
                if (customer != null && customer.Client != null && customer.Client.Connected)
                {
                    // Format: SATISFACTION:OrderId:Score:Stars:ActualTime:EstimatedTime
                    string message = $"{Protocol.SATISFACTION}{Protocol.DELIMITER}" +
                                    $"{order.OrderId}{Protocol.DELIMITER}" +
                                    $"{score}{Protocol.DELIMITER}" +
                                    $"{actualTime}{Protocol.DELIMITER}" +
                                    $"{order.EstimatedDeliverySeconds}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = customer.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified customer {customer.CustomerId} - Satisfaction: {score}/5");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify customer satisfaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify driver about satisfaction score after delivery
        /// </summary>
        private void NotifyDriverSatisfaction(Order order, int score, int actualTime)
        {
            try
            {
                Driver driver = state.GetDriver(order.DriverId);
                if (driver != null && driver.Client != null && driver.Client.Connected)
                {
                    // Format: SATISFACTION:OrderId:Score:ActualTime:EstimatedTime
                    // NOTE: NO STARS/EMOJI for driver (cleaner)
                    string message = $"{Protocol.SATISFACTION}{Protocol.DELIMITER}" +
                                    $"{order.OrderId}{Protocol.DELIMITER}" +
                                    $"{score}{Protocol.DELIMITER}" +
                                    $"{actualTime}{Protocol.DELIMITER}" +
                                    $"{order.EstimatedDeliverySeconds}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);

                    NetworkStream stream = driver.Client.GetStream();
                    stream.Write(data, 0, data.Length);

                    Logger.Info($"Notified driver {driver.Name} - Satisfaction: {score}/5");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to notify driver satisfaction: {ex.Message}");
            }
        }
    }
}
