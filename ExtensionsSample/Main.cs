
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog.Extensions;
using MonoTouch.Dialog;

namespace ExtensionsSample
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override void ReceiveMemoryWarning (UIApplication application)
		{
			//You should not call base in this method :)
			//base.ReceiveMemoryWarning (application);
			
			Console.WriteLine("Low Memory Detected.  Cleaning up LRU image cache");
			
			ImageStore.ReclaimMemory();
			
		}
		
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);
			
			window.AddSubview(navigation.View);
			
			window.MakeKeyAndVisible ();
			
			CreateImages();
			
			return true;
		}
		
		public static string[] SearchImages(string query, int startPosition, int resultsRequested, bool filterSimilarResults)
		{
			// Check preconditions
			if (resultsRequested < 1)
			{
				throw new ArgumentOutOfRangeException("resultsRequested", "Value must be positive");
			}
			else if (startPosition < 0)
			{
				throw new ArgumentOutOfRangeException("startPosition", "Value must be positive");
			}
			else if (resultsRequested + startPosition > 1000)
			{
				throw new ArgumentOutOfRangeException("resultsRequested", "Sorry, Google does not serve more than 1000 results for any query");
			}
			
			string safeSearchStr = "off";
			ArrayList results = new ArrayList();
			
			// Since Google returns 20 results at a time, we have to run the query over and over again (each
			// time with a different starting position) until we get the requested number of results.
		    // Note: During changes to the Google Image search from around January 2007 they
		    // stopped returned a fixed number of 20 images for each query. Instead, they try to return
		    // the amount of images that would fit the browser window. It seems that the "&ndsp" parameter
		    // indicates the number of images per query, so I'm using it manually. 
		    // This whole mechanism doesn't seem to work very accurately - in some cases I receive more images
		    // when my browser window is smaller rather than larger. Also, they seem to not always retrieve the
		    // requested number of results (usually I get 21 results when querying programmatically). 
		    // I'm leaving the flag on anyway, in case they decide to start respecting it correctly...
			for (int i = 0; i < resultsRequested; i+=20)
			{
				string requestUri = string.Format("http://images.google.com/images?q={0}&ndsp={1}&start={2}&filter={3}&safe={4}",
                    query, "20",(startPosition + i).ToString(), (filterSimilarResults) ? "1" : "0", safeSearchStr);

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
				string resultPage = string.Empty;
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse())
				{
					using (Stream responseStream = httpWebResponse.GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(responseStream))
						{
							resultPage = reader.ReadToEnd();
						}
					}
				}

				Regex r;
	            Match m;
	
	            // regular expression for images (not exhaustive)
	            r = new Regex("src\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))",
	                  RegexOptions.IgnoreCase);
	
	            // loop through matches
	            for (m = r.Match(resultPage); m.Success; m = m.NextMatch())
	            {
	                string strPageURL = m.Groups[1].ToString();
					
					Uri uri;
					
					if(Uri.TryCreate(strPageURL,UriKind.Absolute,out uri))
					{
						string strUrl = uri.ToString();
						
						if(strUrl.ToLower().IndexOf("file://") == -1)
							results.Add(strUrl);
						
					}
	
	            }
				
				if (results.Count == 0)
				{
					Console.WriteLine("Parsing of query " + query + " failed - collections count mismatch");
					break;
				}

			}

			return (string[]) results.ToArray(typeof(string));

		}
	
		
		void CreateImages()
		{
			int i = 1;
			RootElement root = new RootElement("Test Image Loader");
			
			foreach(string result in SearchImages("MonoTouch", 1, 99,false))
			{
				UrlImageStringElement element = new UrlImageStringElement(result,i,result);
				
				root.Add(new Section(){element});
				
				i++;
			}
		
			DialogViewController dvc = new DialogViewController(root){RotateUIEnabled=true};
			
			navigation.PushViewController(dvc,true);
			
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}
