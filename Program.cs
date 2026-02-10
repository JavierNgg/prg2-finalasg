using S10271009C_PRG2Assignment;
using System.Diagnostics;
using System.Xml.Serialization;

List<Restaurant> restaurants = new List<Restaurant>();
List<Customer> customers = new List<Customer>();
List<Order> allOrders = new List<Order>();
Stack<Order> refundStack = new Stack<Order>();

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

            Order order = new Order(c, r, orderId, orderDateTime, orderTotal, orderStatus, deliveryDateTime,
                                    deliveryAddress, "CC", true);

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
                    foodItem = menu.foodItems.FirstOrDefault(x => x.itemName.StartsWith(itemName));
                    if (foodItem != null) break;
                }

                if (foodItem != null)
                {
                    OrderedFoodItem orderedItem = new OrderedFoodItem(foodItem, qty);
                    order.AddOrderedFoodItem(orderedItem);
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

static void ListRestaurants(List<Restaurant> restaurants)
{
    try
    {
        Console.Clear();
        Console.WriteLine("All Restaurants and Menu Items\n");
        foreach (Restaurant r in restaurants)
            r.DisplayMenu();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error listing restaurants: " + ex.Message);
    }
}

static void CreateOrder(List<Customer> customers, List<Restaurant> restaurants, List<Order> allOrders)
{
    Console.Clear();
    Console.WriteLine("Create New Order\n");

    try
    {
        Console.Write("Enter Customer Email: ");
        string email = Console.ReadLine();
        Customer c = customers.FirstOrDefault(x => x.emailAddress == email);
        if (c == null) { Console.WriteLine("Not found!"); return; }

        Console.Write("Enter Restaurant ID: ");
        string rid = Console.ReadLine();
        Restaurant r = restaurants.FirstOrDefault(x => x.restaurantId == rid);
        if (r == null) { Console.WriteLine("Not found!"); return; }

        DateTime date;
        while (true)
        {
            Console.Write("Enter Delivery Date (dd/mm/yyyy): ");
            string dateInput = Console.ReadLine();
            if (DateTime.TryParseExact(dateInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date))
                break;

            Console.WriteLine("Invalid date format. Example: 21/02/2026");
        }

        TimeSpan time;
        while (true)
        {
            Console.Write("Enter Delivery Time (hh:mm): ");
            string timeInput = Console.ReadLine();
            if (TimeSpan.TryParse(timeInput, out time))
                break;

            Console.WriteLine("Invalid time format. Example: 18:30");
        }

        Console.Write("Enter Delivery Address: ");
        string address = Console.ReadLine();

        Order order = new Order(c, r, date.Date + time, address);

        Console.WriteLine("\nAvailable Food Items:");
        List<FoodItem> items = new List<FoodItem>();
        foreach (var menu in r.menus) items.AddRange(menu.foodItems);

        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"{i + 1}. {items[i].itemName} - ${items[i].itemPrice:F2}");

        while (true)
        {
            Console.Write("Item number (0 to finish): ");
            if (!int.TryParse(Console.ReadLine(), out int num))
            {
                Console.WriteLine("Invalid number.");
                continue;
            }

            if (num == 0) break;

            if (num < 1 || num > items.Count)
            {
                Console.WriteLine("Item number out of range.");
                continue;
            }

            Console.Write("Enter quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
            {
                Console.WriteLine("Invalid quantity.");
                continue;
            }

            OrderedFoodItem oi = new OrderedFoodItem(items[num - 1], qty);
            order.AddOrderedFoodItem(oi);
        }

        if (order.orderedFoodItems.Count == 0)
        {
            Console.WriteLine("No items. Cancelled.");
            return;
        }

        Console.Write("Add special request? [Y/N]: ");
        string request = Console.ReadLine();
        if (request == "Y")
        {
            Console.Write("Request: ");
            string specialrequest = Console.ReadLine();
        }

        Console.WriteLine($"\nOrder Total: = ${order.orderTotal - 5:F2} + $5.00 (delivery) = ${order.orderTotal:F2}");

        Console.Write("Proceed to payment? [Y/N]: ");
        string request2 = Console.ReadLine();

        if (request2 != null && request2.ToUpper() == "Y")
        {
            Console.Write("Payment method [CC] Credit Card / [PP] PayPal / [CD] Cash on Delivery: ");

            string payMethod = Console.ReadLine();

            if (!string.IsNullOrEmpty(payMethod))
            {
                order.orderPaymentMethod = payMethod.ToUpper();
                order.orderPaid = true;
            }
            else
            {
                Console.WriteLine("Payment method not entered. Payment cancelled.");
            }
        }
        else
        {
            Console.WriteLine("Payment skipped. Order remains unpaid.");
        }


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

static void ModifyOrder(List<Customer> customers)
{
    Console.WriteLine("\nModify Order\r\n=============");
    Console.Write("Enter Customer Email: ");
    string email = Console.ReadLine();
    Customer c = customers.FirstOrDefault(x => x.emailAddress == email);
    Console.WriteLine("Pending Orders:");

    foreach (Order order in c.orders)
    {
        if (order.orderStatus == "Pending")
        {
            Console.WriteLine(order.orderId);
        }
    }

    Console.WriteLine("Enter Order ID: ");
    int orderId = Convert.ToInt32(Console.ReadLine());

    Order selectorder = c.orders.FirstOrDefault(x => x.orderId == orderId);
    Console.WriteLine("Order Items:");
    int count = 0;
    foreach (OrderedFoodItem item in selectorder.orderedFoodItems)
    {
        count++;
        Console.WriteLine($"{count}. {item.itemName} - {item.qtyOrdered}");
    }

    Console.WriteLine("Address:");
    Console.WriteLine(selectorder.deliveryAddress);
    Console.WriteLine("Delivery Date/Time:");
    Console.WriteLine(selectorder.deliveryDateTime.ToShortTimeString());
    
    while (true)
    {
        Console.Write("\nModify: [1] Items [2] Address [3] Delivery Time: ");
        string modchoice = Console.ReadLine();
        try
        {
            if (modchoice == "1")
            {
                while (true)
                {
                    Console.Write("Select item number to edit quantity: ");
                    int idx = Convert.ToInt32(Console.ReadLine());
                    if (idx < 1 || idx > selectorder.orderedFoodItems.Count)
                    {
                        Console.WriteLine("Invalid item number.");
                    }
                    else
                    {
                        double oldTotal = selectorder.CalculateOrderTotal();

                        OrderedFoodItem selectedItem = selectorder.orderedFoodItems[idx - 1];
                        Console.Write($"Enter new quantity for {selectedItem.itemName} (current {selectedItem.qtyOrdered}): ");
                        int newQty = Convert.ToInt32(Console.ReadLine());

                        if (newQty < 1)
                        {
                            selectorder.RemoveOrderedFoodItem(selectedItem);
                            Console.WriteLine($"{selectedItem.itemName} removed from the order.");
                        }
                        else
                        {
                            selectedItem.qtyOrdered = newQty;
                            selectedItem.CalculateSubtotal();
                            selectorder.orderTotal = selectorder.CalculateOrderTotal();
                            Console.WriteLine($"{selectedItem.itemName} quantity updated to {newQty}.");
                        }

                        Console.WriteLine($"\nUpdated Total: {selectorder.CalculateOrderTotal():F2}");

                        if (selectorder.CalculateOrderTotal() > oldTotal)
                        {
                            Console.Write("Outstanding amount. Proceed to payment? [Y/N]: ");
                            string request = Console.ReadLine();
                            if (request != null && request.ToUpper() == "Y")
                            {
                                Console.Write("Payment method [CC] Credit Card / [PP] PayPal / [CD] Cash on Delivery: ");

                                string payMethod = Console.ReadLine();

                                if (!string.IsNullOrEmpty(payMethod))
                                {
                                    selectorder.orderPaymentMethod = payMethod.ToUpper();
                                    selectorder.orderPaid = true;
                                }
                                else
                                {
                                    Console.WriteLine("Payment method not entered. Payment cancelled.");
                                    selectorder.orderPaid = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Payment skipped. Order remains unpaid.");
                                selectorder.orderPaid = false;
                            }
                        }
                        break;
                    }
                }
                break;
            }
            else if (modchoice == "2")
            {
                Console.Write("Enter new Delivery Address: ");
                string newAddress = Console.ReadLine();
                selectorder.deliveryAddress = newAddress;
                break;
            }
            else if (modchoice == "3")
            {
                Console.Write("Enter new Delivery Time (hh:mm): ");
                string newTime = Console.ReadLine();
                selectorder.deliveryDateTime = Convert.ToDateTime(selectorder.deliveryDateTime.Date.ToString() + newTime);
                break;
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

    Console.WriteLine("\nUpdated Order Details:");
    Console.WriteLine("Order Items:");
    int count2 = 0;
    foreach (OrderedFoodItem item in selectorder.orderedFoodItems)
    {
        count2++;
        Console.WriteLine($"{count2}. {item.itemName} - {item.qtyOrdered}");
    }

    Console.WriteLine("Address:");
    Console.WriteLine(selectorder.deliveryAddress);
    Console.WriteLine("Delivery Date/Time:");
    Console.WriteLine(selectorder.deliveryDateTime.ToShortTimeString());
}

static void DeleteOrder(List<Customer> customers, Stack<Order> refundStack)
{
    Console.WriteLine("\nModify Order\r\n=============");
    Console.Write("Enter Customer Email: ");
    string email = Console.ReadLine();
    Customer c = customers.FirstOrDefault(x => x.emailAddress == email);

    Console.WriteLine("Pending Orders:");

    foreach (Order order in c.orders)
    {
        if (order.orderStatus == "Pending")
        {
            Console.WriteLine(order.orderId);
        }
    }

    Console.WriteLine("Enter Order ID: ");
    int orderId = Convert.ToInt32(Console.ReadLine());

    Order selectorder = c.orders.FirstOrDefault(x => x.orderId == orderId);

    Console.WriteLine($"Customer: {selectorder.customer}");
    Console.WriteLine("Ordered Items:");

    int count = 0;
    foreach (OrderedFoodItem item in selectorder.orderedFoodItems)
    {
        count++;
        Console.WriteLine($"{count}. {item.itemName} - {item.qtyOrdered}");
    }
    Console.WriteLine($"Delivery date/time: {selectorder.deliveryDateTime:f}");
    Console.WriteLine($"Total Amount: ${selectorder.orderTotal:F2}");
    Console.WriteLine($"Order Status: {selectorder.orderStatus}");
    Console.Write("Confirm deletion? [Y/N]: ");
    string choice = Console.ReadLine();
    if (choice.ToUpper() == "Y")
    {
        refundStack.Push(selectorder);
        selectorder.orderStatus = "Cancelled";
        Console.WriteLine($"Order {selectorder.orderId} cancelled. Refund of ${selectorder.CalculateOrderTotal()} processed.");
    }
}
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
        using (StreamWriter sw = new StreamWriter("queue.csv"))
        {
            sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime," +
                       "DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

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
                double orderTotal = order.CalculateOrderTotal();
                restaurantTotal += orderTotal;
                gruberooTotal += orderTotal;
            }

            if (order.orderStatus == "Rejected" || order.orderStatus == "Cancelled")
            {
                double orderTotal = order.CalculateOrderTotal();
                restaurantRefund += orderTotal;
                gruberooRefund += orderTotal;
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

Console.WriteLine("Welcome to the Gruberoo Food Delivery System\n");

LoadRestaurants(restaurants);
LoadFoodItems(restaurants);
LoadCustomers(customers);
LoadOrders(allOrders, restaurants, customers);

bool exit = false;
while (!exit)
{
    try
    {
        ShowMenu();
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            ListRestaurants();
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
            ProcessOrder();
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
            BulkProcess();
        }
        else if (choice == "8")
        {
            ShowTotals(restaurants);
        }
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

