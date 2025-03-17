using System;
using System.Collections.Generic;

class OrderItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
}

class OrderCalculator
{
    public static double CalculateTotalPrice(List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            return 0.0;

        double totalBeforeDiscount = 0.0;

        foreach (var item in items)
        {
            double itemTotal = item.Quantity * item.UnitPrice;

            // Apply 10% discount for 3 or more units of the same item
            if (item.Quantity >= 3)
            {
                itemTotal *= 0.9;
            }

            totalBeforeDiscount += itemTotal;
        }

        // Apply 5% discount if total before discount exceeds $100
        if (totalBeforeDiscount > 100)
        {
            totalBeforeDiscount *= 0.95;
        }

        return Math.Round(totalBeforeDiscount, 2);
    }
}

class Program
{
    static void Main()
    {
        List<OrderItem> order = new List<OrderItem>();

        Console.Write("Enter the number of items in the order: ");
        int itemCount = int.Parse(Console.ReadLine());

        for (int i = 0; i < itemCount; i++)
        {
            Console.Write("Enter item name: ");
            string name = Console.ReadLine();

            Console.Write("Enter quantity: ");
            int quantity = int.Parse(Console.ReadLine());

            Console.Write("Enter unit price: ");
            double unitPrice = double.Parse(Console.ReadLine());

            order.Add(new OrderItem { Name = name, Quantity = quantity, UnitPrice = unitPrice });
        }

        double totalPrice = OrderCalculator.CalculateTotalPrice(order);
        Console.WriteLine($"Total Price after discounts: ${totalPrice}");
    }
}
