using System;

namespace SilentOrbit.Web
{
	public class HttpFetchException : Exception
	{
		public HttpFetchException(string message) : base(message)
		{
		}
	}
}

