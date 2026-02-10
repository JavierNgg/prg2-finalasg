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
    class Restaurant
    {
        public string restaurantId { get; set; }
        public string restaurantName { get; set; }
        public string restaurantEmail { get; set; }
        public List<Menu> menus { get; set; }
        public Queue<Order> orders { get; set; }
        public List<SpecialOffer> specialOffers { get; set; }
        public Restaurant() { }
        public Restaurant(string restaurantId, string restaurantName, string restaurantEmail)
        {
            this.restaurantId = restaurantId;
            this.restaurantName = restaurantName;
            this.restaurantEmail = restaurantEmail;
            this.menus = new List<Menu>();
            this.orders = new Queue<Order>();
            this.specialOffers = new List<SpecialOffer>();
        }

        public void DisplayOrders()
        {
            foreach (Order order in orders)
            {
                Console.WriteLine($"  Order {order.orderId}: ${order.orderTotal:F2} - {order.orderStatus}");
            }
        }

        public void DisplaySpecialOffers()
        {
            foreach (SpecialOffer offer in specialOffers)
            {
                Console.WriteLine($"  {offer}");
            }
        }

        public void DisplayMenu()
        {
            Console.WriteLine($"\n{restaurantName} ({restaurantId})");
            foreach (Menu menu in menus)
            {
                menu.DisplayFoodItems();
            }
        }

        public void AddMenu(Menu menu)
        {
            menus.Add(menu);
        }

        public bool RemoveMenu(Menu menu)
        {
            return menus.Remove(menu);
        }

        public override string ToString()
        {
            return restaurantName;
        }
    }
}
