using System;
using Org.BouncyCastle.Crypto.Tls;

namespace SilentOrbit.Web
{
	public class MyTlsAuthentication : TlsAuthentication
	{
		readonly string host;

		public MyTlsAuthentication(string host)
		{
			this.host = host;
		}

		public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
		{
			// return client certificate
			return null;
		}

		public void NotifyServerCertificate(Certificate serverCertificate)
		{
			var list = serverCertificate.GetCertificateList();
			Console.WriteLine(list);
			// validate server certificate
		}
	}
}

