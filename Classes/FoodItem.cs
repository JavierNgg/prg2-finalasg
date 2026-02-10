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
    class FoodItem
    {
        public string itemName { get; set; }
        public string itemDesc { get; set; }
        public double itemPrice { get; set; }
        public string customise { get; set; }

        public FoodItem() { }
        public FoodItem(string itemName, string itemDesc, double itemPrice)
        {
            this.itemName = itemName;
            this.itemDesc = itemDesc;
            this.itemPrice = itemPrice;
            this.customise = "";
        }
        public override string ToString()
        {
            return $"{itemName} - ${itemPrice:F2}";
        }
    }
}
