

using S10272509G_PRG2Assignment;
using System.Globalization;

// ==============================
// Global Data Structures
// ==============================
List<Restaurant> restaurants = new List<Restaurant>();
List<Customer> customers = new List<Customer>();
List<Order> allOrders = new List<Order>();

// Refund stack for rejected/cancelled orders (LIFO)
Stack<Order> refundStack = new Stack<Order>();

// ==========================================================
// FEATURE 0 — Main Menu UI
// ==========================================================
static void ShowMenu()
{
    Console.WriteLine("\n===== Gruberoo Food Delivery System =====");
    Console.WriteLine("1. List all restaurants and menu items");
    Console.WriteLine("2. List all orders");
    Console.WriteLine("3. Create a new order");
    Console.WriteLine("4. Process an order");
    Console.WriteLine("5. Modify an existing order");
    Console.WriteLine("6. Delete an existing order");
    Console.WriteLine("7. Bulk process unprocessed orders (Advanced)");
    Console.WriteLine("8. Display total order amount (Advanced)");
    Console.WriteLine("0. Exit");
    Console.Write("\nEnter your choice: ");
}

// ==========================================================
// DATA LOADING — CSV Loaders (Startup)
// ==========================================================
static void LoadRestaurants(List<Restaurant> restaurants)
{
    try
    {
        string[] lines = File.ReadAllLines("restaurants.csv");
        int count = 0;

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 3) continue;

            Restaurant r = new Restaurant(parts[0].Trim(), parts[1].Trim(), parts[2].Trim());
            restaurants.Add(r);
            count++;
        }

        Console.WriteLine($"{count} restaurants loaded!");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine("Error: restaurants.csv not found.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Error: No permission to read restaurants.csv.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error loading restaurants: " + ex.Message);
    }
}

static void LoadFoodItems(List<Restaurant> restaurants)
{
    try
    {
        string[] lines = File.ReadAllLines("fooditems.csv");
        int count = 0;

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 4) continue;

            string restId = parts[0].Trim();
            string itemName = parts[1].Trim();
            string itemDesc = parts[2].Trim();

            if (!double.TryParse(parts[3].Trim(), out double itemPrice)) continue;

            Restaurant r = restaurants.FirstOrDefault(x => x.restaurantId == restId);
            if (r == null) continue;

            // Ensure restaurant has at least one menu
            if (r.menus.Count == 0)
            {
                Menu menu = new Menu(restId + "M", r.restaurantName + " Menu");
                r.AddMenu(menu);
            }

            FoodItem item = new FoodItem(itemName, itemDesc, itemPrice);
            r.menus[0].AddFoodItem(item);
            count++;
        }

        Console.WriteLine($"{count} food items loaded!");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine("Error: fooditems.csv not found.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Error: No permission to read fooditems.csv.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error loading food items: " + ex.Message);
    }
}

static void LoadCustomers(List<Customer> customers)
{
    try
    {
        string[] lines = File.ReadAllLines("customers.csv");
        int count = 0;

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 2) continue;

            Customer c = new Customer(parts[0].Trim(), parts[1].Trim());
            customers.Add(c);
            count++;
        }

        Console.WriteLine($"{count} customers loaded!");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine("Error: customers.csv not found.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Error: No permission to read customers.csv.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error loading customers: " + ex.Message);
    }
}

static void LoadOrders(List<Order> allOrders, List<Restaurant> restaurants, List<Customer> customers)
{
    try
    {
        string[] lines = File.ReadAllLines("orders.csv");
        int count = 0;

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Split basic fields (items are quoted; handled separately)
            string[] parts = line.Split(',');
            if (parts.Length < 9) continue;

            if (!int.TryParse(parts[0].Trim(), out int orderId)) continue;

            Customer c = customers.FirstOrDefault(x => x.emailAddress == parts[1].Trim());
            Restaurant r = restaurants.FirstOrDefault(y => y.restaurantId == parts[2].Trim());
            if (c == null || r == null) continue;

            // Delivery date + time in two fields
            if (!DateTime.TryParseExact(
                    parts[3].Trim() + " " + parts[4].Trim(),
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime deliveryDateTime))
                continue;

            string deliveryAddress = parts[5].Trim();

            if (!DateTime.TryParseExact(
                    parts[6].Trim(),
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime orderDateTime))
                continue;

            if (!double.TryParse(parts[7].Trim(), out double orderTotal)) continue;

            string orderStatus = parts[8].Trim();

            // Payment details may not exist in CSV (depends on brief). We set safe defaults.
            Order order = new Order(c, r, orderId, orderDateTime, orderTotal, orderStatus,
                                    deliveryDateTime, deliveryAddress, "CC", true);

            // Parse quoted item list: "...,"item1, 2|item2, 1""
            string[] quoteSplit = line.Split('"');
            if (quoteSplit.Length >= 2)
            {
                string itemsString = quoteSplit[1];
                string[] items = itemsString.Split('|', StringSplitOptions.RemoveEmptyEntries);

                foreach (string it in items)
                {
                    string[] itemParts = it.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (itemParts.Length < 2) continue;

                    string itemName = itemParts[0].Trim();
                    if (!int.TryParse(itemParts[1].Trim(), out int qty)) continue;

                    FoodItem foodItem = null;
                    foreach (Menu menu in r.menus)
                    {
                        foodItem = menu.foodItems.FirstOrDefault(x => x.itemName == itemName);
                        if (foodItem != null) break;
                    }

                    if (foodItem != null)
                        order.AddOrderedFoodItem(new OrderedFoodItem(foodItem, qty));
                }
            }

            allOrders.Add(order);
            c.AddOrder(order);
            r.orders.Enqueue(order);
            count++;
        }

        Console.WriteLine($"{count} orders loaded!");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine("Error: orders.csv not found.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Error: No permission to read orders.csv.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error loading orders: " + ex.Message);
    }
}

// ==========================================================
// FEATURE 1 — List all restaurants and menu items
// ==========================================================
static void ListRestaurants(List<Restaurant> restaurants)
{
    try
    {
        Console.Clear();
        Console.WriteLine("All Restaurants and Menu Items");
        Console.WriteLine("==============================");

        foreach (Restaurant r in restaurants)
            r.DisplayMenu();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error listing restaurants: " + ex.Message);
    }
}



// ==========================================================
// FEATURE 4 — Process an order (Confirm/Reject/Skip/Deliver)
// ==========================================================
static void ProcessOrder(List<Restaurant> restaurants, Stack<Order> refundStack)
{
    Console.WriteLine("\nProcess Order");
    Console.WriteLine("=============");
    Console.Write("Enter a restaurant ID: ");
    string restaurantid = Console.ReadLine()?.Trim();

    Restaurant r = restaurants.FirstOrDefault(x => x.restaurantId == restaurantid);
    if (r == null)
    {
        Console.WriteLine("Restaurant not found.");
        return;
    }

    if (r.orders.Count == 0)
    {
        Console.WriteLine("No orders for this restaurant.");
        return;
    }

    int queueSize = r.orders.Count; // IMPORTANT: fixed size for one full pass

    for (int i = 0; i < queueSize; i++)
    {
        Order order = r.orders.Dequeue(); // take one order out

        Console.WriteLine($"\nOrder {order.orderId}:");
        Console.WriteLine($"Customer: {order.customer.customerName}");
        Console.WriteLine("Ordered Items:");

        int count = 0;
        foreach (OrderedFoodItem item in order.orderedFoodItems)
        {
            count++;
            Console.WriteLine($"{count}. {item.itemName} - {item.qtyOrdered}");
        }

        Console.WriteLine($"Delivery date/time: {order.deliveryDateTime:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Total Amount: ${order.orderTotal:F2}");
        Console.WriteLine($"Order Status: {order.orderStatus}");

        bool archiveThisOrder = false; // archive = remove from active queue

        while (true)
        {
            Console.Write("\n[C]onfirm / [R]eject / [S]kip / [D]eliver: ");
            string choice = Console.ReadLine()?.Trim().ToUpper();

            if (choice == "C")
            {
                if (order.orderStatus == "Pending")
                {
                    order.orderStatus = "Preparing";
                    Console.WriteLine($"Order {order.orderId} confirmed. Status: {order.orderStatus}");
                    break;
                }
                Console.WriteLine("Order is not Pending. Failed to confirm.");
            }
            else if (choice == "R")
            {
                if (order.orderStatus == "Pending")
                {
                    order.orderStatus = "Rejected";
                    refundStack.Push(order);     // refund stack
                    archiveThisOrder = true;     // ARCHIVE (diagram)
                    Console.WriteLine($"Order {order.orderId} rejected. Added to refund stack and archived.");
                    break;
                }
                Console.WriteLine("Order is not Pending. Failed to reject.");
            }
            else if (choice == "D")
            {
                if (order.orderStatus == "Preparing")
                {
                    order.orderStatus = "Delivered";
                    archiveThisOrder = true;     // ARCHIVE (diagram)
                    Console.WriteLine($"Order {order.orderId} delivered. Archived.");
                    break;
                }
                Console.WriteLine("Order is not Preparing. Failed to deliver.");
            }
            else if (choice == "S")
            {
                Console.WriteLine($"Order {order.orderId} skipped.");
                break;
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
        }

        // If not archived, put it back into the queue (still active)
        if (!archiveThisOrder)
        {
            r.orders.Enqueue(order);
        }
    }
}



// ==========================================================
// FEATURE 6 — Delete (Cancel) an existing order (Pending only)
// ==========================================================
static void DeleteOrder(List<Customer> customers, Stack<Order> refundStack)
{
    Console.WriteLine("\nDelete Order");
    Console.WriteLine("============");

    try
    {
        Console.Write("Enter Customer Email: ");
        string email = Console.ReadLine()?.Trim();
        Customer c = customers.FirstOrDefault(x => x.emailAddress == email);

        if (c == null)
        {
            Console.WriteLine("Customer not found!");
            return;
        }

        var pendingOrders = c.orders.Where(o => o.orderStatus == "Pending").ToList();
        if (pendingOrders.Count == 0)
        {
            Console.WriteLine("No pending orders found.");
            return;
        }

        Console.WriteLine("Pending Orders:");
        foreach (var o in pendingOrders) Console.WriteLine(o.orderId);

        Console.Write("Enter Order ID: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId))
        {
            Console.WriteLine("Invalid Order ID.");
            return;
        }

        Order selectorder = pendingOrders.FirstOrDefault(x => x.orderId == orderId);
        if (selectorder == null)
        {
            Console.WriteLine("Order not found or not pending.");
            return;
        }

        Console.WriteLine($"\nOrder {selectorder.orderId} details:");
        selectorder.DisplayOrderedFoodItems();
        Console.WriteLine($"Delivery: {selectorder.deliveryDateTime:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Total   : ${selectorder.orderTotal:F2}");

        Console.Write("Confirm cancellation? [Y/N]: ");
        string choice = Console.ReadLine()?.Trim().ToUpper();

        if (choice == "Y")
        {
            selectorder.orderStatus = "Cancelled";
            refundStack.Push(selectorder);
            Console.WriteLine($"Order {selectorder.orderId} cancelled. Added to refund stack.");
        }
        else
        {
            Console.WriteLine("Cancellation aborted.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error deleting order: " + ex.Message);
    }
}


// ==========================================================
// FEATURE 8 — Display total order amount (Advanced)
// ==========================================================
static void ShowTotals(List<Restaurant> restaurants)
{
    double gruberooTotal = 0;
    double gruberooRefund = 0;

    foreach (Restaurant restaurant in restaurants)
    {
        double restaurantTotal = 0;
        double restaurantRefund = 0;

        foreach (Order order in restaurant.orders)
        {
            if (order.orderStatus == "Delivered")
            {
                restaurantTotal += order.CalculateOrderTotal();
            }
            else if (order.orderStatus == "Rejected" || order.orderStatus == "Cancelled")
            {
                restaurantRefund += order.CalculateOrderTotal();
            }
        }

        gruberooTotal += restaurantTotal;
        gruberooRefund += restaurantRefund;

        Console.WriteLine($"\n=== {restaurant.restaurantName} ({restaurant.restaurantId}) ===");
        Console.WriteLine($"Total Order Amount : {restaurantTotal:F2}");
        Console.WriteLine($"Total Refund Amount: {restaurantRefund:F2}");
    }

    Console.WriteLine("\nGruberoo Company Overview:");
    Console.WriteLine($"Total Order Amount : {gruberooTotal:F2}");
    Console.WriteLine($"Total Refund Amount: {gruberooRefund:F2}");
    Console.WriteLine($"Final Amount Earned: {gruberooTotal - gruberooRefund:F2}");
}

// ==========================================================
// FILE OUTPUT HELPERS
// ==========================================================
static void AppendOrderToFile(Order order)
{
    try
    {
        using (StreamWriter sw = File.AppendText("orders.csv"))
        {
            sw.WriteLine(order.ToCSVString());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving order: {ex.Message}");
    }
}

static void SaveFiles(List<Restaurant> restaurants, Stack<Order> refundStack)
{
    try
    {
        // Save the current queue state (all restaurants)
        using (StreamWriter sw = new StreamWriter("queue.csv"))
        {
            sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime," +
                         "DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

            foreach (var restaurant in restaurants)
            {
                foreach (var order in restaurant.orders)
                    sw.WriteLine(order.ToCSVString());
            }
        }
        Console.WriteLine("Order queue saved to queue.csv");

        // Save refund stack snapshot
        using (StreamWriter sw = new StreamWriter("stack.csv"))
        {
            sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime," +
                         "DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

            foreach (var order in refundStack.ToList())
                sw.WriteLine(order.ToCSVString());
        }
        Console.WriteLine("Refund stack saved to stack.csv");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving files: {ex.Message}");
    }
}

// ==========================================================
// MAIN PROGRAM
// ==========================================================
Console.WriteLine("Welcome to the Gruberoo Food Delivery System\n");

// Load CSV data
LoadRestaurants(restaurants);
LoadFoodItems(restaurants);
LoadCustomers(customers);
LoadOrders(allOrders, restaurants, customers);

// Loop menu until exit
bool exit = false;
while (!exit)
{
    try
    {
        ShowMenu();
        string choice = Console.ReadLine()?.Trim();

        if (choice == "1") ListRestaurants(restaurants);
        else if (choice == "4") ProcessOrder(restaurants, refundStack);
        else if (choice == "6") DeleteOrder(customers, refundStack);
        else if (choice == "8") ShowTotals(restaurants);
        else if (choice == "0")
        {
            SaveFiles(restaurants, refundStack);
            Console.WriteLine("Thank you for using Gruberoo! Goodbye!");
            exit = true;
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("System Error: " + ex.Message);
    }
}

