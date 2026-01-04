namespace PizzaDeliverySimulator.Driver
{
    partial class DriverForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label_header = new Label();
            label_welcome = new Label();
            txt_name = new TextBox();
            btn_ready = new Button();
            btn_notReady = new Button();
            btn_outForDelivery = new Button();
            btn_delivered = new Button();
            list_notifications = new ListBox();
            pictureBox_map = new PictureBox();
            lbl_deliveredCount = new Label();
            lbl_avgRating = new Label();
            lbl_enter = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox_map).BeginInit();
            SuspendLayout();
            // 
            // label_header
            // 
            label_header.AutoSize = true;
            label_header.Location = new Point(12, 0);
            label_header.Name = "label_header";
            label_header.Size = new Size(245, 25);
            label_header.TabIndex = 0;
            label_header.Text = "Pizza Delivery Simulator 3000";
            // 
            // label_welcome
            // 
            label_welcome.AutoSize = true;
            label_welcome.Location = new Point(12, 25);
            label_welcome.Name = "label_welcome";
            label_welcome.Size = new Size(142, 25);
            label_welcome.TabIndex = 1;
            label_welcome.Text = "Welcome Driver!";
            // 
            // txt_name
            // 
            txt_name.Location = new Point(12, 53);
            txt_name.Name = "txt_name";
            txt_name.Size = new Size(150, 31);
            txt_name.TabIndex = 2;
            txt_name.Text = "Enter Your Name";
            txt_name.KeyPress += txt_name_KeyPress;
            // 
            // btn_ready
            // 
            btn_ready.Location = new Point(12, 90);
            btn_ready.Name = "btn_ready";
            btn_ready.Size = new Size(112, 34);
            btn_ready.TabIndex = 3;
            btn_ready.Text = "READY";
            btn_ready.UseVisualStyleBackColor = true;
            btn_ready.Click += btn_ready_Click;
            // 
            // btn_notReady
            // 
            btn_notReady.Location = new Point(162, 90);
            btn_notReady.Name = "btn_notReady";
            btn_notReady.Size = new Size(129, 34);
            btn_notReady.TabIndex = 4;
            btn_notReady.Text = "NOT READY";
            btn_notReady.UseVisualStyleBackColor = true;
            btn_notReady.Click += btn_notReady_Click;
            // 
            // btn_outForDelivery
            // 
            btn_outForDelivery.Location = new Point(12, 130);
            btn_outForDelivery.Name = "btn_outForDelivery";
            btn_outForDelivery.Size = new Size(192, 34);
            btn_outForDelivery.TabIndex = 5;
            btn_outForDelivery.Text = "OUT FOR DELIVERY";
            btn_outForDelivery.UseVisualStyleBackColor = true;
            btn_outForDelivery.Click += btn_outForDelivery_Click;
            // 
            // btn_delivered
            // 
            btn_delivered.Location = new Point(289, 130);
            btn_delivered.Name = "btn_delivered";
            btn_delivered.Size = new Size(112, 34);
            btn_delivered.TabIndex = 6;
            btn_delivered.Text = "DELIVERED";
            btn_delivered.UseVisualStyleBackColor = true;
            btn_delivered.Click += btn_delivered_Click;
            // 
            // list_notifications
            // 
            list_notifications.FormattingEnabled = true;
            list_notifications.ItemHeight = 25;
            list_notifications.Location = new Point(426, 12);
            list_notifications.Name = "list_notifications";
            list_notifications.Size = new Size(650, 204);
            list_notifications.TabIndex = 7;
            // 
            // pictureBox_map
            // 
            pictureBox_map.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_map.Location = new Point(498, 222);
            pictureBox_map.Name = "pictureBox_map";
            pictureBox_map.Size = new Size(520, 520);
            pictureBox_map.TabIndex = 8;
            pictureBox_map.TabStop = false;
            // 
            // lbl_deliveredCount
            // 
            lbl_deliveredCount.AutoSize = true;
            lbl_deliveredCount.Location = new Point(12, 222);
            lbl_deliveredCount.Name = "lbl_deliveredCount";
            lbl_deliveredCount.Size = new Size(190, 25);
            lbl_deliveredCount.TabIndex = 9;
            lbl_deliveredCount.Text = "Delivered Order Count";
            // 
            // lbl_avgRating
            // 
            lbl_avgRating.AutoSize = true;
            lbl_avgRating.Location = new Point(12, 266);
            lbl_avgRating.Name = "lbl_avgRating";
            lbl_avgRating.Size = new Size(229, 25);
            lbl_avgRating.TabIndex = 10;
            lbl_avgRating.Text = "Average Satisfaction Rating";
            // 
            // lbl_enter
            // 
            lbl_enter.AutoSize = true;
            lbl_enter.Location = new Point(12, 167);
            lbl_enter.Name = "lbl_enter";
            lbl_enter.Size = new Size(284, 25);
            lbl_enter.TabIndex = 11;
            lbl_enter.Text = "Press Enter After Enter Your Name!";
            // 
            // DriverForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1088, 748);
            Controls.Add(lbl_enter);
            Controls.Add(lbl_avgRating);
            Controls.Add(lbl_deliveredCount);
            Controls.Add(pictureBox_map);
            Controls.Add(list_notifications);
            Controls.Add(btn_delivered);
            Controls.Add(btn_outForDelivery);
            Controls.Add(btn_notReady);
            Controls.Add(btn_ready);
            Controls.Add(txt_name);
            Controls.Add(label_welcome);
            Controls.Add(label_header);
            Name = "DriverForm";
            Text = "Driver";
            FormClosing += DriverForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)pictureBox_map).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label_header;
        private Label label_welcome;
        private TextBox txt_name;
        private Button btn_ready;
        private Button btn_notReady;
        private Button btn_outForDelivery;
        private Button btn_delivered;
        private ListBox list_notifications;
        private PictureBox pictureBox_map;
        private Label lbl_deliveredCount;
        private Label lbl_avgRating;
        private Label lbl_enter;
    }
}
