using System;
using System.Collections.Generic;
using System.Text;

namespace AI_ColdCall_Agent.Core.DTO
{
    public class LeadRequestDto
    {
        public int RequestId { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }

        public string SellerName { get; set; }
        public string SellerPhone { get; set; }

       
        public int PropertyId { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public string Location { get; set; }
        public string PropertyType { get; set; }

        public string StatusName { get; set; }
    }
}
