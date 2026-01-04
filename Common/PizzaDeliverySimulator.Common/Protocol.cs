namespace PizzaDeliverySimulator.Common
{
    /// <summary>
    /// Network protocol constants for Pizza Delivery Simulator
    /// Message format: COMMAND:param1:param2:param3
    /// </summary>
    public static class Protocol
    {
        // ===== COMMANDS - Customer to Server =====
        public const string REGISTER_CUSTOMER = "REGISTER_CUSTOMER"; // REGISTER_CUSTOMER:CustomerName
        public const string ORDER = "ORDER";           // ORDER:PizzaType:Address
        public const string STATUS = "STATUS";         // STATUS:OrderId

        // ===== COMMANDS - Driver to Server =====
        public const string REGISTER = "REGISTER";     // REGISTER:DriverName
        public const string READY = "READY";           // READY
        public const string NOTREADY = "NOTREADY";     // NOTREADY (pause/break)
        public const string OUTFORDELIVERY = "OUTFORDELIVERY"; // OUTFORDELIVERY:OrderId
        public const string ARRIVED = "ARRIVED";       // ARRIVED:OrderId:X:Y (driver reached customer)
        public const string DELIVERED = "DELIVERED";   // DELIVERED:OrderId
        public const string LOCATION = "LOCATION";     // LOCATION:X:Y

        // ===== COMMANDS - Common =====
        public const string DISCONNECT = "DISCONNECT"; // DISCONNECT

        // ===== RESPONSES - Server to Clients =====
        public const string ORDER_CREATED = "ORDER_CREATED";     // ORDER_CREATED:OrderId
        public const string REGISTERED = "REGISTERED";           // REGISTERED:DriverId or CustomerId
        public const string ASSIGN = "ASSIGN";                   // ASSIGN:OrderId:PizzaType:Address:X:Y
        public const string ACCEPTED = "ACCEPTED";               // ACCEPTED
        public const string DRIVER_ASSIGNED = "DRIVER_ASSIGNED"; // DRIVER_ASSIGNED:OrderId:DriverName:BranchId:BranchName:X:Y
        public const string DRIVER_ARRIVED = "DRIVER_ARRIVED";   // DRIVER_ARRIVED:OrderId:DriverName:X:Y
        public const string STATUS_UPDATE = "STATUS_UPDATE";     // STATUS_UPDATE:OrderId:Status
        public const string ESTIMATED = "ESTIMATED";             // ESTIMATED:OrderId:Seconds
        public const string SATISFACTION = "SATISFACTION";       // SATISFACTION:OrderId:Score:ActualTime:EstimatedTime
        public const string ERROR = "ERROR";                     // ERROR:Message

        // ===== UDP Broadcasts =====
        public const string UDP_LOCATION = "UDP_LOCATION";       // UDP_LOCATION:DriverId:X:Y:DriverName:OrderId

        // ===== Delimiters =====
        public const char DELIMITER = ':';

        // ===== Default Host =====
        public const string DEFAULT_HOST = "127.0.0.1";

        // ===== Network Ports =====
        public const int TCP_PORT = 9050;
        public const int UDP_PORT = 9051;

        // ===== Message Validation =====
        /// <summary>
        /// Check if command is valid
        /// </summary>
        public static bool IsValidCommand(string command)
        {
            return command == REGISTER ||
                   command == REGISTER_CUSTOMER ||
                   command == READY ||
                   command == NOTREADY ||
                   command == ORDER ||
                   command == STATUS ||
                   command == OUTFORDELIVERY ||
                   command == ARRIVED ||
                   command == DELIVERED ||
                   command == LOCATION ||
                   command == DISCONNECT;
        }
    }
}
