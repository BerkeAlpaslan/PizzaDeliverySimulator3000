# 🍕 Pizza Delivery Simulator 3000

A real-time multi-driver coordination system demonstrating TCP/UDP network programming, built with .NET 8.0 and C#.

**🔗 GitHub Repository:** [https://github.com/BerkeAlpaslan/PizzaDeliverySimulator3000.git]

---

## 📂 Project Structure

```
PizzaDeliverySimulator.sln
│
├── Client/
│   ├── PizzaDeliverySimulator.Customer/    # Customer GUI (WinForms)
│   │   ├── Controllers/
│   │   │   ├── OrderController.cs
│   │   │   ├── TrackingController.cs
│   │   │   └── UIController.cs
│   │   ├── Models/
│   │   │   └── CustomerState.cs
│   │   ├── Services/
│   │   │   └── NetworkManager.cs
│   │   ├── Rendering/
│   │   │   └── MapRenderer.cs
│   │   └── Form1.cs                         # Main form
│   │
│   └── PizzaDeliverySimulator.Driver/       # Driver GUI (WinForms)
│       ├── Controllers/
│       │   ├── MovementController.cs
│       │   ├── OrderHandler.cs
│       │   └── UIController.cs
│       ├── Models/
│       │   └── DriverState.cs
│       ├── Services/
│       │   └── NetworkManager.cs
│       ├── Rendering/
│       │   └── MapRenderer.cs
│       └── Form1.cs                         # Main form
│
├── Common/
│   └── PizzaDeliverySimulator.Common/       # Shared library
│       ├── Protocol.cs                      # TCP/UDP protocol constants
│       └── PizzaType.cs                     # Pizza type enum
│
└── Server/
    └── PizzaDeliverySimulator.Server/       # Console server
        ├── Core/
        │   └── ServerCore.cs                # TCP listener + async callbacks
        ├── Models/
        │   ├── Driver.cs
        │   ├── Customer.cs
        │   ├── Order.cs
        │   └── Branch.cs
        ├── Services/
        │   ├── ClientHandler.cs             # Per-client TCP handler
        │   ├── OrderManager.cs              # Order assignment logic
        │   └── LocationBroadcaster.cs       # UDP broadcaster thread
        ├── Utils/
        │   └── Logger.cs                    # Colored console logging
        └── Program.cs
```

---

## 🔧 Visual Studio Tips

### **⚠️ Accessing Form1.cs Code (Important!)**

**Problem:** Clicking `Form1.cs` in Solution Explorer opens the **GUI Designer**, not the code!

**Solutions:**

1. **Method 1 (Quick & Easy):**
   - Open GUI Designer (double-click `Form1.cs`)
   - Double-click **ANY button/control** in the designer
   - Visual Studio automatically jumps to `Form1.cs` code (event handler method)
   - ✅ Now you can see the full code!

2. **Method 2 (Direct Access):**
   - Right-click `Form1.cs` in Solution Explorer
   - Select **"View Code"** (or press `F7`)

3. **Method 3 (Expand Tree):**
   - Click the ► arrow next to `Form1.cs` in Solution Explorer
   - You'll see `Form1.Designer.cs` underneath
   - Click on `Form1.cs` itself (not Designer)

**Why this matters:** 
- The GUI designer is for **visual editing** (drag-drop controls)
- To see **actual implementation** (controllers, network events, timers), you need **code view**
- Many instructors may not be familiar with WinForms workflow!

---

## 🚀 Quick Start

### **1. Start Server**
```bash
# Run from Visual Studio or:
cd Server/PizzaDeliverySimulator.Server
dotnet run
```

Server starts on:
- TCP: `localhost:9050`
- UDP: `localhost:9051`

### **2. Start Driver(s)**
```bash
# Run from Visual Studio or:
cd Client/PizzaDeliverySimulator.Driver
dotnet run
```

1. Enter driver name
2. Press Enter to register
3. Click **READY** to accept orders

### **3. Start Customer(s)**
```bash
# Run from Visual Studio or:
cd Client/PizzaDeliverySimulator.Customer
dotnet run
```

1. Enter customer name
2. Press Enter to register
3. Select pizza type
4. Enter address
5. Click **ORDER**

---

## 🎯 Key Features

### **Server (Console)**
- ✅ Multi-client TCP server (APM pattern: `BeginAcceptTcpClient/EndAcceptTcpClient`)
- ✅ Async message handling (`BeginRead/EndRead` + `WriteLineAsync`)
- ✅ UDP location broadcaster (dedicated thread, 2-second intervals)
- ✅ Thread-safe state management (Singleton pattern with locks)
- ✅ 10 branch locations with random driver assignment
- ✅ Automatic order assignment (first available driver)
- ✅ Satisfaction scoring (time-based: 1-5 stars)

### **Driver GUI (WinForms)**
- ✅ Autonomous movement simulation (2-4 units/2s, vector-based pathfinding)
- ✅ Real-time 50x50 grid map with movement trails
- ✅ Automatic branch return after delivery (with `NOTREADY` enforcement)
- ✅ READY button locked during return journey
- ✅ Event-driven architecture (Observer pattern)
- ✅ TCP command transmission + UDP location broadcasting

### **Customer GUI (WinForms)**
- ✅ 8 pizza types (Margherita, Pepperoni, Hawaiian, etc.)
- ✅ Real-time driver tracking (UDP filtering by OrderId)
- ✅ Distance calculation and ETA display
- ✅ Order status notifications
- ✅ Satisfaction score display after delivery

---

## 🌐 Network Protocol

### **TCP Commands (Port 9050)**

**Driver → Server:**
```
REGISTER:DriverName
READY
NOTREADY
OUTFORDELIVERY:OrderId
ARRIVED:OrderId:X:Y
DELIVERED:OrderId
LOCATION:X:Y
```

**Customer → Server:**
```
REGISTER_CUSTOMER:Name
ORDER:PizzaType:Address
```

**Server → Clients:**
```
REGISTERED:DriverId:X:Y:BranchId:BranchName
ORDER_CREATED:OrderId
ASSIGN:OrderId:PizzaType:Address:CustomerX:CustomerY
DRIVER_ASSIGNED:OrderId:Name:BranchId:BranchName:X:Y
ESTIMATED:OrderId:Seconds
SATISFACTION:OrderId:Score:ActualTime:EstimatedTime
ERROR:Message
```

### **UDP Broadcasts (Port 9051)**

**Server → All Clients:**
```
UDP_LOCATION:DriverId:X:Y:DriverName:OrderId
```

- Broadcast every 2 seconds
- Sent to `255.255.255.255:9051`
- Clients filter by `OrderId`

---

## 🏗️ Architecture Highlights

### **Design Patterns**

**Singleton Pattern:**
- `ServerState.Instance` - Thread-safe server state management
- Single source of truth for drivers, customers, orders

**Observer Pattern:**
- Event-driven GUI (Driver & Customer)
- Controllers expose events: `OnMovementUpdate`, `OnOrderAssigned`, `OnArrivedAtCustomer`
- UI components subscribe without direct dependencies

**MVC-Style Separation:**
- **Models:** State classes (DriverState, CustomerState)
- **Controllers:** Business logic (MovementController, OrderController, TrackingController)
- **Views:** Form1.cs (minimal, orchestrates controllers)

### **Async Programming**

**Server:**
- APM (Asynchronous Programming Model): `BeginAcceptTcpClient/EndAcceptTcpClient`
- Callback-based connection handling
- `WriteLineAsync` for non-blocking message transmission

**Clients:**
- Modern async/await patterns
- `SendCommandAsync()` for TCP commands
- Background UDP listeners with async loops

### **Thread Safety**

**Server:**
```csharp
private static readonly object _lock = new object();

public void AddDriver(Driver driver)
{
    lock (_lock)  // Protects Dictionary from concurrent access
    {
        Drivers[driver.DriverId] = driver;
    }
}
```

**Why locks?**
- Multiple client callback threads
- UDP broadcaster background thread
- Concurrent dictionary modifications would cause exceptions

---

## 🎮 Testing Scenarios

### **Scenario 1: Basic Delivery Flow**
1. Start Server
2. Start Driver "John" → Click READY
3. Start Customer "Alice" → Order Pepperoni
4. Watch autonomous movement
5. Driver arrives → Click DELIVERED
6. Both see satisfaction score
7. Driver auto-returns to branch

### **Scenario 2: Multi-Driver Concurrent**
1. Start Server
2. Start 3 Drivers (all READY)
3. Start 2 Customers
4. Both customers order simultaneously
5. Observe:
   - First 2 drivers get orders
   - Third driver remains ready
   - Independent UDP streams
   - No notification cross-contamination

### **Scenario 3: UDP Port Sharing**
1. Start Server
2. Start 3 Customers on same machine
3. All bind to UDP 9051 (thanks to `ReuseAddress`)
4. Each filters broadcasts by their OrderId
5. No "address already in use" errors

---

## 🔧 Technical Challenges Solved

### **1. UDP Port Conflict**
**Problem:** Multiple customers on same machine → "Address already in use"

**Solution:**
```csharp
udpClient.Client.SetSocketOption(
    SocketOptionLevel.Socket, 
    SocketOptionName.ReuseAddress, true);
udpClient.ExclusiveAddressUse = false;
```

### **2. Thread Safety**
**Problem:** Race conditions, dictionary modification exceptions

**Solution:** Lock-based synchronization on all `ServerState` access

### **3. Cross-Thread UI Updates**
**Problem:** `InvalidOperationException` from background threads

**Solution:**
```csharp
if (control.InvokeRequired)
{
    control.Invoke(() => UpdateUI());
}
```

### **4. Duplicate Event Subscriptions**
**Problem:** Notifications appearing twice

**Solution:** Careful event wiring (subscribe only once per event)

### **5. Branch Return Mechanism**
**Problem:** Drivers accepting orders while far from branch

**Solution:**
- Auto-send `NOTREADY` when starting return journey
- Lock READY button until branch arrival (distance < 3 units)
- Only re-enable button upon reaching branch

---

## 📊 Performance

- **Max Tested Clients:** 10 (5 drivers + 5 customers)
- **TCP Response Time:** <50ms (localhost)
- **UDP Broadcast Rate:** Exactly 2.0s intervals
- **Movement Update Rate:** 2 seconds (smooth animation)

---

## 🚨 Known Issues

### **Arrival Notification Spam (Cosmetic)**
**Issue:** When driver reaches customer (distance < 3), UDP broadcasts continue every 2s showing "Distance: 0.0" until DELIVERED clicked.

**Cause:** UDP broadcaster checks `driver.HasActiveOrder()` (order still active until DELIVERED).

**Impact:** Visual clutter for 5-10 seconds, no functional impact.

**Status:** Accepted trade-off (fixing adds complexity for minimal benefit).

---

## 🔮 Future Enhancements

**State Persistence:**
- PostgreSQL + Entity Framework Core
- Redis caching for active orders
- Order history and analytics

**Networking:**
- WebSocket support for browser clients
- QUIC protocol (UDP-based reliable transport)
- Authentication & TLS/SSL encryption

**Features:**
- Multiple pizzas per order
- Order cancellation
- Real-time traffic simulation
- Customer rating of drivers
- Push notifications

**Scalability:**
- Load balancing across multiple servers
- Microservices architecture
- Auto-reconnection with exponential backoff

---

## 📚 Technologies

- **.NET 8.0** - Framework
- **C# 12** - Language
- **Windows Forms** - GUI framework
- **System.Net.Sockets** - Networking (TcpListener, UdpClient)
- **Visual Studio 2022** - IDE

---

## 🎓 Educational Value

This project demonstrates:
- ✅ TCP vs UDP protocol selection (hybrid architecture)
- ✅ Asynchronous programming (APM + async/await)
- ✅ Multi-threading and thread safety
- ✅ Event-driven architecture (Observer pattern)
- ✅ State management (Singleton pattern)
- ✅ Network protocol design
- ✅ Real-time system coordination
- ✅ Windows Forms GUI development

---

## 📖 References

- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- Richard Blum, *C# Network Programming*, Sybex, 2002

---

## 👤 Author

**Berke Alpaslan**  
Computer Engineering Student  
Manisa Celal Bayar University  
Fall 2025-2026
CSE3233 - Computer Network Programming
