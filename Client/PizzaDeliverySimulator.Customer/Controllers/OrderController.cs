using PizzaDeliverySimulator.Common;
using PizzaDeliverySimulator.Customer.Models;
using PizzaDeliverySimulator.Customer.Services;

namespace PizzaDeliverySimulator.Customer.Controllers
{
    /// <summary>
    /// Handles order creation and tracking
    /// </summary>
    public class OrderController
    {
        private CustomerState state;
        private NetworkManager network;

        // Events for UI updates
        public event Action<string> OnOrderUpdate;
        public event Action<OrderCreatedInfo> OnOrderCreated;
        public event Action<DriverAssignedInfo> OnDriverAssigned;
        public event Action<SatisfactionInfo> OnSatisfactionReceived;

        public OrderController(CustomerState customerState, NetworkManager networkManager)
        {
            state = customerState;
            network = networkManager;
        }

        /// <summary>
        /// Create new order
        /// </summary>
        public async void CreateOrder(string pizzaType, string address)
        {
            // Validate
            if (state.HasActiveOrder)
            {
                throw new InvalidOperationException("Already have active order");
            }

            // Send ORDER command
            await network.SendCommandAsync($"{Protocol.ORDER}:{pizzaType}:{address}");

            OnOrderUpdate?.Invoke($"[INFO] Ordering {pizzaType} to {address}...");
        }

        /// <summary>
        /// Handle order creation response
        /// </summary>
        public void HandleOrderCreated(string message)
        {
            // Parse: ORDER_CREATED:ORD12345678
            string[] parts = message.Split(':');

            if (parts.Length >= 2)
            {
                state.CurrentOrderId = parts[1];
                state.HasActiveOrder = true;

                OrderCreatedInfo info = new OrderCreatedInfo
                {
                    OrderId = state.CurrentOrderId
                };

                OnOrderCreated?.Invoke(info);
                OnOrderUpdate?.Invoke($"[SUCCESS] Order created: {state.CurrentOrderId}");
            }
        }

        /// <summary>
        /// Handle driver assignment
        /// </summary>
        public void HandleDriverAssignment(string message)
        {
            // Parse: DRIVER_ASSIGNED:OrderId:DriverName:BranchId:BranchName:BranchX:BranchY
            string[] parts = message.Split(':');

            if (parts.Length >= 7)
            {
                string orderId = parts[1];
                string driverName = parts[2];
                string branchId = parts[3];
                string branchName = parts[4].Replace("_", " ");
                int branchX = int.Parse(parts[5]);
                int branchY = int.Parse(parts[6]);

                // Update state
                state.DriverName = driverName;
                state.BranchId = branchId;
                state.BranchName = branchName;
                state.BranchX = branchX;
                state.BranchY = branchY;
                state.IsDriverEnRoute = false;  // Not en route yet

                // Create info
                DriverAssignedInfo info = new DriverAssignedInfo
                {
                    OrderId = orderId,
                    DriverName = driverName,
                    BranchName = branchName,
                    BranchX = branchX,
                    BranchY = branchY
                };

                OnDriverAssigned?.Invoke(info);

                OnOrderUpdate?.Invoke($"[INFO] Driver assigned: {driverName}");
                OnOrderUpdate?.Invoke($"[INFO] Driver from: {branchName}");
                OnOrderUpdate?.Invoke($"[INFO] Waiting for driver to start delivery...");
            }
        }

        /// <summary>
        /// Handle satisfaction score
        /// </summary>
        public void HandleSatisfaction(string message)
        {
            // Parse: SATISFACTION:OrderId:Score:ActualTime:EstimatedTime
            string[] parts = message.Split(':');

            if (parts.Length >= 5)
            {
                string orderId = parts[1];
                int score = int.Parse(parts[2]);
                int actualTime = int.Parse(parts[3]);
                int estimatedTime = int.Parse(parts[4]);

                // Create info
                SatisfactionInfo info = new SatisfactionInfo
                {
                    OrderId = orderId,
                    Score = score,
                    ActualTime = actualTime,
                    EstimatedTime = estimatedTime
                };

                // Reset state
                state.ResetOrderState();

                OnSatisfactionReceived?.Invoke(info);

                OnOrderUpdate?.Invoke($"[SUCCESS] Delivery completed! Rating: {score}/5");
                OnOrderUpdate?.Invoke($"[INFO] You can place a new order");
            }
        }
    }

    /// <summary>
    /// Order created information
    /// </summary>
    public class OrderCreatedInfo
    {
        public string OrderId { get; set; }
    }

    /// <summary>
    /// Driver assigned information
    /// </summary>
    public class DriverAssignedInfo
    {
        public string OrderId { get; set; }
        public string DriverName { get; set; }
        public string BranchName { get; set; }
        public int BranchX { get; set; }
        public int BranchY { get; set; }
    }

    /// <summary>
    /// Satisfaction information
    /// </summary>
    public class SatisfactionInfo
    {
        public string OrderId { get; set; }
        public int Score { get; set; }
        public int ActualTime { get; set; }
        public int EstimatedTime { get; set; }
    }
}
