using PizzaDeliverySimulator.Driver.Models;
using PizzaDeliverySimulator.Driver.Services;

namespace PizzaDeliverySimulator.Driver.Controllers
{
    /// <summary>
    /// Handles all driver movement logic
    /// </summary>
    public class MovementController
    {
        private DriverState state;
        private NetworkManager network;

        // Events for UI updates
        public event Action<string> OnMovementUpdate;
        public event Action OnArrivedAtCustomer;
        public event Action OnArrivedAtBranch;
        public event Action OnMapNeedsUpdate;

        public MovementController(DriverState driverState, NetworkManager networkManager)
        {
            state = driverState;
            network = networkManager;
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
        /// Calculate distance to current target
        /// </summary>
        public double GetDistanceToTarget()
        {
            return CalculateDistance(state.DriverX, state.DriverY, state.TargetX, state.TargetY);
        }

        /// <summary>
        /// Move driver towards target (called by timer)
        /// </summary>
        public void MoveTowardsTarget()
        {
            double distance = GetDistanceToTarget();

            // Check if arrived
            if (distance < 3)
            {
                HandleArrival();
                return;
            }

            // Calculate and apply movement
            PerformMovementStep();

            // Update distance after movement
            distance = GetDistanceToTarget();

            // Notify UI
            OnMovementUpdate?.Invoke($"[MOVING] Position: ({state.DriverX}, {state.DriverY}) - Distance: {distance:F1} units");
            OnMapNeedsUpdate?.Invoke();
        }

        /// <summary>
        /// Handle arrival at destination (customer or branch)
        /// </summary>
        private void HandleArrival()
        {
            // Snap to exact position
            state.DriverX = state.TargetX;
            state.DriverY = state.TargetY;
            state.IsMoving = false;

            if (state.IsReturningToBranch)
            {
                HandleBranchArrival();
            }
            else
            {
                HandleCustomerArrival();
            }
        }

        /// <summary>
        /// Handle arrival at customer location
        /// </summary>
        private async void HandleCustomerArrival()
        {
            // Notify server
            await network.SendCommandAsync($"ARRIVED:{state.CurrentOrderId}:{state.DriverX}:{state.DriverY}");

            // Notify UI
            OnMovementUpdate?.Invoke($"[ARRIVAL] Arrived at customer ({state.TargetX}, {state.TargetY})!");
            OnMovementUpdate?.Invoke($"[INFO] You can now mark order as DELIVERED");
            OnArrivedAtCustomer?.Invoke();
            OnMapNeedsUpdate?.Invoke();
        }

        /// <summary>
        /// Handle arrival at branch
        /// </summary>
        private void HandleBranchArrival()
        {
            state.IsReturningToBranch = false;
            state.MovementTrail.Clear();

            // Notify UI
            OnMovementUpdate?.Invoke($"[ARRIVAL] Returned to {state.BranchName}!");
            OnMovementUpdate?.Invoke($"[INFO] Ready for next order - click READY button");
            OnArrivedAtBranch?.Invoke();
            OnMapNeedsUpdate?.Invoke();
        }

        /// <summary>
        /// Perform single movement step towards target
        /// </summary>
        private void PerformMovementStep()
        {
            // Calculate direction
            int dx = state.TargetX - state.DriverX;
            int dy = state.TargetY - state.DriverY;

            double magnitude = Math.Sqrt(dx * dx + dy * dy);
            double normalizedDx = dx / magnitude;
            double normalizedDy = dy / magnitude;

            // Random step size (2-3 units)
            Random random = new Random();
            int stepSize = random.Next(2, 4);

            int moveX = (int)Math.Round(normalizedDx * stepSize);
            int moveY = (int)Math.Round(normalizedDy * stepSize);

            // Update position
            state.DriverX += moveX;
            state.DriverY += moveY;

            // Keep in bounds (0-50)
            state.DriverX = Math.Max(0, Math.Min(50, state.DriverX));
            state.DriverY = Math.Max(0, Math.Min(50, state.DriverY));

            // Add to trail
            state.MovementTrail.Add(new Point(state.DriverX, state.DriverY));
        }

        /// <summary>
        /// Start movement to customer location
        /// </summary>
        public void StartDeliveryMovement()
        {
            state.IsMoving = true;
            state.IsReturningToBranch = false;
        }

        /// <summary>
        /// Start return to branch
        /// </summary>
        public void StartReturnToBranch()
        {
            state.TargetX = state.BranchX;
            state.TargetY = state.BranchY;
            state.IsReturningToBranch = true;
            state.IsMoving = true;
        }
    }
}
