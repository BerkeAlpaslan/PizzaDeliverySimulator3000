using PizzaDeliverySimulator.Common;
using PizzaDeliverySimulator.Driver.Models;
using PizzaDeliverySimulator.Driver.Services;

namespace PizzaDeliverySimulator.Driver.Controllers
{
    /// <summary>
    /// Handles order-related operations and state transitions
    /// </summary>
    public class OrderHandler
    {
        private DriverState state;
        private NetworkManager network;
        private MovementController movement;

        // Events for UI updates
        public event Action<string> OnOrderUpdate;
        public event Action<OrderInfo> OnOrderAssigned;
        public event Action<DeliveryStats> OnDeliveryCompleted;

        public OrderHandler(DriverState driverState, NetworkManager networkManager, MovementController movementController)
        {
            state = driverState;
            network = networkManager;
            movement = movementController;
        }

        /// <summary>
        /// Handle order assignment from server
        /// </summary>
        public void HandleOrderAssignment(string message)
        {
            // Parse: ASSIGN:OrderId:PizzaType:Address:CustomerX:CustomerY
            string[] parts = message.Split(':');

            if (parts.Length >= 6)
            {
                state.CurrentOrderId = parts[1];
                string pizzaType = parts[2];
                string address = parts[3];
                state.TargetX = int.Parse(parts[4]);
                state.TargetY = int.Parse(parts[5]);

                // Calculate distance
                double distance = movement.CalculateDistance(
                    state.DriverX, state.DriverY,
                    state.TargetX, state.TargetY
                );

                // Create order info
                OrderInfo orderInfo = new OrderInfo
                {
                    OrderId = state.CurrentOrderId,
                    PizzaType = pizzaType,
                    Address = address,
                    CustomerX = state.TargetX,
                    CustomerY = state.TargetY,
                    Distance = distance
                };

                // Notify UI
                OnOrderAssigned?.Invoke(orderInfo);

                OnOrderUpdate?.Invoke("[ORDER ASSIGNED]");
                OnOrderUpdate?.Invoke($"  Order ID: {state.CurrentOrderId}");
                OnOrderUpdate?.Invoke($"  Pizza: {pizzaType}");
                OnOrderUpdate?.Invoke($"  Address: {address}");
                OnOrderUpdate?.Invoke($"  Customer Location: ({state.TargetX}, {state.TargetY})");
                OnOrderUpdate?.Invoke($"  Distance: {distance:F1} units");
            }
        }

        /// <summary>
        /// Start delivery (OUTFORDELIVERY button clicked)
        /// </summary>
        public async void StartDelivery()
        {
            await network.SendCommandAsync($"{Protocol.OUTFORDELIVERY}:{state.CurrentOrderId}");

            movement.StartDeliveryMovement();

            OnOrderUpdate?.Invoke($"[INFO] Started delivery for order {state.CurrentOrderId}");
        }

        /// <summary>
        /// Complete delivery (DELIVERED button clicked)
        /// </summary>
        public async void CompleteDelivery()
        {
            string completedOrderId = state.CurrentOrderId;

            await network.SendCommandAsync($"{Protocol.DELIVERED}:{completedOrderId}");

            OnOrderUpdate?.Invoke($"[INFO] Order {completedOrderId} delivered");

            // Auto NOTREADY
            await network.SendCommandAsync($"{Protocol.NOTREADY}");

            // Clear order and start return to branch
            state.CurrentOrderId = "";
            movement.StartReturnToBranch();

            OnOrderUpdate?.Invoke($"[INFO] Returning to {state.BranchName}...");
            OnOrderUpdate?.Invoke($"[INFO] READY button locked until arrival");
        }

        /// <summary>
        /// Handle satisfaction/completion notification
        /// </summary>
        public void HandleSatisfaction(string message)
        {
            // Parse: SATISFACTION:OrderId:Score:ActualTime:EstimatedTime
            string[] parts = message.Split(':');

            if (parts.Length >= 5)
            {
                string orderId = parts[1];
                int score = int.Parse(parts[2]);
                string actualTime = parts[3];
                string estimatedTime = parts[4];

                // Update statistics
                state.DeliveredCount++;
                state.SatisfactionScores.Add(score);

                // Create delivery stats
                DeliveryStats stats = new DeliveryStats
                {
                    OrderId = orderId,
                    Score = score,
                    ActualTime = int.Parse(actualTime),
                    EstimatedTime = int.Parse(estimatedTime),
                    TotalDelivered = state.DeliveredCount,
                    AverageRating = state.GetAverageRating()
                };

                // Notify UI
                OnDeliveryCompleted?.Invoke(stats);

                OnOrderUpdate?.Invoke($"[SUCCESS] Order {orderId} completed - Rating: {score}/5");
            }
        }
    }

    /// <summary>
    /// Order information data structure
    /// </summary>
    public class OrderInfo
    {
        public string OrderId { get; set; }
        public string PizzaType { get; set; }
        public string Address { get; set; }
        public int CustomerX { get; set; }
        public int CustomerY { get; set; }
        public double Distance { get; set; }
    }

    /// <summary>
    /// Delivery statistics data structure
    /// </summary>
    public class DeliveryStats
    {
        public string OrderId { get; set; }
        public int Score { get; set; }
        public int ActualTime { get; set; }
        public int EstimatedTime { get; set; }
        public int TotalDelivered { get; set; }
        public double AverageRating { get; set; }
    }
}
