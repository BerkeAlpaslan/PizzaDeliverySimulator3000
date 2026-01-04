using PizzaDeliverySimulator.Customer.Models;

namespace PizzaDeliverySimulator.Customer.Controllers
{
    /// <summary>
    /// Handles driver location tracking (UDP updates)
    /// </summary>
    public class TrackingController
    {
        private CustomerState state;

        // Events for UI updates
        public event Action<string> OnTrackingUpdate;
        public event Action<DriverLocationInfo> OnDriverLocationUpdated;
        public event Action<DriverArrivalInfo> OnDriverArrived;

        public TrackingController(CustomerState customerState)
        {
            state = customerState;
        }

        /// <summary>
        /// Calculate distance between two points
        /// </summary>
        public double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(
                Math.Pow(x2 - x1, 2) +
                Math.Pow(y2 - y1, 2)
            );
        }

        /// <summary>
        /// Get distance from customer to driver
        /// </summary>
        public double GetDistanceToDriver()
        {
            if (state.DriverX < 0 || state.DriverY < 0)
                return -1;

            return CalculateDistance(
                state.CustomerX, state.CustomerY,
                state.DriverX, state.DriverY
            );
        }

        /// <summary>
        /// Handle driver location update (UDP)
        /// </summary>
        public void HandleLocationUpdate(string message)
        {
            // Parse: UDP_LOCATION:DriverId:X:Y:DriverName:OrderId
            string[] parts = message.Split(':');

            if (parts.Length >= 6)
            {
                string driverId = parts[1];
                int driverX = int.Parse(parts[2]);
                int driverY = int.Parse(parts[3]);
                string driverName = parts[4];
                string orderId = parts[5];

                // Update state
                state.DriverX = driverX;
                state.DriverY = driverY;
                state.DriverId = driverId;
                state.DriverName = driverName;
                state.IsDriverEnRoute = true;

                // Add to trail
                state.DriverMovementTrail.Add(new Point(driverX, driverY));

                // Calculate distance
                double distance = GetDistanceToDriver();

                // Create info
                DriverLocationInfo info = new DriverLocationInfo
                {
                    DriverId = driverId,
                    DriverName = driverName,
                    X = driverX,
                    Y = driverY,
                    Distance = distance,
                    OrderId = orderId
                };

                OnDriverLocationUpdated?.Invoke(info);
                OnTrackingUpdate?.Invoke($"[TRACKING] Driver at ({driverX}, {driverY}) - Distance: {distance:F1} units");
            }
        }

        /// <summary>
        /// Handle driver arrival
        /// </summary>
        public void HandleDriverArrival(string message)
        {
            // Parse: DRIVER_ARRIVED:OrderId:DriverName:X:Y
            string[] parts = message.Split(':');

            if (parts.Length >= 5)
            {
                string orderId = parts[1];
                string driverName = parts[2];
                int finalX = int.Parse(parts[3]);
                int finalY = int.Parse(parts[4]);

                // Update to final position
                state.DriverX = finalX;
                state.DriverY = finalY;

                // Create info
                DriverArrivalInfo info = new DriverArrivalInfo
                {
                    OrderId = orderId,
                    DriverName = driverName,
                    X = finalX,
                    Y = finalY
                };

                OnDriverArrived?.Invoke(info);
                OnTrackingUpdate?.Invoke($"[ARRIVAL] Driver {driverName} has arrived at ({finalX}, {finalY})!");
                OnTrackingUpdate?.Invoke($"[INFO] Waiting for delivery confirmation...");
            }
        }
    }

    /// <summary>
    /// Driver location information
    /// </summary>
    public class DriverLocationInfo
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public double Distance { get; set; }
        public string OrderId { get; set; }
    }

    /// <summary>
    /// Driver arrival information
    /// </summary>
    public class DriverArrivalInfo
    {
        public string OrderId { get; set; }
        public string DriverName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
