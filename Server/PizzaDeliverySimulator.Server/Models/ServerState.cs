namespace PizzaDeliverySimulator.Server.Models
{
    /// <summary>
    /// Singleton class managing server state (drivers, customers, orders)
    /// Thread-safe implementation for async operations
    /// </summary>
    public class ServerState
    {
        private static ServerState _instance;
        private static readonly object _lock = new object();

        // Collections for tracking entities
        public Dictionary<string, Driver> Drivers { get; private set; }
        public Dictionary<string, Customer> Customers { get; private set; }
        public Dictionary<string, Order> Orders { get; private set; }

        // Mapping client endpoints to IDs
        private Dictionary<string, string> clientEndpointToDriverId;
        private Dictionary<string, string> clientEndpointToCustomerId;

        /// <summary>
        /// Private constructor (Singleton pattern)
        /// </summary>
        private ServerState()
        {
            Drivers = new Dictionary<string, Driver>();
            Customers = new Dictionary<string, Customer>();
            Orders = new Dictionary<string, Order>();
            clientEndpointToDriverId = new Dictionary<string, string>();
            clientEndpointToCustomerId = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get singleton instance (thread-safe)
        /// </summary>
        public static ServerState Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ServerState();
                    return _instance;
                }
            }
        }

        // ===== DRIVER MANAGEMENT =====

        /// <summary>
        /// Add driver to server state
        /// </summary>
        public void AddDriver(Driver driver, string clientEndpoint)
        {
            lock (_lock)
            {
                Drivers[driver.DriverId] = driver;
                clientEndpointToDriverId[clientEndpoint] = driver.DriverId;
            }
        }

        /// <summary>
        /// Get driver by ID
        /// </summary>
        public Driver GetDriver(string driverId)
        {
            lock (_lock)
            {
                return Drivers.ContainsKey(driverId) ? Drivers[driverId] : null;
            }
        }

        /// <summary>
        /// Get driver by client endpoint
        /// </summary>
        public Driver GetDriverByEndpoint(string clientEndpoint)
        {
            lock (_lock)
            {
                if (clientEndpointToDriverId.ContainsKey(clientEndpoint))
                {
                    string driverId = clientEndpointToDriverId[clientEndpoint];
                    return GetDriver(driverId);
                }
                return null;
            }
        }

        /// <summary>
        /// Get first available driver (ready and no active order)
        /// </summary>
        public Driver GetAvailableDriver()
        {
            lock (_lock)
            {
                return Drivers.Values.FirstOrDefault(d => d.IsReady && !d.HasActiveOrder());
            }
        }

        /// <summary>
        /// Remove driver
        /// </summary>
        public void RemoveDriver(string driverId)
        {
            lock (_lock)
            {
                if (Drivers.ContainsKey(driverId))
                {
                    Drivers.Remove(driverId);

                    // Remove from endpoint mapping
                    var endpoint = clientEndpointToDriverId.FirstOrDefault(x => x.Value == driverId).Key;
                    if (endpoint != null)
                        clientEndpointToDriverId.Remove(endpoint);
                }
            }
        }

        // ===== CUSTOMER MANAGEMENT =====

        /// <summary>
        /// Add customer to server state
        /// </summary>
        public void AddCustomer(Customer customer, string clientEndpoint)
        {
            lock (_lock)
            {
                Customers[customer.CustomerId] = customer;
                clientEndpointToCustomerId[clientEndpoint] = customer.CustomerId;
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        public Customer GetCustomer(string customerId)
        {
            lock (_lock)
            {
                return Customers.ContainsKey(customerId) ? Customers[customerId] : null;
            }
        }

        /// <summary>
        /// Get customer by client endpoint
        /// </summary>
        public Customer GetCustomerByEndpoint(string clientEndpoint)
        {
            lock (_lock)
            {
                if (clientEndpointToCustomerId.ContainsKey(clientEndpoint))
                {
                    string customerId = clientEndpointToCustomerId[clientEndpoint];
                    return GetCustomer(customerId);
                }
                return null;
            }
        }

        /// <summary>
        /// Remove customer
        /// </summary>
        public void RemoveCustomer(string customerId)
        {
            lock (_lock)
            {
                if (Customers.ContainsKey(customerId))
                {
                    Customers.Remove(customerId);

                    // Remove from endpoint mapping
                    var endpoint = clientEndpointToCustomerId.FirstOrDefault(x => x.Value == customerId).Key;
                    if (endpoint != null)
                        clientEndpointToCustomerId.Remove(endpoint);
                }
            }
        }

        // ===== ORDER MANAGEMENT =====

        /// <summary>
        /// Add order to server state
        /// </summary>
        public void AddOrder(Order order)
        {
            lock (_lock)
            {
                Orders[order.OrderId] = order;
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        public Order GetOrder(string orderId)
        {
            lock (_lock)
            {
                return Orders.ContainsKey(orderId) ? Orders[orderId] : null;
            }
        }

        /// <summary>
        /// Remove order
        /// </summary>
        public void RemoveOrder(string orderId)
        {
            lock (_lock)
            {
                if (Orders.ContainsKey(orderId))
                    Orders.Remove(orderId);
            }
        }

        // ===== STATISTICS =====

        /// <summary>
        /// Get server statistics
        /// </summary>
        public (int drivers, int customers, int activeOrders, int completedOrders) GetStats()
        {
            lock (_lock)
            {
                int activeOrders = Orders.Values.Count(o => o.Status != OrderStatus.Delivered);
                int completedOrders = Orders.Values.Count(o => o.Status == OrderStatus.Delivered);

                return (Drivers.Count, Customers.Count, activeOrders, completedOrders);
            }
        }

        /// <summary>
        /// Print server statistics to console
        /// </summary>
        public void PrintStats()
        {
            lock (_lock)
            {
                var stats = GetStats();
                Console.WriteLine();
                Console.WriteLine("=== SERVER STATISTICS ===");
                Console.WriteLine($"Connected Drivers: {stats.drivers}");
                Console.WriteLine($"Connected Customers: {stats.customers}");
                Console.WriteLine($"Active Orders: {stats.activeOrders}");
                Console.WriteLine($"Completed Orders: {stats.completedOrders}");
                Console.WriteLine("========================");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Available pizza branches (seed data)
        /// </summary>
        public static List<Branch> Branches = new List<Branch>
        {
            new Branch("BR01", "Downtown_Branch", 10, 10),
            new Branch("BR02", "Uptown_Branch", 40, 40),
            new Branch("BR03", "Westside_Branch", 5, 25),
            new Branch("BR04", "Eastside_Branch", 45, 25),
            new Branch("BR05", "North_Branch", 25, 5),
            new Branch("BR06", "South_Branch", 25, 45),
            new Branch("BR07", "Central_Branch", 25, 25),
            new Branch("BR08", "Riverside_Branch", 15, 35),
            new Branch("BR09", "Hillside_Branch", 35, 15),
            new Branch("BR10", "Lakeside_Branch", 30, 30)
        };

        /// <summary>
        /// Get random branch for driver assignment
        /// </summary>
        public static Branch GetRandomBranch()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int index = random.Next(0, Branches.Count);
            return Branches[index];
        }
    }
}
