/*
Copyright (c) 2010 Novell Inc.

Author: Miguel de Icaza
Updates: Warren Moxley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

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
using System.Drawing;

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
			
			Console.WriteLine ("Low Memory Detected.  Cleaning up LRU image cache");
			
			ImageStore.ReclaimMemory ();
			
		}

		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);
			
			window.AddSubview (navigation.View);
			
			window.MakeKeyAndVisible ();
			
			CreateSearch();
			
			return true;
		}

		public static string[] SearchImages (string query, int startPosition, int resultsRequested, bool filterSimilarResults)
		{
			// Check preconditions
			if (resultsRequested < 1) {
				throw new ArgumentOutOfRangeException ("resultsRequested", "Value must be positive");
			} else if (startPosition < 0) {
				throw new ArgumentOutOfRangeException ("startPosition", "Value must be positive");
			} else if (resultsRequested + startPosition > 1000) {
				throw new ArgumentOutOfRangeException ("resultsRequested", "Sorry, Google does not serve more than 1000 results for any query");
			}
			
			string safeSearchStr = "off";
			ArrayList results = new ArrayList ();
			
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
			for (int i = 0; i < resultsRequested; i += 20) {
				string requestUri = string.Format ("http://images.google.com/images?q={0}&ndsp={1}&start={2}&filter={3}&safe={4}", query, "20", (startPosition + i).ToString (), (filterSimilarResults) ? "1" : "0", safeSearchStr);
				
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUri);
				string resultPage = string.Empty;
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse ()) {
					using (Stream responseStream = httpWebResponse.GetResponseStream ()) {
						using (StreamReader reader = new StreamReader (responseStream)) {
							resultPage = reader.ReadToEnd ();
						}
					}
				}
				
				Regex r;
				Match m;
				
				// regular expression for images (not exhaustive)
				r = new Regex ("src\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))", RegexOptions.IgnoreCase);
				
				// loop through matches
				for (m = r.Match (resultPage); m.Success; m = m.NextMatch ()) {
					string strPageURL = m.Groups[1].ToString ();
					
					Uri uri;
					
					if (Uri.TryCreate (strPageURL, UriKind.Absolute, out uri)) {
						string strUrl = uri.ToString ();
						
						if (strUrl.ToLower ().IndexOf ("file://") == -1)
							results.Add (strUrl);
						
					}
					
				}
				
				if (results.Count == 0) {
					Console.WriteLine ("Parsing of query " + query + " failed - collections count mismatch");
					break;
				}
				
			}
			
			return (string[])results.ToArray (typeof(string));
			
		}

		void CreateSearch()
		{
			RootElement root = new RootElement("Search Google Images");
			
			DialogViewController dvc = new DialogViewController (root) { RotateUIEnabled = true };
			
			var f = new RectangleF (0f, 0f, dvc.View.Bounds.Width, 44f);
			SearchDelegate searchDelegate = new SearchDelegate();
			searchDelegate.OnSearch+= HandleSearchDelegateOnSearch;
		
			bar = new UISearchBar (f) { Delegate = searchDelegate, ShowsCancelButton = true };
			
			dvc.View.InsertSubview (bar, 0);
			
			navigation.PushViewController (dvc, true);
				
		}

		void CreateImages (string searchTerm)
		{
			Util.PushNetworkActive();
			
			ImageStore.ClearCache();

			int i = 1;
			RootElement root = new RootElement (searchTerm + " Results");
		
			if(!string.IsNullOrEmpty(searchTerm))
			{
				foreach (string result in SearchImages (searchTerm, 1, 99, false)) {
					UrlImageStringElement element = new UrlImageStringElement (result, i, result);
					
					root.Add (new Section { element });
					
					i++;
				}
			}
			
			DialogViewController dvc = new DialogViewController (root,true) { RotateUIEnabled = true };
			
			navigation.PushViewController (dvc, true);
			
			Util.PopNetworkActive();
			
		}
	
		void HandleSearchDelegateOnSearch (object sender, EventArgs e)
		{
			if(bar != null)
				CreateImages(bar.Text);
			
		}
		

		UISearchBar bar;
		
		class SearchDelegate : UISearchBarDelegate
		{
			public event EventHandler<EventArgs> OnSearch;
			
			public override void SearchButtonClicked (UISearchBar bar)
			{
				bar.ResignFirstResponder ();
				
				if(!string.IsNullOrEmpty(bar.Text))
				   if(OnSearch != null)
						OnSearch(this,new EventArgs());
				   	
			}

			public override void CancelButtonClicked (UISearchBar bar)
			{
				bar.ResignFirstResponder ();
			}
			
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}
