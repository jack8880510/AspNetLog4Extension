using System;
using log4net.Layout;

namespace Example
{
    public class CustomerLogLayout : PatternLayout
    {
        public CustomerLogLayout()
        {
            this.AddConverter("property", typeof(CustomerLogPatternConverter));
        }
    }
}
