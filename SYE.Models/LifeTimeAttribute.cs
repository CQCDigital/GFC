using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SYE.Models.Enums;

namespace SYE.Models
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class LifeTimeAttribute : Attribute
    {
        public LifeTime name;

        public LifeTimeAttribute(LifeTime lifeTime)
        {
            this.name = lifeTime;           
        }
    }
}
