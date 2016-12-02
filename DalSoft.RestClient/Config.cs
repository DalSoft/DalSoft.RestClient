using System;

namespace DalSoft.RestClient
{
    public class Config
    {
        public Config() { }

        public Config(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan? Timeout { get; private set; }
    }
}
