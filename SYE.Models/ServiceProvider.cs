using System;
using System.Collections.Generic;
using System.Text;

namespace SYE.Models
{
    /// <summary>
    /// this class represents a single service provider
    /// </summary>
    public class ServiceProvider    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
    }
}
