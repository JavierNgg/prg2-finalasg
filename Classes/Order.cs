//==========================================================
// Student Number : S10271009C
// Student Name : Axel Tee Yu Le
// Partner Name : Javier Ng Zhe Wei
//==========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10271009C_PRG2Assignment
{
    class Order
    {
        private static int nextOrderId = 1001;
        public int orderId { get; set; }
        public DateTime orderDateTime { get; set; }
        public double orderTotal { get; set; }
        public string orderStatus { get; set; }
        public DateTime deliveryDateTime { get; set; }
        public string deliveryAddress { get; set; }
        public string orderPaymentMethod { get; set; }
        public bool orderPaid { get; set; }
        public List<OrderedFoodItem> orderedFoodItems { get; set; }
        public Customer customer { get; set; }
        public Restaurant restaurant { get; set; }
        public SpecialOffer appliedOffer { get; set; }  // optional
        public string specialRequest { get; set; }
        public Order() { }

        // Constructor for new orders
        public Order(Customer customer, Restaurant restaurant, DateTime deliveryDateTime, string deliveryAddress)
        {
            this.orderId = nextOrderId++;
            this.customer = customer;
            this.restaurant = restaurant;
            this.orderDateTime = DateTime.Now;
            this.deliveryDateTime = deliveryDateTime;
            this.deliveryAddress = deliveryAddress;
            this.orderStatus = "Pending";
            this.orderPaid = false;
            this.orderTotal = 0;
            this.orderedFoodItems = new List<OrderedFoodItem>();
        }
        public Order(Customer customer, Restaurant restaurant, int orderId, DateTime orderDateTime, double orderTotal, string orderStatus, DateTime deliveryDateTime, string deliveryAddress, string orderPaymentMethod, bool orderPaid)
        {
            this.customer = customer;
            this.restaurant = restaurant;
            this.orderId = orderId;
            this.orderDateTime = orderDateTime;
            this.orderTotal = orderTotal;
            this.orderStatus = orderStatus;
            this.deliveryDateTime = deliveryDateTime;
            this.deliveryAddress = deliveryAddress;
            this.orderPaymentMethod = orderPaymentMethod;
            this.orderPaid = orderPaid;
            this.orderedFoodItems = new List<OrderedFoodItem>();

            if (orderId >= nextOrderId)
            {
                nextOrderId = orderId + 1;
            }
        }

        public double CalculateOrderTotal()
        {
            orderTotal = 0;
            foreach (var item in orderedFoodItems)
            {
                orderTotal += item.subTotal;
            }
            orderTotal += 5.00;
            return orderTotal;
        }
        public void AddOrderedFoodItem(OrderedFoodItem item)
        {
            orderedFoodItems.Add(item);
            CalculateOrderTotal();
        }

        public bool RemoveOrderedFoodItem(OrderedFoodItem item)
        {
            if (orderedFoodItems.Remove(item))
            {
                CalculateOrderTotal();
                return true;
            }
            return false;
        }

        public void DisplayOrderedFoodItems()
        {
            foreach (OrderedFoodItem item in orderedFoodItems)
            {
                Console.WriteLine($"  {item.itemName} x {item.qtyOrdered} = ${item.subTotal:F2}");
            }
        }

        public override string ToString()
        {
            return $"Order {orderId} - ${orderTotal:F2} ({orderStatus})";
        }
        public string ToCSVString()
        {
            string itemsString = string.Join("|", orderedFoodItems.Select(oi =>
                $"{oi.itemName}, {oi.qtyOrdered}"));

            return $"{orderId},{customer.emailAddress},{restaurant.restaurantId}," +
                   $"{deliveryDateTime.Date:dd/MM/yyyy},{deliveryDateTime.TimeOfDay:hh\\:mm}," +
                   $"{deliveryAddress},{orderDateTime:dd/MM/yyyy HH:mm}," +
                   $"{orderTotal},{orderStatus},\"{itemsString}\"";
        }
    }
}
