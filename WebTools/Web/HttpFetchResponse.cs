using System;
using System.Net;
using System.IO;

namespace SilentOrbit.Web
{
	public class HttpFetchResponse
	{
		public string HttpVersion { get; set; }

		public HttpStatusCode StatusCode { get; set; }

		public string StatusMessage { get; set; }

		public bool KeepAlive { get; set; }

		public bool ChunkedTransferEncoding { get; set; }

		public string Location { get; set; }

		public DateTime? LastModified { get; set; }

		public Exception Exception { get; set; }

		public long ContentLength = -1;

		public MemoryStream Stream { get; set; }

		public HttpFetchResponse()
		{
		}
	}
}

