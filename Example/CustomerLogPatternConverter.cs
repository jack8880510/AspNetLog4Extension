using System;
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace Example
{
    public class CustomerLogPatternConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
        }
    }
}
