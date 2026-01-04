using PizzaDeliverySimulator.Customer.Models;
using PizzaDeliverySimulator.Common;

namespace PizzaDeliverySimulator.Customer.Controllers
{
    /// <summary>
    /// Handles UI state updates and visual feedback
    /// </summary>
    public class UIController
    {
        private CustomerState state;

        // UI Controls (injected from Form)
        private TextBox txt_name;
        private TextBox txt_address;
        private ComboBox comboBox_pizza;
        private Button btn_order;
        private ListBox list_notifications;

        public UIController(
            CustomerState customerState,
            TextBox nameTextBox,
            TextBox addressTextBox,
            ComboBox pizzaComboBox,
            Button orderButton,
            ListBox notificationsList)
        {
            state = customerState;
            txt_name = nameTextBox;
            txt_address = addressTextBox;
            comboBox_pizza = pizzaComboBox;
            btn_order = orderButton;
            list_notifications = notificationsList;
        }

        /// <summary>
        /// Show registration success popup and update UI
        /// </summary>
        public void ShowRegistrationSuccess()
        {
            // Lock name textbox
            txt_name.ReadOnly = true;
            txt_name.BackColor = Color.LightGray;

            // Enable order controls
            comboBox_pizza.Enabled = true;
            txt_address.Enabled = true;
            btn_order.Enabled = true;

            // Populate pizza ComboBox
            PopulatePizzaComboBox();

            // Show popup
            MessageBox.Show(
                $"Welcome!\n\n" +
                $"Customer ID: {state.CustomerId}\n" +
                $"Location: ({state.CustomerX}, {state.CustomerY})",
                "Registration Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Populate pizza ComboBox
        /// </summary>
        private void PopulatePizzaComboBox()
        {
            comboBox_pizza.Items.Clear();

            foreach (var pizza in PizzaTypeExtensions.GetAll())
            {
                comboBox_pizza.Items.Add(pizza.GetDisplayName());
            }

            // Select first item
            if (comboBox_pizza.Items.Count > 0)
            {
                comboBox_pizza.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Update UI when order starts
        /// </summary>
        public void OnOrderStarted()
        {
            btn_order.Enabled = false;
            btn_order.BackColor = Color.Gray;
        }

        /// <summary>
        /// Update UI when order completes
        /// </summary>
        public void OnOrderCompleted()
        {
            btn_order.Enabled = true;
            btn_order.BackColor = SystemColors.Control;
        }

        /// <summary>
        /// Show delivery completion popup
        /// </summary>
        public void ShowDeliveryComplete(SatisfactionInfo info)
        {
            MessageBox.Show(
                "DELIVERY COMPLETED!\n\n" +
                $"Order ID: {info.OrderId}\n" +
                $"Satisfaction Score: {info.Score}/5\n\n" +
                $"Delivered in: {info.ActualTime}s\n" +
                $"Estimated: {info.EstimatedTime}s",
                "Delivery Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Add message to notifications list
        /// </summary>
        public void AddMessage(string message)
        {
            if (list_notifications.InvokeRequired)
            {
                list_notifications.Invoke(new Action(() => AddMessage(message)));
                return;
            }

            list_notifications.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            list_notifications.TopIndex = list_notifications.Items.Count - 1;
        }

        /// <summary>
        /// Disable all controls (initial state)
        /// </summary>
        public void DisableAllControls()
        {
            txt_name.Enabled = true;
            txt_name.ReadOnly = false;

            comboBox_pizza.Enabled = false;
            txt_address.Enabled = false;
            btn_order.Enabled = false;
        }

        /// <summary>
        /// Show network error popup
        /// </summary>
        public void ShowNetworkError(string error)
        {
            MessageBox.Show(error, "Network Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Validate order inputs
        /// </summary>
        public bool ValidateOrderInputs(out string errorMessage)
        {
            if (comboBox_pizza.SelectedIndex < 0)
            {
                errorMessage = "Please select a pizza type!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txt_address.Text))
            {
                errorMessage = "Please enter your address!";
                return false;
            }

            if (state.HasActiveOrder)
            {
                errorMessage = "You already have an active order!";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Get selected pizza and address
        /// </summary>
        public (string pizza, string address) GetOrderInputs()
        {
            string pizza = comboBox_pizza.SelectedItem?.ToString() ?? "";
            string address = txt_address.Text.Trim();
            return (pizza, address);
        }
    }
}
