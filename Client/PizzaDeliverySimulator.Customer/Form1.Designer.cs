namespace PizzaDeliverySimulator.Customer
{
    partial class CustomerForm
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
            comboBox_pizza = new ComboBox();
            txt_address = new TextBox();
            btn_order = new Button();
            list_notifications = new ListBox();
            pictureBox_map = new PictureBox();
            lbl_enter = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox_map).BeginInit();
            SuspendLayout();
            // 
            // label_header
            // 
            label_header.AutoSize = true;
            label_header.Location = new Point(12, 9);
            label_header.Name = "label_header";
            label_header.Size = new Size(245, 25);
            label_header.TabIndex = 0;
            label_header.Text = "Pizza Delivery Simulator 3000";
            // 
            // label_welcome
            // 
            label_welcome.AutoSize = true;
            label_welcome.Location = new Point(12, 39);
            label_welcome.Name = "label_welcome";
            label_welcome.Size = new Size(172, 25);
            label_welcome.TabIndex = 1;
            label_welcome.Text = "Welcome Customer!";
            // 
            // txt_name
            // 
            txt_name.Location = new Point(12, 67);
            txt_name.Name = "txt_name";
            txt_name.Size = new Size(150, 31);
            txt_name.TabIndex = 2;
            txt_name.Text = "Enter Your Name";
            txt_name.KeyPress += txt_name_KeyPress;
            // 
            // comboBox_pizza
            // 
            comboBox_pizza.FormattingEnabled = true;
            comboBox_pizza.Location = new Point(12, 115);
            comboBox_pizza.Name = "comboBox_pizza";
            comboBox_pizza.Size = new Size(182, 33);
            comboBox_pizza.TabIndex = 3;
            comboBox_pizza.Text = "Pizza Types";
            // 
            // txt_address
            // 
            txt_address.Location = new Point(181, 67);
            txt_address.Name = "txt_address";
            txt_address.Size = new Size(171, 31);
            txt_address.TabIndex = 4;
            txt_address.Text = "Enter Your Address";
            // 
            // btn_order
            // 
            btn_order.Location = new Point(219, 115);
            btn_order.Name = "btn_order";
            btn_order.Size = new Size(112, 34);
            btn_order.TabIndex = 5;
            btn_order.Text = "ORDER";
            btn_order.UseVisualStyleBackColor = true;
            btn_order.Click += btn_order_Click;
            // 
            // list_notifications
            // 
            list_notifications.FormattingEnabled = true;
            list_notifications.ItemHeight = 25;
            list_notifications.Location = new Point(358, 12);
            list_notifications.Name = "list_notifications";
            list_notifications.Size = new Size(741, 179);
            list_notifications.TabIndex = 6;
            // 
            // pictureBox_map
            // 
            pictureBox_map.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_map.Location = new Point(474, 197);
            pictureBox_map.Name = "pictureBox_map";
            pictureBox_map.Size = new Size(520, 520);
            pictureBox_map.TabIndex = 7;
            pictureBox_map.TabStop = false;
            // 
            // lbl_enter
            // 
            lbl_enter.AutoSize = true;
            lbl_enter.Location = new Point(12, 151);
            lbl_enter.Name = "lbl_enter";
            lbl_enter.Size = new Size(284, 25);
            lbl_enter.TabIndex = 8;
            lbl_enter.Text = "Press Enter After Enter Your Name!";
            // 
            // CustomerForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1112, 721);
            Controls.Add(lbl_enter);
            Controls.Add(pictureBox_map);
            Controls.Add(list_notifications);
            Controls.Add(btn_order);
            Controls.Add(txt_address);
            Controls.Add(comboBox_pizza);
            Controls.Add(txt_name);
            Controls.Add(label_welcome);
            Controls.Add(label_header);
            Name = "CustomerForm";
            Text = "Customer";
            FormClosing += CustomerForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)pictureBox_map).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label_header;
        private Label label_welcome;
        private TextBox txt_name;
        private ComboBox comboBox_pizza;
        private TextBox txt_address;
        private Button btn_order;
        private ListBox list_notifications;
        private PictureBox pictureBox_map;
        private Label lbl_enter;
    }
}
