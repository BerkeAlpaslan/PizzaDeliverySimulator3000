using PizzaDeliverySimulator.Customer.Controllers;
using PizzaDeliverySimulator.Customer.Models;
using PizzaDeliverySimulator.Customer.Services;
using PizzaDeliverySimulator.Customer.Rendering;
using PizzaDeliverySimulator.Common;

namespace PizzaDeliverySimulator.Customer
{
    /// <summary>
    /// Main customer form - orchestrates UI events and controllers
    /// </summary>
    public partial class CustomerForm : Form
    {
        // Core components
        private CustomerState state;
        private NetworkManager network;
        private MapRenderer mapRenderer;

        // Controllers
        private OrderController orderController;
        private TrackingController trackingController;
        private UIController uiController;

        public CustomerForm()
        {
            InitializeComponent();
            InitializeComponents();
            InitializeControllers();
            InitializeAsync();
        }

        /// <summary>
        /// Initialize core components
        /// </summary>
        private void InitializeComponents()
        {
            // Create state
            state = new CustomerState();

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
            // Create order controller
            orderController = new OrderController(state, network);
            orderController.OnOrderUpdate += (msg) => uiController.AddMessage(msg);
            orderController.OnOrderCreated += OnOrderCreatedHandler;
            orderController.OnDriverAssigned += OnDriverAssignedHandler;
            orderController.OnSatisfactionReceived += OnSatisfactionReceivedHandler;

            // Create tracking controller
            trackingController = new TrackingController(state);
            trackingController.OnTrackingUpdate += (msg) => uiController.AddMessage(msg);
            trackingController.OnDriverLocationUpdated += OnDriverLocationUpdatedHandler;
            trackingController.OnDriverArrived += OnDriverArrivedHandler;

            // Create UI controller
            uiController = new UIController(
                state,
                txt_name,
                txt_address,
                comboBox_pizza,
                btn_order,
                list_notifications
            );

            // Subscribe to network events
            network.OnMessageReceived += uiController.AddMessage;
            network.OnError += uiController.ShowNetworkError;
            network.OnCustomerRegistered += OnCustomerRegistered;
            network.OnOrderCreated += orderController.HandleOrderCreated;
            network.OnDriverAssigned += orderController.HandleDriverAssignment;
            network.OnDriverLocationUpdate += trackingController.HandleLocationUpdate;
            network.OnDriverArrived += trackingController.HandleDriverArrival;
            network.OnSatisfactionReceived += orderController.HandleSatisfaction;
        }

        /// <summary>
        /// Initialize async operations
        /// </summary>
        private async void InitializeAsync()
        {
            uiController.DisableAllControls();

            bool connected = await network.ConnectAsync();
            if (connected)
            {
                _ = network.StartReceivingAsync();
                network.StartUdpListener();
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
                    await network.SendCommandAsync($"{Protocol.REGISTER_CUSTOMER}:{txt_name.Text.Trim()}");
                }
                else
                {
                    MessageBox.Show("Please enter your name!", "Name Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btn_order_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (!uiController.ValidateOrderInputs(out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get inputs
                var (pizza, address) = uiController.GetOrderInputs();

                // Create order
                orderController.CreateOrder(pizza, address);

                // Update UI
                uiController.OnOrderStarted();
            }
            catch (Exception ex)
            {
                uiController.AddMessage($"[ERROR] Order failed: {ex.Message}");
            }
        }

        // ===== NETWORK EVENT HANDLERS =====

        private void OnCustomerRegistered(string message)
        {
            // THREAD SAFETY
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnCustomerRegistered(message)));
                return;
            }

            // Parse: REGISTERED:CUSTABCD1234:X:Y
            string[] parts = message.Split(':');

            if (parts.Length >= 4)
            {
                state.CustomerId = parts[1];
                state.CustomerX = int.Parse(parts[2]);
                state.CustomerY = int.Parse(parts[3]);
                state.Name = txt_name.Text.Trim();
                state.IsRegistered = true;

                // Update UI
                uiController.ShowRegistrationSuccess();
                mapRenderer.DrawMap();

                uiController.AddMessage($"[SUCCESS] Registered successfully");
            }
        }

        // ===== CONTROLLER EVENT HANDLERS =====

        private void OnOrderCreatedHandler(OrderCreatedInfo info)
        {
            // Order created - no UI changes needed
        }

        private void OnDriverAssignedHandler(DriverAssignedInfo info)
        {
            mapRenderer.DrawMap();
        }

        private void OnDriverLocationUpdatedHandler(DriverLocationInfo info)
        {
            mapRenderer.DrawMap();
        }

        private void OnDriverArrivedHandler(DriverArrivalInfo info)
        {
            mapRenderer.DrawMap();
        }

        private void OnSatisfactionReceivedHandler(SatisfactionInfo info)
        {
            uiController.ShowDeliveryComplete(info);
            uiController.OnOrderCompleted();
            mapRenderer.DrawMap();
        }

        // ===== FORM EVENTS =====

        private void CustomerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            network?.Disconnect();
        }
    }
}
