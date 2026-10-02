using System;

namespace FunctionalDeliveryCalculator
{
    // the two enums the assignment asked for
    enum DeliveryType
    {
        Pickup,
        Courier,
        DoorToDoor
    }

    enum DeliveryZone
    {
        City,
        OutsideCity,
        Remote
    }

    class Program
    {
        // ---------------------------------------------------------
        // Main = the only place that talks to the console.
        // It reads input, calls the calculation functions, and
        // prints the result. It does not do any price math itself.
        // ---------------------------------------------------------
        static void Main(string[] args)
        {
            Console.WriteLine("=== Delivery Cost Calculator ===");
            Console.WriteLine();

            // 1. base price
            Console.Write("Enter base delivery price: ");
            string? priceInput = Console.ReadLine();
            if (!decimal.TryParse(priceInput, out decimal basePrice))
            {
                Console.WriteLine("Error: base price must be a valid number (example: 1500 or 1500.50).");
                return; // early return on bad input
            }

            if (basePrice < 0)
            {
                Console.WriteLine("Error: base price cannot be negative.");
                return;
            }

            // 2. number of items
            Console.Write("Enter number of items: ");
            string? itemsInput = Console.ReadLine();
            if (!int.TryParse(itemsInput, out int itemCount))
            {
                Console.WriteLine("Error: number of items must be a whole number.");
                return;
            }

            if (itemCount <= 0)
            {
                Console.WriteLine("Error: number of items must be at least 1.");
                return;
            }

            // 3. express delivery status
            Console.Write("Is this express delivery? (true/false): ");
            string? expressInput = Console.ReadLine();
            if (!bool.TryParse(expressInput, out bool isExpress))
            {
                Console.WriteLine("Error: express status must be true or false.");
                return;
            }

            // 4. delivery type
            Console.Write("Enter delivery type (Pickup, Courier, DoorToDoor): ");
            string? typeInput = Console.ReadLine();
            if (!Enum.TryParse<DeliveryType>(typeInput, true, out DeliveryType deliveryType))
            {
                Console.WriteLine("Error: unknown delivery type. Use Pickup, Courier or DoorToDoor.");
                return;
            }

            // 5. delivery zone
            Console.Write("Enter delivery zone (City, OutsideCity, Remote): ");
            string? zoneInput = Console.ReadLine();
            if (!Enum.TryParse<DeliveryZone>(zoneInput, true, out DeliveryZone deliveryZone))
            {
                Console.WriteLine("Error: unknown delivery zone. Use City, OutsideCity or Remote.");
                return;
            }

            // all input is valid now, hand it off to the pure calculation part
            decimal finalPrice = PriceCalculator.CalculateFinalPrice(
                basePrice, itemCount, deliveryType, deliveryZone, isExpress);

            Console.WriteLine();
            Console.WriteLine($"Final delivery price: {finalPrice:F2}");
        }
    }

    // ---------------------------------------------------------
    // PriceCalculator only does math. No Console.Read / Console.Write
    // anywhere in here, and no mutable fields - every method just
    // takes values in and returns a value out.
    // ---------------------------------------------------------
    static class PriceCalculator
    {
        // named static method #1
        // this is the function that puts all the rules together,
        // following the order: base -> items -> type -> zone -> express
        public static decimal CalculateFinalPrice(
            decimal basePrice,
            int itemCount,
            DeliveryType type,
            DeliveryZone zone,
            bool isExpress)
        {
            decimal price = basePrice;

            price = ApplyRule(price, GetItemCountRule(itemCount));
            price = ApplyRule(price, GetDeliveryTypeRule(type));

            // two Func<decimal, decimal> values declared directly,
            // just to show Func being used as a normal variable too
            Func<decimal, decimal> zoneRule = GetDeliveryZoneRule(zone);
            Func<decimal, decimal> expressRule = isExpress
                ? (p => p * 1.30m)      // lambda expression
                : (p => p);

            price = ApplyRule(price, zoneRule);
            price = ApplyRule(price, expressRule);

            return Math.Round(price, 2);
        }

        // ---------------------------------------------------------
        // This is the required higher-order function: it receives a
        // decimal and a pricing function (Func<decimal, decimal>),
        // applies it, and returns the new decimal. Expression-bodied.
        // ---------------------------------------------------------
        static decimal ApplyRule(decimal price, Func<decimal, decimal> rule) => rule(price);

        // named static method #2
        // returns the right rule depending on how many items there are
        static Func<decimal, decimal> GetItemCountRule(int itemCount)
        {
            if (itemCount >= 1 && itemCount <= 3)
            {
                return price => price; // no adjustment
            }
            else if (itemCount >= 4 && itemCount <= 7)
            {
                return price => price * 1.10m; // +10%
            }
            else // 8 or more
            {
                return price => price * 1.20m; // +20%
            }
        }

        // uses a switch expression (conditional logic requirement)
        static Func<decimal, decimal> GetDeliveryTypeRule(DeliveryType type) =>
            type switch
            {
                DeliveryType.Pickup => price => price * 0.80m,     // -20%
                DeliveryType.Courier => price => price,            // no change
                DeliveryType.DoorToDoor => price => price * 1.15m, // +15%
                _ => price => price
            };

        // the assignment only gives explicit rules for City (no change)
        // and OutsideCity (+25%). Remote is not covered by the rules in
        // the task, so it is treated the same as "no extra charge" here.
        // (see README notes about this assumption)
        static Func<decimal, decimal> GetDeliveryZoneRule(DeliveryZone zone)
        {
            switch (zone)
            {
                case DeliveryZone.City:
                    return price => price;
                case DeliveryZone.OutsideCity:
                    return price => price * 1.25m;
                case DeliveryZone.Remote:
                    return price => price; // not defined by the assignment rules
                default:
                    return price => price;
            }
        }
    }
}
