using System;
namespace Log4Extension
{
    public class CustomerLogFormat
    {
        public CustomerLogFormat()
        {
        }

        public string Message { get; set; }

        public object Data { get; set; }

        public override string ToString()
        {
            return this.Message;
        }
    }
}
