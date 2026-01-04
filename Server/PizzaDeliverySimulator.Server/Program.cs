using PizzaDeliverySimulator.Server.Core;
using PizzaDeliverySimulator.Server.Utils;

namespace PizzaDeliveryServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Create and start server
                ServerCore server = new ServerCore();
                server.Start();

                Logger.Info("Press 'Q' to stop the server...");

                // Keep server running until 'Q' is pressed
                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        Logger.Warning("Shutting down server...");
                        server.Stop();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Fatal error: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}