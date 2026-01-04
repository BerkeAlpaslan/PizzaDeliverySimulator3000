using PizzaDeliverySimulator.Driver.Models;
using System.Drawing.Drawing2D;

namespace PizzaDeliverySimulator.Driver.Rendering
{
    /// <summary>
    /// Handles all map rendering operations
    /// </summary>
    public class MapRenderer
    {
        private DriverState state;
        private PictureBox mapPictureBox;

        // Map constants
        private const int PADDING = 10;
        private const int GRID_SIZE = 50;
        private const int CELL_SIZE = 10;
        private const int MAP_SIZE = 520; // 500 + 20 padding

        public MapRenderer(DriverState driverState, PictureBox pictureBox)
        {
            state = driverState;
            mapPictureBox = pictureBox;
        }

        /// <summary>
        /// Draw complete map with all elements
        /// </summary>
        public void DrawMap()
        {
            try
            {
                // Thread safety check
                if (mapPictureBox.InvokeRequired)
                {
                    mapPictureBox.Invoke(new Action(() => DrawMap()));
                    return;
                }

                // Create bitmap
                Bitmap bmp = new Bitmap(MAP_SIZE, MAP_SIZE);
                Graphics g = Graphics.FromImage(bmp);

                // Enable anti-aliasing
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Background
                g.Clear(Color.White);

                // Draw all layers
                DrawAlternatingGrid(g);
                DrawGridLines(g);
                DrawGridNumbers(g);
                DrawLegend(g);
                DrawMovementTrail(g);
                DrawDistanceLine(g);
                DrawMarkers(g);
                DrawArrivalAnimation(g);

                // Update picture box
                mapPictureBox.Image = bmp;

                // Cleanup
                g.Dispose();
            }
            catch (Exception ex)
            {
                // Silent fail - don't crash UI
                Console.WriteLine($"Map draw error: {ex.Message}");
            }
        }

        private void DrawAlternatingGrid(Graphics g)
        {
            Brush lightGrayBrush = new SolidBrush(Color.FromArgb(248, 248, 248));

            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        g.FillRectangle(lightGrayBrush,
                            PADDING + x * CELL_SIZE,
                            PADDING + y * CELL_SIZE,
                            CELL_SIZE, CELL_SIZE);
                    }
                }
            }
        }

        private void DrawGridLines(Graphics g)
        {
            Pen gridPen = new Pen(Color.LightGray, 1);

            for (int i = 0; i <= GRID_SIZE; i++)
            {
                // Vertical lines
                g.DrawLine(gridPen,
                    PADDING + i * CELL_SIZE, PADDING,
                    PADDING + i * CELL_SIZE, PADDING + GRID_SIZE * CELL_SIZE);

                // Horizontal lines
                g.DrawLine(gridPen,
                    PADDING, PADDING + i * CELL_SIZE,
                    PADDING + GRID_SIZE * CELL_SIZE, PADDING + i * CELL_SIZE);
            }
        }

        private void DrawGridNumbers(Graphics g)
        {
            Font numberFont = new Font("Arial", 7);
            Brush numberBrush = Brushes.Gray;

            for (int i = 0; i <= GRID_SIZE; i += 10)
            {
                string num = i.ToString();
                SizeF textSize = g.MeasureString(num, numberFont);

                // X-axis (top)
                g.DrawString(num, numberFont, numberBrush,
                    PADDING + i * CELL_SIZE - textSize.Width / 2, 2);

                // Y-axis (left)
                g.DrawString(num, numberFont, numberBrush,
                    2, PADDING + i * CELL_SIZE - textSize.Height / 2);
            }
        }

        private void DrawLegend(Graphics g)
        {
            int legendX = 300;
            int legendY = 5;
            Font legendFont = new Font("Arial", 8, FontStyle.Bold);

            // Customer 
            g.FillEllipse(Brushes.Blue, legendX - 50, legendY, 10, 10);
            g.DrawString("Customer", legendFont, Brushes.Black, legendX - 35, legendY - 2);

            // Driver
            g.FillEllipse(Brushes.Red, legendX + 60, legendY, 10, 10);
            g.DrawString("Driver", legendFont, Brushes.Black, legendX + 75, legendY - 2);

            // Branch
            g.FillRectangle(Brushes.LimeGreen, legendX + 135, legendY, 10, 10);
            g.DrawString("Branch", legendFont, Brushes.Black, legendX + 150, legendY - 2);
        }

        private void DrawMovementTrail(Graphics g)
        {
            if (state.MovementTrail.Count > 1)
            {
                Pen trailPen = new Pen(Color.LightCoral, 2);

                for (int i = 0; i < state.MovementTrail.Count - 1; i++)
                {
                    Point p1 = state.MovementTrail[i];
                    Point p2 = state.MovementTrail[i + 1];

                    g.DrawLine(trailPen,
                        PADDING + p1.X * CELL_SIZE,
                        PADDING + p1.Y * CELL_SIZE,
                        PADDING + p2.X * CELL_SIZE,
                        PADDING + p2.Y * CELL_SIZE);
                }
            }
        }

        private void DrawDistanceLine(Graphics g)
        {
            if (state.TargetX >= 0 && state.TargetY >= 0)
            {
                Pen distancePen = new Pen(Color.Gray, 1);
                distancePen.DashStyle = DashStyle.Dash;

                int driverPixelX = PADDING + state.DriverX * CELL_SIZE;
                int driverPixelY = PADDING + state.DriverY * CELL_SIZE;
                int customerPixelX = PADDING + state.TargetX * CELL_SIZE;
                int customerPixelY = PADDING + state.TargetY * CELL_SIZE;

                g.DrawLine(distancePen,
                    driverPixelX, driverPixelY,
                    customerPixelX, customerPixelY);

                // Distance text
                double distance = Math.Sqrt(
                    Math.Pow(state.TargetX - state.DriverX, 2) +
                    Math.Pow(state.TargetY - state.DriverY, 2)
                );

                int midX = (driverPixelX + customerPixelX) / 2;
                int midY = (driverPixelY + customerPixelY) / 2;

                Font distanceFont = new Font("Arial", 9, FontStyle.Bold);
                string distanceText = $"{distance:F1}";
                SizeF textSize = g.MeasureString(distanceText, distanceFont);

                // White background
                g.FillRectangle(Brushes.White,
                    midX - textSize.Width / 2 - 2,
                    midY - textSize.Height / 2 - 2,
                    textSize.Width + 4,
                    textSize.Height + 4);

                // Text
                g.DrawString(distanceText, distanceFont, Brushes.Red,
                    midX - textSize.Width / 2,
                    midY - textSize.Height / 2);
            }
        }

        private void DrawMarkers(Graphics g)
        {
            int driverPixelX = PADDING + state.DriverX * CELL_SIZE;
            int driverPixelY = PADDING + state.DriverY * CELL_SIZE;

            // Draw branch (green square) if assigned
            if (state.BranchX >= 0 && state.BranchY >= 0)
            {
                int branchPixelX = PADDING + state.BranchX * CELL_SIZE;
                int branchPixelY = PADDING + state.BranchY * CELL_SIZE;

                // Green square
                g.FillRectangle(Brushes.Green,
                    branchPixelX - 6, branchPixelY - 6, 12, 12);

                // Border
                g.DrawRectangle(new Pen(Color.DarkGreen, 2),
                    branchPixelX - 6, branchPixelY - 6, 12, 12);
            }

            // Customer marker
            if (state.TargetX >= 0 && state.TargetY >= 0 && !state.IsReturningToBranch)
            {
                int customerPixelX = PADDING + state.TargetX * CELL_SIZE;
                int customerPixelY = PADDING + state.TargetY * CELL_SIZE;

                GraphicsPath customerPath = new GraphicsPath();
                customerPath.AddEllipse(customerPixelX - 7, customerPixelY - 7, 14, 14);

                PathGradientBrush customerBrush = new PathGradientBrush(customerPath);
                customerBrush.CenterColor = Color.Blue;
                customerBrush.SurroundColors = new Color[] { Color.DarkBlue };

                g.FillPath(customerBrush, customerPath);
                g.DrawEllipse(new Pen(Color.DarkBlue, 2),
                    customerPixelX - 7, customerPixelY - 7, 14, 14);
            }

            // Driver marker
            GraphicsPath driverPath = new GraphicsPath();
            driverPath.AddEllipse(driverPixelX - 7, driverPixelY - 7, 14, 14);

            PathGradientBrush driverBrush = new PathGradientBrush(driverPath);
            driverBrush.CenterColor = Color.Red;
            driverBrush.SurroundColors = new Color[] { Color.DarkRed };

            g.FillPath(driverBrush, driverPath);
            g.DrawEllipse(new Pen(Color.DarkRed, 2),
                driverPixelX - 7, driverPixelY - 7, 14, 14);
        }

        private void DrawArrivalAnimation(Graphics g)
        {
            if (state.DriverX == state.TargetX &&
                state.DriverY == state.TargetY &&
                state.TargetX >= 0)
            {
                int arrivedPixelX = PADDING + state.DriverX * CELL_SIZE;
                int arrivedPixelY = PADDING + state.DriverY * CELL_SIZE;

                Pen arrivalPen1 = new Pen(Color.LimeGreen, 3);
                Pen arrivalPen2 = new Pen(Color.FromArgb(100, Color.LimeGreen), 2);

                g.DrawEllipse(arrivalPen1,
                    arrivedPixelX - 12, arrivedPixelY - 12, 24, 24);

                g.DrawEllipse(arrivalPen2,
                    arrivedPixelX - 18, arrivedPixelY - 18, 36, 36);
            }
        }
    }
}
