namespace PizzaDeliverySimulator.Customer.Models
{
    /// <summary>
    /// Holds all customer state and data
    /// </summary>
    public class CustomerState
    {
        // Customer identity
        public string CustomerId { get; set; } = "";
        public string Name { get; set; } = "";
        public string CurrentOrderId { get; set; } = "";

        // Customer position (fixed)
        public int CustomerX { get; set; } = 0;
        public int CustomerY { get; set; } = 0;

        // Branch tracking
        public int BranchX { get; set; } = -1;
        public int BranchY { get; set; } = -1;
        public string BranchId { get; set; } = "";
        public string BranchName { get; set; } = "";

        // Driver tracking (during delivery)
        public int DriverX { get; set; } = -1;  // -1 = no driver
        public int DriverY { get; set; } = -1;
        public string DriverName { get; set; } = "";
        public string DriverId { get; set; } = "";

        // State flags
        public bool IsRegistered { get; set; } = false;
        public bool IsDriverEnRoute { get; set; } = false;
        public bool HasActiveOrder { get; set; } = false;

        // Map Trail
        public List<Point> DriverMovementTrail { get; set; } = new List<Point>();

        /// <summary>
        /// Reset after delivery completion
        /// </summary>
        public void ResetOrderState()
        {
            CurrentOrderId = "";
            DriverX = -1;
            DriverY = -1;
            DriverName = "";
            DriverId = "";
            IsDriverEnRoute = false;
            HasActiveOrder = false;
            BranchX = -1;
            BranchY = -1;
            BranchId = "";
            BranchName = "";
            DriverMovementTrail.Clear();
        }
    }
}
