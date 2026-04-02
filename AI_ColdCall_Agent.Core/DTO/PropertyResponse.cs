using System;
using System.Collections.Generic;
using System.Text;

namespace AI_ColdCall_Agent.Core.DTO
{
    public class PropertyResponse
    {
        public int PropertyId { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
      
        public string PropertyType { get; set; } 
        public string Status { get; set; }     
        public string City { get; set; }        
        public string District { get; set; }    
        public List<string> ImageURLs { get; set; }
	}
}
