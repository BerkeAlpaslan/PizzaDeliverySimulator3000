using System.Net.Sockets;

namespace PizzaDeliverySimulator.Server.Models
{
    /// <summary>
    /// Customer model
    /// </summary>
    public class Customer
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public int OrderCount { get; set; }
        public string CurrentOrderId { get; set; }
        public DateTime ConnectedTime { get; set; }

        // Customer location (50x50 grid)
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Customer(string name, TcpClient client)
        {
            CustomerId = "CUST" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            Name = name;
            Client = client;
            OrderCount = 0;
            CurrentOrderId = null;
            ConnectedTime = DateTime.Now;

            // Random location on 50x50 grid
            Random random = new Random(Guid.NewGuid().GetHashCode());
            X = random.Next(0, 51);  // 0-50
            Y = random.Next(0, 51);  // 0-50
        }

        /// <summary>
        /// Check if customer has an active order
        /// </summary>
        public bool HasActiveOrder()
        {
            return !string.IsNullOrEmpty(CurrentOrderId);
        }

        /// <summary>
        /// Place a new order
        /// </summary>
        public void PlaceOrder(string orderId)
        {
            CurrentOrderId = orderId;
            OrderCount++;
        }

        /// <summary>
        /// Complete order
        /// </summary>
        public void CompleteOrder()
        {
            CurrentOrderId = null;
        }

        public override string ToString()
        {
            string status = HasActiveOrder() ? $"Order: {CurrentOrderId}" : "No active order";
            return $"[{CustomerId}] {Name} - {status} (Total Orders: {OrderCount})";
        }
    }
}
