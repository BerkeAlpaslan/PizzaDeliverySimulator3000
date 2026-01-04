namespace PizzaDeliverySimulator.Driver.Models
{
    public class DriverState
    {
        // Driver identity
        public string DriverId { get; set; } = "";
        public string CurrentOrderId { get; set; } = "";

        // Positions
        public int DriverX { get; set; } = 0;
        public int DriverY { get; set; } = 0;
        public int TargetX { get; set; } = -1;  // -1 = no customer
        public int TargetY { get; set; } = -1;

        // State flags
        public bool IsRegistered { get; set; } = false;
        public bool IsMoving { get; set; } = false;

        // Movement trail
        public List<Point> MovementTrail { get; set; } = new List<Point>();

        // Branch information
        public int BranchX { get; set; } = -1;
        public int BranchY { get; set; } = -1;
        public string BranchId { get; set; } = "";
        public string BranchName { get; set; } = "";

        // Statistics
        public int DeliveredCount { get; set; } = 0;
        public List<int> SatisfactionScores { get; set; } = new List<int>();

        // Return to branch state
        public bool IsReturningToBranch { get; set; } = false;

        /// <summary>
        /// Reset order-related state after delivery
        /// </summary>
        public void ResetOrderState()
        {
            CurrentOrderId = "";
            TargetX = -1;
            TargetY = -1;
            IsMoving = false;
            MovementTrail.Clear();
        }

        /// <summary>
        /// Get average satisfaction rating
        /// </summary>
        public double GetAverageRating()
        {
            if (SatisfactionScores.Count == 0)
                return 0.0;
            return SatisfactionScores.Average();
        }
    }
}
