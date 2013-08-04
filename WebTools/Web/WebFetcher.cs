using System;
using System.Net;
using SilentOrbit.Web;
using System.Net.Sockets;

namespace SilentOrbit.Web
{
	/// <summary>
	/// Fetches an url and follows redirects
	/// </summary>
	public class WebFetcher
	{
		public bool FollowMovedPermanently { get; set; }

		public readonly HttpFetcher Http = new HttpFetcher();

		public WebFetcher()
		{
		}

		public HttpFetchResponse Fetch(Uri url)
		{
			try
			{
				//Previoulsy we used a exception catcher here
				return FetchInternal(url);
			}
			finally
			{
				Http.Dispose();
			}
		}

		/// <summary>
		/// Return the response following temporary redirections only
		/// </summary>
		HttpFetchResponse FetchInternal(Uri url)
		{
			int redirections = 0; //To limit number of redirection

			while (true)
			{
				//Fetch feed data
				Http.Connect(url);
				Http.SendGetRequest(url);
				var resp = Http.ReadHeaders();
				Http.ReadBody(resp);

				if (resp.StatusCode == HttpStatusCode.Found ||
					resp.StatusCode == HttpStatusCode.RedirectKeepVerb ||
					resp.StatusCode == HttpStatusCode.RedirectMethod ||
					(resp.StatusCode == HttpStatusCode.MovedPermanently && FollowMovedPermanently))
				{
					if (resp.Location.StartsWith("/"))
						url = new Uri(url, resp.Location);
					else
						url = new Uri(resp.Location);
					Console.WriteLine("Redirect to: " + resp.Location);
					redirections += 1;
					if (redirections > 10)
						throw new Exception("Too many redirection");
					Http.Dispose();
					continue;
				}
				return resp;
			}
		}
	}
}

