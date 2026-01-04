namespace PizzaDeliverySimulator.Server.Models
{
    /// <summary>
    /// Order status enum
    /// </summary>
    public enum OrderStatus
    {
        Pending,          // Order received, waiting for driver
        Preparing,        // Pizza being prepared
        OutForDelivery,   // Driver is delivering
        Delivered         // Order completed
    }

    /// <summary>
    /// Pizza order model
    /// </summary>
    public class Order
    {
        public string OrderId { get; set; }
        public string PizzaType { get; set; }
        public string Address { get; set; }
        public OrderStatus Status { get; set; }
        public string CustomerId { get; set; }
        public string DriverId { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? DeliveryTime { get; set; }

        // Customer location (50x50 grid)
        public int CustomerX { get; set; }
        public int CustomerY { get; set; }

        // Estimated delivery time (in seconds)
        public int EstimatedDeliverySeconds { get; set; }

        /// <summary>
        /// Constructor - generates unique OrderId
        /// </summary>
        public Order()
        {
            OrderId = "ORD" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            Status = OrderStatus.Pending;
            OrderTime = DateTime.Now;
        }

        /// <summary>
        /// Get delivery time in seconds
        /// </summary>
        public int GetDeliveryTimeSeconds()
        {
            if (DeliveryTime.HasValue)
                return (int)(DeliveryTime.Value - OrderTime).TotalSeconds;
            return 0;
        }

        /// <summary>
        /// Calculate customer satisfaction (1-5)
        /// Based on actual vs estimated delivery time
        /// </summary>
        public int GetSatisfactionScore()
        {
            // If no estimated time, use old scoring
            if (EstimatedDeliverySeconds == 0)
            {
                int deliveryTime = GetDeliveryTimeSeconds();
                if (deliveryTime < 30) return 5;
                if (deliveryTime < 60) return 4;
                if (deliveryTime < 90) return 3;
                if (deliveryTime < 120) return 2;
                return 1;
            }

            // Calculate ratio: actual / estimated
            int actualTime = GetDeliveryTimeSeconds();
            double ratio = (double)actualTime / EstimatedDeliverySeconds;

            // Score based on how close to estimated time
            if (ratio <= 1.1) return 5;  // ⭐⭐⭐⭐⭐ On time or early!
            if (ratio <= 1.3) return 4;  // ⭐⭐⭐⭐ Slightly late
            if (ratio <= 1.5) return 3;  // ⭐⭐⭐ Moderately late
            if (ratio <= 2.0) return 2;  // ⭐⭐ Late
            return 1;                    // ⭐ Very late
        }

        /// <summary>
        /// Calculate estimated delivery time based on distance
        /// Formula: distance * 2 seconds per unit
        /// </summary>
        public int CalculateEstimatedTime(int driverX, int driverY)
        {
            // Euclidean distance
            double distance = Math.Sqrt(
                Math.Pow(CustomerX - driverX, 2) +
                Math.Pow(CustomerY - driverY, 2)
            );

            // 2 seconds per unit distance (movement speed)
            return (int)(distance * 2);
        }

        public override string ToString()
        {
            return $"[{OrderId}] {PizzaType} → {Address} (Status: {Status})";
        }
    }
}
