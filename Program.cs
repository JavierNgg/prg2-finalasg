using S10271009C_PRG2Assignment;
using System.Diagnostics;
using System.Xml.Serialization;

// Lists to store main program data in memory
List<Restaurant> restaurants = new List<Restaurant>();
List<Customer> customers = new List<Customer>();
List<Order> allOrders = new List<Order>();
Stack<Order> refundStack = new Stack<Order>();

// Reads a non-empty string from user and keeps asking until something is entered
static string ReadNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string s = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(s))
            return s.Trim();

        Console.WriteLine("Input cannot be empty. Please try again.");
    }
}

// Reads a choice and checks if it matches one of the allowed values
static string ReadChoice(string prompt, params string[] allowed)
{
    while (true)
    {
        Console.Write(prompt);
        string input = (Console.ReadLine() ?? "").Trim().ToUpper();

        // Loops through allowed choices to see if input matches
        for (int i = 0; i < allowed.Length; i++)
        {
            if (input == allowed[i].Trim().ToUpper())
                return input;
        }

        Console.WriteLine("Invalid choice. Allowed: " + string.Join("/", allowed));
    }
}

// Reads an integer and keeps asking until a valid number is entered
static int ReadInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string s = (Console.ReadLine() ?? "").Trim();
        if (int.TryParse(s, out int value)) return value;

        Console.WriteLine("Invalid number. Please enter an integer.");
    }
}

// Reads an integer within a given range
static int ReadIntInRange(string prompt, int min, int max)
{
    while (true)
    {
        int v = ReadInt(prompt);
        if (v >= min && v <= max) return v;
        Console.WriteLine($"Out of range. Enter a number from {min} to {max}.");
    }
}

// Reads a date and converts it to DateTime
static DateTime ReadDate(string prompt)
{
    string format = "dd/MM/yyyy";
    while (true)
    {
        Console.Write(prompt);
        string s = (Console.ReadLine() ?? "").Trim();
        if (DateTime.TryParse(s, out DateTime dt))
            return dt.Date;

        Console.WriteLine($"Invalid date. Format must be {format}. Example: 15/02/2026");
    }
}

// Reads time in HH:mm format
static TimeSpan ReadTime(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string s = (Console.ReadLine() ?? "").Trim();

        // TryParseExact ensures correct HH:mm format
        if (TimeSpan.TryParseExact(s, "hh\\:mm", null, out TimeSpan t))
            return t;

        Console.WriteLine("Invalid time. Format must be HH:mm. Example: 18:30");
    }
}

// Finds restaurants by their restaurant id
static Restaurant ReadRestaurantById(List<Restaurant> restaurants)
{
    while (true)
    {
        string rid = ReadNonEmpty("Enter Restaurant ID: ").ToUpper();

        // Searches list of restaurants to find matching ID
        Restaurant r = restaurants.FirstOrDefault(x => x.restaurantId.Equals(rid, StringComparison.OrdinalIgnoreCase));
        if (r != null) return r;

        Console.WriteLine("Restaurant not found. Please try again.");
    }
}

// Finds customers by email address
static Customer ReadCustomerByEmail(List<Customer> customers)
{
    while (true)
    {
        string email = ReadNonEmpty("Enter Customer Email: ");

        // Searches list of customers to find matching email
        Customer c = customers.FirstOrDefault(x => x.emailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (c != null) return c;

        Console.WriteLine("Customer not found. Please try again.");
    }
}

// Displays the main menu
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

// Loads restaurants from CSV file into objects
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

            // Creates restaurant object from CSV row
            Restaurant r = new Restaurant(parts[0], parts[1], parts[2]);
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

// Loads food items and assigns them to the correct restaurant menus
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

            string restId = parts[0];
            string itemName = parts[1];
            string itemDesc = parts[2];

            if (!double.TryParse(parts[3], out double itemPrice))
                continue;

            // Finds restaurant that owns this food item
            Restaurant r = restaurants.FirstOrDefault(x => x.restaurantId == restId);
            if (r != null)
            {
                // Ensures restaurant has at least one menu
                if (r.menus.Count == 0)
                {
                    Menu menu = new Menu(restId + "M", r.restaurantName + " Menu");
                    r.AddMenu(menu);
                }

                FoodItem item = new FoodItem(itemName, itemDesc, itemPrice);
                r.menus[0].AddFoodItem(item);
                count++;
            }
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

// Loads customers from CSV file
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

            // Creates customer object from CSV row
            Customer c = new Customer(parts[0], parts[1]);
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

// Loads orders from CSV file and links them to customers and restaurants
static void LoadOrders(List<Order> allOrders, List<Restaurant> restaurants, List<Customer> customers)
{
    try
    {
        string[] lines = File.ReadAllLines("orders.csv");
        int count = 0;

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 10) continue;

            if (!int.TryParse(parts[0], out int orderId)) continue;

            // Finds matching customer and restaurant
            Customer c = customers.FirstOrDefault(x => x.emailAddress == parts[1]);
            Restaurant r = restaurants.FirstOrDefault(y => y.restaurantId == parts[2].Trim());
            if (c == null || r == null) continue;

            DateTime deliveryDateTime;
            if (!DateTime.TryParse(parts[3] + " " + parts[4], out deliveryDateTime)) continue;

            string deliveryAddress = parts[5];

            DateTime orderDateTime;
            if (!DateTime.TryParse(parts[6], out orderDateTime)) continue;

            if (!double.TryParse(parts[7], out double orderTotal)) continue;

            string orderStatus = parts[8];

            // Creates order object from CSV row
            Order order = new Order(c, r, orderId, orderDateTime, orderTotal, orderStatus, deliveryDateTime,
                                    deliveryAddress, "CC", true);

            // Parses ordered items inside quotes
            string[] quoteSplit = line.Split('"');
            string itemsString = quoteSplit[1];

            string[] items = itemsString.Split('|');
            foreach (string item in items)
            {
                string[] itemParts = item.Split(',');

                if (itemParts.Length < 2) continue;

                string itemName = itemParts[0].Trim().Trim('"');
                if (!int.TryParse(itemParts[1], out int qty)) continue;

                FoodItem foodItem = null;
                foreach (Menu menu in r.menus)
                {
                    // Finds food item in restaurant menu
                    foodItem = menu.foodItems.FirstOrDefault(x => x.itemName.StartsWith(itemName));
                    if (foodItem != null) break;
                }

                if (foodItem != null)
                {
                    OrderedFoodItem orderedItem = new OrderedFoodItem(foodItem, qty);
                    order.AddOrderedFoodItem(orderedItem);
                }
            }

            // Links order to system lists
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
// Displays all restaurants and their menu items
static void ListRestaurants(List<Restaurant> restaurants)
{
    try
    {
        Console.Clear();
        Console.WriteLine("All Restaurants and Menu Items\n");

        // Loops through each restaurant and prints its menu
        foreach (Restaurant r in restaurants)
            r.DisplayMenu();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error listing restaurants: " + ex.Message);
    }
}

// Displays all orders in a formatted table
static void ListOrders(List<Order> allOrders)
{
    try
    {
        Console.Clear();
        Console.WriteLine("All Orders\n");

        Console.WriteLine($"{"ID",-8} {"Customer",-15} {"Restaurant",-15} {"Delivery",-18} {"Amount",-10} {"Status",-10}");
        Console.WriteLine(new string('-', 80));

        // Prints basic order details
        foreach (Order order in allOrders)
        {
            Console.WriteLine($"{order.orderId,-8} {order.customer.customerName,-15} {order.restaurant.restaurantName,-15} " +
                              $"{order.deliveryDateTime:dd/MM/yyyy HH:mm}  ${order.orderTotal,-9:F2} {order.orderStatus,-10}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error listing orders: " + ex.Message);
    }
}

// Handles creating a new order
static void CreateOrder(List<Customer> customers, List<Restaurant> restaurants, List<Order> allOrders)
{
    Console.Clear();
    Console.WriteLine("Create New Order\n");

    try
    {
        // Gets valid customer and restaurant
        Customer c = ReadCustomerByEmail(customers);
        Restaurant r = ReadRestaurantById(restaurants);

        DateTime date = ReadDate("Enter Delivery Date (dd/MM/yyyy): ");
        TimeSpan time = ReadTime("Enter Delivery Time (HH:mm): ");
        DateTime deliveryDT = date + time;

        string address = ReadNonEmpty("Enter Delivery Address: ");

        Order order = new Order(c, r, date.Date + time, address);

        Console.WriteLine("\nAvailable Food Items:");

        // Collects all food items from menus
        List<FoodItem> items = new List<FoodItem>();
        foreach (var menu in r.menus) items.AddRange(menu.foodItems);

        // Displays items
        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"{i + 1}. {items[i].itemName} - ${items[i].itemPrice:F2}");

        // Allows selecting multiple items
        while (true)
        {
            int itemNo = ReadIntInRange("Enter item number (0 to finish): ", 0, items.Count);
            if (itemNo == 0) break;

            int qty = ReadIntInRange("Enter quantity: ", 1, 999);

            OrderedFoodItem oi = new OrderedFoodItem(items[itemNo - 1], qty);
            order.AddOrderedFoodItem(oi);
        }

        // Prevent empty order
        if (order.orderedFoodItems.Count == 0)
        {
            Console.WriteLine("No items. Cancelled.");
            return;
        }

        // Special request
        string srChoice = ReadChoice("Add special request? [Y/N]: ", "Y", "N");

        if (srChoice == "Y")
        {
            string sr = ReadNonEmpty("Enter special request: ");
            order.specialRequest = sr;
        }

        // Calculates total
        order.CalculateOrderTotal();
        Console.WriteLine($"\nOrder Total: = ${order.orderTotal - 5:F2} + $5.00 (delivery) = ${order.orderTotal:F2}");

        // Payment step
        string payNow = ReadChoice("Proceed to payment? [Y/N]: ", "Y", "N");

        if (payNow == "N")
        {
            Console.WriteLine("Order creation cancelled (payment required).");
            return;
        }

        string method = ReadChoice("Payment method [CC/PP/CD]: ", "CC", "PP", "CD");
        order.orderPaymentMethod = method;
        order.orderPaid = true;

        // Adds order to system
        allOrders.Add(order);
        c.AddOrder(order);
        r.orders.Enqueue(order);

        AppendOrderToFile(order);

        Console.WriteLine($"\nOrder {order.orderId} created successfully! Status: Pending");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating order: " + ex.Message);
    }
}

// Processes orders in a restaurant queue
static void ProcessOrder(List<Restaurant> restaurants, Stack<Order> refundStack)
{
    Console.WriteLine("\nProcess Order\r\n=============");

    Restaurant r = ReadRestaurantById(restaurants);

    if (r.orders.Count == 0)
    {
        Console.WriteLine("No orders in this restaurant's queue.");
        return;
    }

    // Loops through orders in queue
    foreach (Order order in r.orders)
    {
        Console.WriteLine($"\nOrder {order.orderId}:");
        Console.WriteLine($"Customer: {order.customer}");
        Console.WriteLine("Ordered Items:");

        int count = 0;
        foreach (OrderedFoodItem item in order.orderedFoodItems)
        {
            count++;
            Console.WriteLine($"{count}. {item.itemName} - {item.qtyOrdered}");
        }

        Console.WriteLine($"Delivery date/time: {order.deliveryDateTime:f}");
        Console.WriteLine($"Total Amount: ${order.orderTotal:F2}");
        Console.WriteLine($"Order Status: {order.orderStatus}");

        // Lets user decide what to do with order
        while (true)
        {
            string action = ReadChoice("\n[C]onfirm / [R]eject / [S]kip / [D]eliver: ", "C", "R", "S", "D");
            try
            {
                if (action == "C")
                {
                    if (order.orderStatus == "Pending")
                    {
                        order.orderStatus = "Preparing";
                        Console.WriteLine($"Order {order.orderId} confirmed. Status: {order.orderStatus}");
                        break;
                    }
                    else Console.WriteLine("Order is not pending. Failed to confirm.");
                }
                else if (action == "R")
                {
                    if (order.orderStatus == "Pending")
                    {
                        order.orderStatus = "Rejected";
                        refundStack.Push(order);
                        Console.WriteLine($"Order {order.orderId} added to refund queue. Status: {order.orderStatus}");
                        break;
                    }
                    else Console.WriteLine("Order is not pending. Failed to reject.");
                }
                else if (action == "S")
                {
                    if (order.orderStatus == "Cancelled" || order.orderStatus == "Delivered" || order.orderStatus == "Rejected")
                    {
                        Console.WriteLine($"Order {order.orderId} skipped. Status: {order.orderStatus}");
                        break;
                    }
                    else Console.WriteLine("Order is not cancelled/delivered. Failed to skip.");
                }
                else if (action == "D")
                {
                    if (order.orderStatus == "Preparing")
                    {
                        order.orderStatus = "Delivered";
                        Console.WriteLine($"Order {order.orderId} delivered. Status: {order.orderStatus}");
                        break;
                    }
                    else Console.WriteLine("Order is not preparing. Failed to deliver.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("System Error: " + ex.Message);
            }
        }
    }
}

// Modifies an existing order
static void ModifyOrder(List<Customer> customers)
{
    Console.WriteLine("\nModify Order\r\n=============");

    Customer c = ReadCustomerByEmail(customers);

    Console.WriteLine("Pending Orders:");

    int pendingCount = 0;
    foreach (Order order in c.orders)
    {
        if (order.orderStatus == "Pending")
        {
            pendingCount++;
            Console.WriteLine(order.orderId);
        }
    }

    if (pendingCount == 0)
    {
        Console.WriteLine("\nNo Pending orders found for this customer.");
        return;
    }

    // Select order to modify
    Order selectedorder;
    while (true)
    {
        int orderId = ReadInt("Enter Order ID: ");
        selectedorder = c.orders.FirstOrDefault(x => x.orderId == orderId);
        if (selectedorder != null && selectedorder.orderStatus == "Pending") break;
        Console.WriteLine("Invalid Order ID (must be one of the pending orders). Please try again.");
    }

    Console.WriteLine("Order Items:");
    int count = 0;
    foreach (OrderedFoodItem item in selectedorder.orderedFoodItems)
    {
        count++;
        Console.WriteLine($"{count}. {item.itemName} - {item.qtyOrdered}");
    }

    Console.WriteLine("Address:");
    Console.WriteLine(selectedorder.deliveryAddress);
    Console.WriteLine("Delivery Date/Time:");
    Console.WriteLine(selectedorder.deliveryDateTime.ToShortTimeString());

    string modchoice = ReadChoice("\nModify: [1] Items [2] Address [3] Delivery Time: ", "1", "2", "3");

    try
    {
        if (modchoice == "1")
        {
            int idx = ReadIntInRange("Select item number to edit quantity: ", 1, selectedorder.orderedFoodItems.Count);

            double oldTotal = selectedorder.CalculateOrderTotal();
            OrderedFoodItem selectedItem = selectedorder.orderedFoodItems[idx - 1];

            int newQty = ReadIntInRange($"Enter new quantity for {selectedItem.itemName} (current {selectedItem.qtyOrdered}) (0 to remove): ", 0, 999);

            if (newQty < 1)
            {
                selectedorder.RemoveOrderedFoodItem(selectedItem);
                Console.WriteLine($"{selectedItem.itemName} removed from the order.");
            }
            else
            {
                selectedItem.qtyOrdered = newQty;
                selectedItem.CalculateSubtotal();
                selectedorder.orderTotal = selectedorder.CalculateOrderTotal();
                Console.WriteLine($"{selectedItem.itemName} quantity updated to {newQty}.");
            }

            Console.WriteLine($"\nUpdated Total: {selectedorder.CalculateOrderTotal():F2}");

            if (selectedorder.CalculateOrderTotal() > oldTotal)
            {
                string pay = ReadChoice("Outstanding amount. Proceed to payment? [Y/N]: ", "Y", "N");
                if (pay == "Y")
                {
                    string method = ReadChoice("Payment method [CC/PP/CD]: ", "CC", "PP", "CD");
                    selectedorder.orderPaymentMethod = method.ToUpper();
                    selectedorder.orderPaid = true;
                }
                else
                {
                    Console.WriteLine("Payment skipped. Order marked unpaid.");
                    selectedorder.orderPaid = false;
                }
            }
        }
        else if (modchoice == "2")
        {
            string newAddress = ReadNonEmpty("Enter new Delivery Address: ");
            selectedorder.deliveryAddress = newAddress;
        }
        else if (modchoice == "3")
        {
            TimeSpan newTime = ReadTime("Enter new Delivery Time (HH:mm): ");
            selectedorder.deliveryDateTime = selectedorder.deliveryDateTime.Date + newTime;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("System Error: " + ex.Message);
    }

    Console.WriteLine("\nUpdated Order Details:");
    selectedorder.DisplayOrderedFoodItems();
    Console.WriteLine(selectedorder.deliveryAddress);
    Console.WriteLine(selectedorder.deliveryDateTime.ToShortTimeString());
}

// Cancels an order and pushes it to refund stack
static void DeleteOrder(List<Customer> customers, Stack<Order> refundStack)
{
    Console.WriteLine("\nModify Order\r\n=============");

    Customer c = ReadCustomerByEmail(customers);

    Console.WriteLine("Pending Orders:");

    int pendingCount = 0;
    foreach (Order order in c.orders)
    {
        if (order.orderStatus == "Pending")
        {
            pendingCount++;
            Console.WriteLine(order.orderId);
        }
    }

    // Stops if no pending orders
    if (pendingCount == 0)
    {
        Console.WriteLine("\nNo Pending orders found for this customer.");
        return;
    }

    // Select order to cancel
    Order selectedorder;
    while (true)
    {
        int orderId = ReadInt("Enter Order ID: ");
        selectedorder = c.orders.FirstOrDefault(o => o.orderId == orderId);
        if (selectedorder != null) break;
        Console.WriteLine("Invalid Order ID (must be one of the pending orders). Please try again.");
    }

    // Shows order details before confirmation
    Console.WriteLine($"Customer: {selectedorder.customer}");
    Console.WriteLine("Ordered Items:");
    selectedorder.DisplayOrderedFoodItems();
    Console.WriteLine($"Delivery date/time: {selectedorder.deliveryDateTime:f}");
    Console.WriteLine($"Total Amount: ${selectedorder.orderTotal:F2}");
    Console.WriteLine($"Order Status: {selectedorder.orderStatus}");

    // Confirm cancellation
    string confirm = ReadChoice("Confirm deletion? [Y/N]: ", "Y", "N");
    if (confirm == "Y")
    {
        refundStack.Push(selectedorder);
        selectedorder.orderStatus = "Cancelled";
        Console.WriteLine($"Order {selectedorder.orderId} cancelled. Refund of ${selectedorder.CalculateOrderTotal():F2} processed.");
    }
    else
    {
        Console.WriteLine("Cancellation aborted.");
    }
}

// Appends a new order to orders.csv
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

// Saves queue and stack to CSV files before exit
static void SaveFiles(List<Restaurant> restaurants, Stack<Order> refundStack)
{
    try
    {
        using (StreamWriter sw = new StreamWriter("queue.csv"))
        {
            sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime," +
                       "DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

            // Saves all orders currently in restaurant queues
            foreach (var restaurant in restaurants)
            {
                foreach (var order in restaurant.orders)
                {
                    sw.WriteLine(order.ToCSVString());
                }
            }
        }

        Console.WriteLine("Order queue saved to queue.csv");

        try
        {
            using (StreamWriter sw = new StreamWriter("stack.csv"))
            {
                sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime," +
                            "DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

                // Saves refund stack
                var stackList = refundStack.ToList();
                foreach (var order in stackList)
                {
                    sw.WriteLine(order.ToCSVString());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving refund stack: {ex.Message}");
        }

        Console.WriteLine("Refund stack saved to stack.csv");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving files: {ex.Message}");
    }
}

// Calculates totals for restaurants and the company
static void ShowTotals(List<Restaurant> restaurants)
{
    double gruberooTotal = 0;
    double gruberooRefund = 0;

    foreach (Restaurant restaurant in restaurants)
    {
        double restaurantTotal = 0;
        double restaurantRefund = 0;

        // Loops through each order in restaurant queue
        foreach (Order order in restaurant.orders)
        {
            if (order.orderStatus == "Delivered")
            {
                double net = order.orderTotal - 5.0; // subtract delivery fee
                restaurantTotal += net;
                gruberooTotal += net;
            }

            if (order.orderStatus == "Rejected" || order.orderStatus == "Cancelled")
            {
                double net = order.orderTotal - 5.0;
                restaurantTotal += net;
                gruberooTotal += net;
            }
        }

        Console.WriteLine($"\n=== {restaurant} ===");
        Console.WriteLine("Restaurant Overview: ");
        Console.WriteLine($"Total Order Amount: {restaurantTotal:F2}");
        Console.WriteLine($"Total Refund Amount: {restaurantRefund:F2}");
    }

    Console.WriteLine("\nGruberoo Company Overview: ");
    Console.WriteLine($"Total Order Amount: {gruberooTotal:F2}");
    Console.WriteLine($"Total Refund Amount: {gruberooRefund:F2}");
    Console.WriteLine($"Final Amount Earned: {gruberooTotal - gruberooRefund:F2}");
}

// Bulk processes all pending orders
static void BulkProcess(List<Restaurant> restaurants, Stack<Order> refundStack, List<Order> allOrders)
{
    Console.WriteLine("Bulk Process Unprocessed Orders");
    Console.WriteLine("================================");

    int totalPending = 0;
    int preparing = 0;
    int rejected = 0;

    // Counts pending orders first
    foreach (Restaurant restaurant in restaurants)
    {
        foreach (Order order in restaurant.orders)
        {
            if (order.orderStatus == "Pending")
            {
                totalPending++;
            }
        }
    }

    Console.WriteLine($"Total pending orders: {totalPending}");

    if (totalPending == 0)
    {
        Console.WriteLine("No pending orders to process.");
        return;
    }

    // Processes each order in each restaurant queue
    foreach (Restaurant restaurant in restaurants)
    {
        int queueSize = restaurant.orders.Count;

        for (int i = 0; i < queueSize; i++)
        {
            Order order = restaurant.orders.Dequeue();

            if (order.orderStatus == "Pending")
            {
                DateTime deliveryDateTime = order.deliveryDateTime.Date + order.deliveryDateTime.TimeOfDay;
                TimeSpan timeUntilDelivery = deliveryDateTime - DateTime.Now;

                // Reject if less than 1 hour to delivery
                if (timeUntilDelivery.TotalHours < 1)
                {
                    order.orderStatus = "Rejected";
                    refundStack.Push(order);
                    rejected++;
                    Console.WriteLine($"Order {order.orderId} rejected (delivery time < 1 hour)");
                }
                else
                {
                    order.orderStatus = "Preparing";
                    preparing++;
                    Console.WriteLine($"Order {order.orderId} set to Preparing");
                }
            }

            // Reinsert order back into queue
            restaurant.orders.Enqueue(order);
        }
    }

    int processed = preparing + rejected;
    double percentage = allOrders.Count > 0 ? (processed * 100.0 / allOrders.Count) : 0;

    Console.WriteLine("\n=== Summary ===");
    Console.WriteLine($"Orders processed: {processed}");
    Console.WriteLine($"Preparing: {preparing}");
    Console.WriteLine($"Rejected: {rejected}");
    Console.WriteLine($"Percentage of all orders: {percentage:F2}%");
}

// Program entry point
Console.WriteLine("Welcome to the Gruberoo Food Delivery System\n");

// Loads all CSV data at startup
LoadRestaurants(restaurants);
LoadFoodItems(restaurants);
LoadCustomers(customers);
LoadOrders(allOrders, restaurants, customers);

bool exit = false;

// Main menu loop
while (!exit)
{
    try
    {
        ShowMenu();
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            ListRestaurants(restaurants);
        }
        else if (choice == "2")
        {
            ListOrders(allOrders);
        }
        else if (choice == "3")
        {
            CreateOrder(customers, restaurants, allOrders);
        }
        else if (choice == "4")
        {
            ProcessOrder(restaurants, refundStack);
        }
        else if (choice == "5")
        {
            ModifyOrder(customers);
        }
        else if (choice == "6")
        {
            DeleteOrder(customers, refundStack);
        }
        else if (choice == "7")
        {
            BulkProcess(restaurants, refundStack, allOrders);
        }
        else if (choice == "8")
        {
            ShowTotals(restaurants);
        }
        else if (choice == "0")
        {
            // Saves queue and stack before exit
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
