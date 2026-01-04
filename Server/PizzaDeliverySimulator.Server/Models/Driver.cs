using PizzaDeliverySimulator.Server.Utils;
using System.Net.Sockets;

namespace PizzaDeliverySimulator.Server.Models
{
    /// <summary>
    /// Delivery driver model
    /// </summary>
    public class Driver
    {
        public string DriverId { get; set; }
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public bool IsReady { get; set; }
        public string CurrentOrderId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int DeliveredCount { get; set; }

        // Flag for when driver has arrived at customer location
        public bool IsArrived { get; set; }

        public Branch AssignedBranch { get; set; }
        public DateTime ConnectedTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Driver(string name, TcpClient client)
        {
            DriverId = "DRV" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            Name = name;
            Client = client;
            IsReady = false;
            CurrentOrderId = null;
            DeliveredCount = 0;

            // Random starting position on 50x50 grid
            Branch assignedBranch = ServerState.GetRandomBranch();
            X = assignedBranch.X;
            Y = assignedBranch.Y;
            AssignedBranch = assignedBranch;

            Logger.Info($"Driver {DriverId} assigned to {assignedBranch.Name} @ ({X}, {Y})");

            ConnectedTime = DateTime.Now;
        }

        /// <summary>
        /// Check if driver has an active order
        /// </summary>
        public bool HasActiveOrder()
        {
            return !string.IsNullOrEmpty(CurrentOrderId);
        }

        /// <summary>
        /// Assign order to driver
        /// </summary>
        public void AssignOrder(string orderId)
        {
            CurrentOrderId = orderId;
            IsReady = false;
        }

        /// <summary>
        /// Complete delivery
        /// </summary>
        public void CompleteDelivery()
        {
            CurrentOrderId = null;
            DeliveredCount++;
            IsReady = true;
        }

        public override string ToString()
        {
            string status = HasActiveOrder() ? $"Delivering {CurrentOrderId}" : (IsReady ? "Ready" : "Not Ready");
            return $"[{DriverId}] {Name} - {status} (Deliveries: {DeliveredCount})";
        }
    }
}
