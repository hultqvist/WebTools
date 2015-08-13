using System;
using ARSoft.Tools.Net.Dns;
using System.Net;
using System.Net.Sockets;
using SilentOrbit.IO;
using System.Text;
using System.IO;
using System.Globalization;
using System.Net.Security;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using SilentOrbit.Web;
using System.Threading;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

namespace SilentOrbit.Web
{
	/// <summary>
	/// Fetches one file using a HTTP session
	/// </summary>
	public class HttpFetcher : IDisposable
	{
		/// <summary>
		/// Fethcer will abort if received size is larger than this
		/// </summary>
		public int MaxContentLength = -1;
		public string UserAgent = "HttpFetcher";
		public string Accept = "*/*";

		public bool KeepAlive { get; set; }

		public TimeSpan ConnectTimeout = TimeSpan.FromSeconds(60);

		public DateTime IfModifiedSince { get; set; }

		Socket socket;
		Stream stream;
		StreamLineReader reader;

		public HttpFetcher()
		{
		}

		[Obsolete]
		public static HttpFetchResponse Fetch(string url)
		{
			try
			{
				using (var f = new HttpFetcher())
				{
					return f.Get(new Uri(url));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				var resp = new HttpFetchResponse();
				resp.Exception = e;
				return resp;
			}
		}

		[Obsolete]
		public HttpFetchResponse Get(Uri url)
		{
			Connect(url);
			SendGetRequest(url);
			var resp = ReadHeaders();
			ReadBody(resp);
			return resp;
		}

		public void Dispose()
		{
			if (socket == null)
				return;
			if (socket.Connected)
				socket.Shutdown(SocketShutdown.Both);
			socket = null;
		}

		public void Connect(Uri address)
		{
			IPAddress ip;

			if (address.HostNameType == UriHostNameType.Dns)
				ip = LookupDNS(address.Host);
			else
				ip = IPAddress.Parse(address.Host);

			if (ip == null)
				throw new HttpFetchException("No DNS for " + address.Host);

			//TODO: loop for all dns ips on connect failures

			var tcp = new TcpClient();
			var result = tcp.BeginConnect(ip, address.Port, null, null);
			if (!result.AsyncWaitHandle.WaitOne(ConnectTimeout, true))
			{
				tcp.Close();
				throw new HttpFetchException("Timeout connecting to " + ip);
			}
			tcp.EndConnect(result);
			socket = tcp.Client;
			stream = tcp.GetStream();

			if (address.Scheme == "https")
			{
				//Broken in mono with newer ciphers
				/*
				var ssl = new SslStream(stream, false, VerifyCert);
				ssl.AuthenticateAsClient(address.Host);
				stream = ssl;*/

				var handler = new TlsClientProtocol(stream, new SecureRandom());
				handler.Connect(new MyTlsClient(address.Host));
				stream = handler.Stream;
			}

			reader = new StreamLineReader(stream);
			Console.WriteLine("Connected to " + socket.RemoteEndPoint + "(" + address.Host + ")");
		}
		/*
		const string serverKey = "";
		const string boxHash = "2...9";
		static readonly X509Certificate2Collection certs = new X509Certificate2Collection(
			new X509Certificate2(Convert.FromBase64String(serverKey))
			);*/
		static bool VerifyCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			//TODO: store and remember certificate changes
			//Notify when cert changes

			//For now accept all certs
			return true;
		}

		public static IPAddress LookupDNS(string hostname)
		{
			DnsMessage r;

			//IPv4
			r = DnsClient.Default.Resolve(hostname, RecordType.A);
			if (r == null)
				return null;
			foreach (var ar in r.AnswerRecords)
			{
				var a = ar as ARecord;
				if (a == null)
					continue;
				return a.Address;
			}

			//IPv6
			r = DnsClient.Default.Resolve(hostname, RecordType.Aaaa);
			if (r == null)
				return null;
			foreach (var ar in r.AnswerRecords)
			{
				var a = ar as AaaaRecord;
				if (a == null)
					continue;
				return a.Address;
			}

			return null;
		}

		[Conditional("DEBUG")]
		static void DebugLine(string msg)
		{
			//Console.WriteLine(msg);
		}

		public void SendGetRequest(Uri uri)
		{
			string request = "GET " + uri.PathAndQuery + " HTTP/1.1\r\n" +
			                 "Host: " + uri.Host + "\r\n" +
			                 "User-Agent: " + UserAgent + "\r\n" +
			                 "Accept: " + Accept + "\r\n" +
			                 (IfModifiedSince > DateTime.MinValue ? "If-Modified-Since: " + IfModifiedSince.ToUniversalTime().ToString("r") + "\r\n" : "") +
			                 "Connection: " + (KeepAlive ? "Keep-Alive" : "close") + "\r\n" +
			                 "\r\n";
			DebugLine(request);
			var buffer = Encoding.ASCII.GetBytes(request);
			stream.Write(buffer, 0, buffer.Length);
		}

		public HttpFetchResponse ReadHeaders()
		{
			//Read headers
			var resp = new HttpFetchResponse();

			//HTTP response
			string line = reader.ReadLine();
			if (line == null)
				throw new EndOfStreamException();
			string[] parts = line.Split(new char[] { ' ' }, 3);

			resp.HttpVersion = parts[0];

			try
			{
				resp.StatusCode = (HttpStatusCode)int.Parse(parts[1]);
			}
			catch (FormatException)
			{
				resp.StatusCode = (HttpStatusCode)(-1);
				resp.StatusMessage = "Invalid HTTP StatusCode: " + line;
				return resp;
			}

			if (parts.Length == 3)
				resp.StatusMessage = parts[2];
			else if (parts.Length == 2)
				resp.StatusMessage = "";
			else
			{
				resp.StatusCode = (HttpStatusCode)(-1);
				resp.StatusMessage = "Invalid HTTP response: " + line;
				return resp;
			}

			DebugLine(line);

			//Remaining headers
			while (true)
			{
				line = reader.ReadLine();
				DebugLine(line);
				if (line == "")
					return resp;

				int sep = line.IndexOf(':');
				if (sep < 0)
					continue;
				string key = line.Substring(0, sep).ToLowerInvariant();
				string val = line.Substring(sep + 1).Trim();

#if DEBUG
				if (key.StartsWith("x-"))
					continue;
#endif

				switch (key)
				{
					case "location":
						resp.Location = val;
						break;
					case "connection":
						val = val.ToLowerInvariant();
						if (val == "close")
							resp.KeepAlive = false;
						else if (val.Contains("keep-alive"))
							resp.KeepAlive = true;
						else
							Console.WriteLine("ResponseHeader: unknown Connection: " + val);
						break;
					case "content-length":
						long.TryParse(val, out resp.ContentLength);
						break;
					case "last-modified":
						DateTime dt;
						if (SilentOrbit.Parsing.DateParser.TryParse(val, out dt))
							resp.LastModified = dt;
						else
							Console.Error.WriteLine("Parse failed: " + line);
						break;
					case "transfer-encoding":
						val = val.ToLowerInvariant().Trim();
						if (val == "chunked")
							resp.ChunkedTransferEncoding = true;
						else
							throw new NotImplementedException(line);
						break;

						#if DEBUG
					case "server":
					case "date":
					case "content-type":
					case "set-cookie":
					case "vary":
					case "accept-ranges":
					case "etag":
					case "p3p":
					case "expires":
					case "pragma":
					case "cache-control":
					case "microsoftofficewebserver":
					case "wp-super-cache":
					case "via":
					case "age":
					case "status":
					case "communityserver":
					case "gdata-version":
					case "strict-transport-security":
					case "cf-ray":
					case "content-language":
					case "keep-alive":
					case "content-disposition":
					case "mw-webserver":
					case "telligent-evolution":
					case "host-header":
					case "link":
					case "ms-author-via":
					case "alternate-protocol":
					case "pool-info":
					case "edge-control":
					case "content-security-policy":
						break; //Ignore
					default:
						throw new NotImplementedException(line);
						#endif
				}
			}
		}

		public void ReadBody(HttpFetchResponse resp)
		{
			resp.Stream = new MemoryStream();
			if (resp.StatusCode == HttpStatusCode.NotModified)
				return;
			if (resp.ContentLength == 0)
				return;

			using (Timer timeout = new Timer(ReadTimeout, null, TimeSpan.FromMinutes(3), Timeout.InfiniteTimeSpan))
			{
				if (resp.ChunkedTransferEncoding)
				{
					ReadChunkedBody(resp.Stream);
					resp.Stream.Seek(0, SeekOrigin.Begin);
					return;
				}

				if (resp.ContentLength < 0)
				{
					byte[] buffer = reader.GetBuffered();
					resp.Stream.Write(buffer, 0, buffer.Length);
					reader.ReadToEnd(resp.Stream, (int)MaxContentLength);
					resp.Stream.Seek(0, SeekOrigin.Begin);
					return;
				}

				if (resp.ContentLength > MaxContentLength)
					throw new InvalidDataException("Content-Length(" + resp.ContentLength + ") > MaxContentLength(" + MaxContentLength + ")");

				//Read given length
				byte[] data = reader.ReadBytes((int)resp.ContentLength);
				resp.Stream = new MemoryStream(data);
			}
		}

		void ReadTimeout(object state)
		{
			Console.WriteLine("HTTP body read timeout");
			socket.Shutdown(SocketShutdown.Both);
		}

		void ReadChunkedBody(MemoryStream outStream)
		{
			while (true)
			{
				string line = reader.ReadLine();
				if (line == "")
					line = reader.ReadLine();
				int length = Convert.ToInt32(line, 16);
				if (length == 0)
					return;
				byte[] data = reader.ReadBytes(length);
				outStream.Write(data, 0, data.Length);
			}
		}
	}
}

