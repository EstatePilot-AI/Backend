using System;
using System.Collections.Generic;
using System.Text;

namespace AI_ColdCall_Agent.Core.DTO
{
    public class LeadRequestResponseDto
    {
        public int RequestId { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }

       
        public string StatusName { get; set; }
    }
}
