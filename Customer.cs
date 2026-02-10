//==========================================================
// Student Number : S10271009C
// Student Name : Axel Tee Yu Le
// Partner Name : Javier Ng Zhe Wei
//==========================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10271009C_PRG2Assignment
{
    class Customer
    {
        public string emailAddress {  get; set; }
        public string customerName { get; set; }
        public List<Order> orders { get; set; }

        public Customer(string customerName, string emailAddress) {
            this.emailAddress = emailAddress;
            this.customerName = customerName;
            this.orders = new List<Order>();
        }

        public void AddOrder(Order order) 
        {
            orders.Add(order);
        }
        public void DisplayAllOrders() 
        {
            foreach (Order order in orders)
            {
                Console.WriteLine($"  Order {order.orderId}: ${order.orderTotal:F2} - {order.orderStatus}");
            }
        }
        public bool RemoveOrder(Order order) 
        {
            return orders.Remove(order);
        }

        public override string ToString()
        {
            return customerName;
        }
    }
}
