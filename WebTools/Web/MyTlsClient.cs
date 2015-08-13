using System;
using Org.BouncyCastle.Crypto.Tls;

namespace SilentOrbit.Web
{
	public class MyTlsClient : DefaultTlsClient
	{
		readonly string host;

		public MyTlsClient(string host)
		{
			this.host = host;
		}

		public override TlsAuthentication GetAuthentication()
		{
			return new MyTlsAuthentication(host);
		}
	}
}

