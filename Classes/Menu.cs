//==========================================================
// Student Number : S10272509G
// Student Name : Javier Ng Zhe Wei
// Partner Name : Axel Tee Yu Le
//==========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10271009C_PRG2Assignment
{

    class Menu
    {
        public string menuId { get; set; }
        public string menuName { get; set; }
        public List<FoodItem> foodItems { get; set; }
        public Menu() { }
        public Menu(string menuId, string menuName)
        {
            this.menuId = menuId;
            this.menuName = menuName;
            this.foodItems = new List<FoodItem>();
        }

        public void AddFoodItem(FoodItem item) 
        {
            foodItems.Add(item);
        }

        public bool RemoveFoodItem(FoodItem item)
        {
            return foodItems.Remove(item);
        }

        public void DisplayFoodItems()
        {
            foreach (FoodItem item in foodItems)
            {
                Console.WriteLine($"  {item.itemName}: {item.itemDesc} - ${item.itemPrice:F2}");
            }
        }

        public override string ToString()
        {
            return menuName;
        }
    }
}
