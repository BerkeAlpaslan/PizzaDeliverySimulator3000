using PizzaDeliverySimulator.Driver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliverySimulator.Driver.Controllers
{
    /// <summary>
    /// Handles UI state updates and visual feedback
    /// </summary>
    public class UIController
    {
        private DriverState state;

        // UI Controls (injected from Form)
        private TextBox txt_name;
        private Button btn_ready;
        private Button btn_notReady;
        private Button btn_outForDelivery;
        private Button btn_delivered;
        private Label lbl_deliveredCount;
        private Label lbl_avgRating;
        private ListBox list_notifications;

        public UIController(
            DriverState driverState,
            TextBox nameTextBox,
            Button readyButton,
            Button notReadyButton,
            Button outForDeliveryButton,
            Button deliveredButton,
            Label deliveredCountLabel,
            Label avgRatingLabel,
            ListBox notificationsList)
        {
            state = driverState;
            txt_name = nameTextBox;
            btn_ready = readyButton;
            btn_notReady = notReadyButton;
            btn_outForDelivery = outForDeliveryButton;
            btn_delivered = deliveredButton;
            lbl_deliveredCount = deliveredCountLabel;
            lbl_avgRating = avgRatingLabel;
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

            // Enable buttons
            btn_ready.Enabled = true;
            btn_notReady.Enabled = true;

            // Show popup
            MessageBox.Show(
                $"Welcome!\n\n" +
                $"Driver ID: {state.DriverId}\n" +
                $"Assigned Branch: {state.BranchName}\n" +
                $"Location: ({state.DriverX}, {state.DriverY})",
                "Registration Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Update UI when order is assigned
        /// </summary>
        public void OnOrderAssigned()
        {
            btn_outForDelivery.Enabled = true;
            btn_outForDelivery.BackColor = Color.LightGreen;
        }

        /// <summary>
        /// Update UI when delivery starts
        /// </summary>
        public void OnDeliveryStarted()
        {
            btn_outForDelivery.Enabled = false;
            btn_outForDelivery.BackColor = Color.Gray;
        }

        /// <summary>
        /// Update UI when driver arrives at customer
        /// </summary>
        public void OnArrivedAtCustomer()
        {
            btn_delivered.Enabled = true;
            btn_delivered.BackColor = Color.Orange;
        }

        /// <summary>
        /// Update UI when delivery is completed
        /// </summary>
        public void OnDeliveryCompleted()
        {
            btn_delivered.Enabled = false;
            btn_delivered.BackColor = Color.Gray;

            // Lock READY button until branch arrival
            btn_ready.Enabled = false;
        }

        /// <summary>
        /// Update UI when driver arrives at branch
        /// </summary>
        public void OnArrivedAtBranch()
        {
            btn_ready.Enabled = true;
        }

        /// <summary>
        /// Show delivery completion popup
        /// </summary>
        public void ShowDeliveryComplete(DeliveryStats stats)
        {
            string popupMessage = "DELIVERY COMPLETED!\n\n" +
                                 $"Order ID: {stats.OrderId}\n" +
                                 $"Satisfaction Score: {stats.Score}/5\n" +
                                 $"Delivered in: {stats.ActualTime}s\n" +
                                 $"Estimated: {stats.EstimatedTime}s";

            MessageBox.Show(popupMessage, "Delivery Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Update statistics labels
        /// </summary>
        public void UpdateStatistics()
        {
            if (lbl_deliveredCount.InvokeRequired)
            {
                lbl_deliveredCount.Invoke(new Action(() => UpdateStatistics()));
                return;
            }

            lbl_deliveredCount.Text = $"Total Delivered: {state.DeliveredCount}";
            lbl_avgRating.Text = $"Avg Rating: {state.GetAverageRating():F2}/5.0";
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
        /// Disable all buttons (initial state)
        /// </summary>
        public void DisableAllButtons()
        {
            btn_ready.Enabled = false;
            btn_notReady.Enabled = false;
            btn_outForDelivery.Enabled = false;
            btn_delivered.Enabled = false;
        }

        /// <summary>
        /// Show network error popup
        /// </summary>
        public void ShowNetworkError(string error)
        {
            MessageBox.Show(error, "Network Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
