using System;
using System.Net;

namespace OurIPH.Services
{
    public sealed class TimeoutWebClient : WebClient
    {
        private readonly int _timeoutMilliseconds;

        public TimeoutWebClient(int timeoutMilliseconds)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = _timeoutMilliseconds;
            }

            return request;
        }
    }
}
