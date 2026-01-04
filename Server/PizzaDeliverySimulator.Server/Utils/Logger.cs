namespace PizzaDeliverySimulator.Server.Utils
{
    /// <summary>
    /// Thread-safe console logger with color-coded messages
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Log informational message (white text)
        /// </summary>
        public static void Info(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] INFO: {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log success message (green text)
        /// </summary>
        public static void Success(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SUCCESS: {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log warning message (yellow text)
        /// </summary>
        public static void Warning(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] WARNING: {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log error message (red text)
        /// </summary>
        public static void Error(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR: {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Print a separator line
        /// </summary>
        public static void Separator()
        {
            lock (_lock)
            {
                Console.WriteLine(new string('=', 60));
            }
        }

        /// <summary>
        /// Print header with title
        /// </summary>
        public static void Header(string title)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Separator();
                Console.WriteLine($"  {title}");
                Separator();
                Console.ResetColor();
            }
        }
    }
}
