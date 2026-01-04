namespace PizzaDeliverySimulator.Common
{
    /// <summary>
    /// Available pizza types
    /// </summary>
    public enum PizzaType
    {
        Margherita,
        Pepperoni,
        Hawaiian,
        Veggie,
        MeatLovers,
        BBQChicken,
        Supreme,
        FourCheese
    }

    /// <summary>
    /// Extension methods for PizzaType
    /// </summary>
    public static class PizzaTypeExtensions
    {
        /// <summary>
        /// Get display name for pizza type
        /// </summary>
        public static string GetDisplayName(this PizzaType pizza)
        {
            return pizza switch
            {
                PizzaType.Margherita => "Margherita",
                PizzaType.Pepperoni => "Pepperoni",
                PizzaType.Hawaiian => "Hawaiian",
                PizzaType.Veggie => "Veggie",
                PizzaType.MeatLovers => "Meat Lovers",
                PizzaType.BBQChicken => "BBQ Chicken",
                PizzaType.Supreme => "Supreme",
                PizzaType.FourCheese => "Four Cheese",
                _ => pizza.ToString()
            };
        }

        /// <summary>
        /// Get all pizza types for dropdowns/menus
        /// </summary>
        public static PizzaType[] GetAll()
        {
            return new[]
            {
                PizzaType.Margherita,
                PizzaType.Pepperoni,
                PizzaType.Hawaiian,
                PizzaType.Veggie,
                PizzaType.MeatLovers,
                PizzaType.BBQChicken,
                PizzaType.Supreme,
                PizzaType.FourCheese
            };
        }

        /// <summary>
        /// Parse pizza type from string (case-insensitive)
        /// </summary>
        public static bool TryParse(string pizzaName, out PizzaType pizzaType)
        {
            // Remove spaces and try to parse
            string normalized = pizzaName.Replace(" ", "");
            return Enum.TryParse(normalized, ignoreCase: true, out pizzaType);
        }

        /// <summary>
        /// Check if pizza name is valid
        /// </summary>
        public static bool IsValid(string pizzaName)
        {
            return TryParse(pizzaName, out _);
        }

        /// <summary>
        /// Get PizzaType from display name
        /// </summary>
        public static PizzaType FromDisplayName(string displayName)
        {
            return GetAll().FirstOrDefault(p => p.GetDisplayName() == displayName);
        }
    }
}
