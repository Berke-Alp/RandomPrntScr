using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Flurl.Http;
using HtmlAgilityPack;
using System.IO;
using System.Net;
using Flurl;
using System.Collections.Concurrent;
using System.Threading;
using Flurl.Http.Configuration;

namespace RandomPrntScr
{
	public class ProxyHttpClientFactory : DefaultHttpClientFactory
	{
		private string _address;

		public ProxyHttpClientFactory(string address)
		{
			_address = address;
		}

		public override HttpMessageHandler CreateMessageHandler()
		{
			return new HttpClientHandler
			{
				Proxy = new WebProxy(_address),
				UseProxy = true
			};
		}
	}

	class Program
	{

		static void Main(string[] args)
		{
			Initialize();
			theLoop();
			Console.ReadKey();
		}

		static List<string> plist = new List<string>();
		static Random r = new Random();

		public static void Initialize()
		{
			StreamReader sr = new StreamReader("proxys.txt");
			string proxy = sr.ReadLine();
			while (proxy != null)
			{
				plist.Add(proxy);
				proxy = sr.ReadLine();
			}
			sr.Close();
		}

		static async void theLoop()
		{
			Console.Title = "Random Image Capturer";
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("How many images do you want?: ");
			Console.ForegroundColor = ConsoleColor.White;
			string input = Console.ReadLine();
			if (!int.TryParse(input, out int result)) theLoop();
			else
			{
				for (int i = 0; i < result; i++)
				{
					string buildme = "";
					buildme += (char)r.Next(97, 123); // 1. karakter
					buildme += (char)r.Next(97, 123); // 2. karakter
					buildme += (char)r.Next(48, 58); // 3. sayı
					buildme += (char)r.Next(48, 58); // 4. sayı
					buildme += (char)r.Next(48, 58); // 5. sayı
					buildme += (char)r.Next(48, 58); // 6. sayı
					string url = ("https://prnt.sc/" + buildme);

					try
					{

						var response = await url.WithHeaders(new
						{
							Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
							User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36"
						}).GetStringAsync();

						FlurlHttp.Configure(settings => {
							settings.HttpClientFactory = new ProxyHttpClientFactory(plist[r.Next(0, plist.Count)]);
						});

						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(response);
						string link = doc.DocumentNode.SelectSingleNode("//*[@id=\"screenshot-image\"]").Attributes["src"].Value;
						if (!Directory.Exists("captured")) Directory.CreateDirectory("captured");
						WebClient client = new WebClient();
						if (!File.Exists(buildme + ".png"))
						{
							try
							{
								client.DownloadFile(link, "captured/" + buildme + ".png");
							}
							catch (Exception)
							{
							}

						}
						Console.Title = "Random Image Capturer - " + ((i + 1) + ". file downloaded");
					}
					catch (Exception) { Console.WriteLine((i + 1) + ". file not found"); }
				}
			}
			Console.WriteLine("Process is done!");
		}
	}
}
