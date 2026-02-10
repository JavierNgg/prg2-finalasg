using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//==========================================================
// Student Number : S10271009C
// Student Name : Axel Tee Yu Le
// Partner Name : Javier Ng Zhe Wei
//==========================================================

namespace S10271009C_PRG2Assignment
{
    class SpecialOffer
    {
        public string offerCode { get; set; }
        public string offerDesc { get; set; }
        public double discount { get; set; }
        public SpecialOffer() { }
        public SpecialOffer(string offerCode, string offerDesc, double discount) 
        {
            this.offerCode = offerCode;
            this.offerDesc = offerDesc;
            this.discount = discount;
        }
    }
}
