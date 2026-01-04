namespace PizzaDeliverySimulator.Server.Models
{
    /// <summary>
    /// Represents a pizza restaurant branch
    /// </summary>
    public class Branch
    {
        public string BranchId { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Branch(string id, string name, int x, int y)
        {
            BranchId = id;
            Name = name;
            X = x;
            Y = y;
        }
    }
}
