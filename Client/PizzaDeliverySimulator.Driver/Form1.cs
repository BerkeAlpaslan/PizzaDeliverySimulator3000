using PizzaDeliverySimulator.Driver.Controllers;
using PizzaDeliverySimulator.Driver.Models;
using PizzaDeliverySimulator.Driver.Rendering;
using PizzaDeliverySimulator.Driver.Services;

namespace PizzaDeliverySimulator.Driver
{
    /// <summary>
    /// Main driver form - orchestrates UI events and controllers
    /// </summary>
    public partial class DriverForm : Form
    {
        // Core components
        private DriverState state;
        private NetworkManager network;
        private MapRenderer mapRenderer;

        // Controllers
        private MovementController movement;
        private OrderHandler orderHandler;
        private UIController uiController;

        // Timers
        private System.Windows.Forms.Timer locationTimer;
        private System.Windows.Forms.Timer movementTimer;

        public DriverForm()
        {
            InitializeComponent();
            InitializeComponents();
            InitializeControllers();
            InitializeTimers();
            InitializeAsync();
        }

        /// <summary>
        /// Initialize core components
        /// </summary>
        private void InitializeComponents()
        {
            // Create state
            state = new DriverState();

            // Create network manager
            network = new NetworkManager(state);

            // Create map renderer
            mapRenderer = new MapRenderer(state, pictureBox_map);
        }

        /// <summary>
        /// Initialize controllers
        /// </summary>
        private void InitializeControllers()
        {
            // Create UI controller
            uiController = new UIController(
                state,
                txt_name,
                btn_ready,
                btn_notReady,
                btn_outForDelivery,
                btn_delivered,
                lbl_deliveredCount,
                lbl_avgRating,
                list_notifications
            );

            // Create movement controller
            movement = new MovementController(state, network);
            movement.OnMovementUpdate += uiController.AddMessage;  // Will initialize after UIController
            movement.OnArrivedAtCustomer += () => uiController.OnArrivedAtCustomer();
            movement.OnArrivedAtBranch += () => uiController.OnArrivedAtBranch();
            movement.OnMapNeedsUpdate += () => mapRenderer.DrawMap();

            // Create order handler
            orderHandler = new OrderHandler(state, network, movement);
            orderHandler.OnOrderUpdate += (msg) => uiController.AddMessage(msg);
            orderHandler.OnOrderAssigned += OnOrderAssignedHandler;
            orderHandler.OnDeliveryCompleted += OnDeliveryCompletedHandler;

            // Re-wire movement events (now that UIController exists)
            // movement.OnMovementUpdate += uiController.AddMessage;

            // Subscribe to network events
            network.OnMessageReceived += uiController.AddMessage;
            network.OnError += uiController.ShowNetworkError;
            network.OnDriverRegistered += OnDriverRegistered;
            network.OnOrderAssigned += orderHandler.HandleOrderAssignment;
            network.OnSatisfactionReceived += orderHandler.HandleSatisfaction;
        }

        /// <summary>
        /// Initialize timers
        /// </summary>
        private void InitializeTimers()
        {
            // Location update timer (every 2 seconds)
            locationTimer = new System.Windows.Forms.Timer();
            locationTimer.Interval = 2000;
            locationTimer.Tick += LocationTimer_Tick;
            locationTimer.Start();

            // Movement timer (every 2 seconds)
            movementTimer = new System.Windows.Forms.Timer();
            movementTimer.Interval = 2000;
            movementTimer.Tick += MovementTimer_Tick;
            movementTimer.Start();
        }

        /// <summary>
        /// Initialize async operations
        /// </summary>
        private async void InitializeAsync()
        {
            uiController.DisableAllButtons();

            bool connected = await network.ConnectAsync();
            if (connected)
            {
                _ = network.StartReceivingAsync();
            }
        }

        // ===== TIMER EVENTS =====

        private async void LocationTimer_Tick(object sender, EventArgs e)
        {
            if (state.IsRegistered)
            {
                await network.SendCommandAsync($"LOCATION:{state.DriverX}:{state.DriverY}", silent: true);
            }
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            if (state.IsMoving)
            {
                movement.MoveTowardsTarget();
            }
        }

        // ===== BUTTON EVENTS =====

        private async void txt_name_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;

                if (!string.IsNullOrWhiteSpace(txt_name.Text))
                {
                    await network.SendCommandAsync($"REGISTER:{txt_name.Text.Trim()}");
                }
                else
                {
                    MessageBox.Show("Please enter your name!", "Name Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private async void btn_ready_Click(object sender, EventArgs e)
        {
            await network.SendCommandAsync("READY");
        }

        private async void btn_notReady_Click(object sender, EventArgs e)
        {
            await network.SendCommandAsync("NOTREADY");
        }

        private void btn_outForDelivery_Click(object sender, EventArgs e)
        {
            orderHandler.StartDelivery();
            uiController.OnDeliveryStarted();
        }

        private void btn_delivered_Click(object sender, EventArgs e)
        {
            orderHandler.CompleteDelivery();
            uiController.OnDeliveryCompleted();
            mapRenderer.DrawMap();
        }

        // ===== NETWORK EVENT HANDLERS =====

        private void OnDriverRegistered(string message)
        {
            // THREAD SAFETY
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnDriverRegistered(message)));
                return;
            }

            // Parse: REGISTERED:DRV12345678:X:Y:BranchId:BranchName
            string[] parts = message.Split(':');

            if (parts.Length >= 6)
            {
                state.DriverId = parts[1];
                state.DriverX = int.Parse(parts[2]);
                state.DriverY = int.Parse(parts[3]);
                state.BranchX = state.DriverX;
                state.BranchY = state.DriverY;
                state.BranchId = parts[4];
                state.BranchName = parts[5];
                state.IsRegistered = true;

                // Update UI
                uiController.ShowRegistrationSuccess();
                mapRenderer.DrawMap();

                uiController.AddMessage($"[SUCCESS] Registered at {state.BranchName}");
            }
        }

        // ===== CONTROLLER EVENT HANDLERS =====

        private void OnOrderAssignedHandler(OrderInfo orderInfo)
        {
            uiController.OnOrderAssigned();
            mapRenderer.DrawMap();
        }

        private void OnDeliveryCompletedHandler(DeliveryStats stats)
        {
            uiController.UpdateStatistics();
            uiController.ShowDeliveryComplete(stats);
        }

        // ===== FORM EVENTS =====

        private void DriverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            locationTimer?.Stop();
            movementTimer?.Stop();
            network?.Disconnect();
        }
    }
}
