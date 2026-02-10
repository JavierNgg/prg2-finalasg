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
    class OrderedFoodItem : FoodItem
    {
        public int qtyOrdered {  get; set; }
        public double subTotal { get; set; }

        public OrderedFoodItem() { }
        public OrderedFoodItem(FoodItem foodItem, int qtyOrdered) : base(foodItem.itemName, foodItem.itemDesc, foodItem.itemPrice)
        {
            this.qtyOrdered = qtyOrdered;
            this.customise = "";
            this.subTotal = CalculateSubtotal(); ;
        }

        public double CalculateSubtotal()
        {
            this.subTotal = itemPrice * qtyOrdered;
            return subTotal;
        }
    }
}
