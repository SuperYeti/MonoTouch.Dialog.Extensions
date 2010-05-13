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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace MonoTouch.Dialog.Extensions
{
	public interface IImageUpdated {
		void UpdatedImage (long id);
	
	}
	
	//
	// Provides an interface to download pictures in the background
	// and keep a local cache of the original files + rounded versions
	//
	public static class ImageStore
	{
		const int MaxRequests = 4;
		static string PicDir, SmallPicDir, TmpDir, RoundedPicDir; 
		public readonly static UIImage DefaultImage = UIImage.FromFile("Images/default.png");
		static LRUCache<long,UIImage> cache;
		
		// A list of requests that have been issues, with a list of objects to notify.
		static Dictionary<long, List<IImageUpdated>> pendingRequests;
		
		// A list of updates that have completed, we must notify the main thread about them.
		static HashSet<long> queuedUpdates;
		
		// A queue used to avoid flooding the network stack with HTTP requests
		static Queue<long> requestQueue;

		static NSString nsDispatcher = new NSString ("x");
		
		static private int capacity;
		
		static public int Capacity
		{
			get{return capacity;}
			set{
				capacity = value;
				Init();
			}
		}
		
		static ImageStore ()
		{
			PicDir = Path.Combine (Util.BaseDir, "Library/Caches/Pictures");
			TmpDir = Path.Combine (Util.BaseDir, "tmp/downloads/");
			SmallPicDir = Path.Combine (PicDir, "Scaled/");
			RoundedPicDir = Path.Combine (PicDir, "Rounded/");
				
			if(!Directory.Exists(PicDir))
				Directory.CreateDirectory(PicDir);
			
			if (!Directory.Exists (SmallPicDir))
				Directory.CreateDirectory (SmallPicDir);
			
			if (!Directory.Exists (TmpDir))
				Directory.CreateDirectory (TmpDir);
			
			if (!Directory.Exists (RoundedPicDir))
				Directory.CreateDirectory (RoundedPicDir);

			capacity = 100;
			Init();
			
		}
		
		static void Init()
		{
			cache = new LRUCache<long,UIImage> (capacity);
			pendingRequests = new Dictionary<long,List<IImageUpdated>> ();
			queuedUpdates = new HashSet<long>();
			requestQueue = new Queue<long> ();
		}
		
		public static void ClearCache()
		{
			cache.Clear();
			pendingRequests.Clear();
			queuedUpdates.Clear();
			requestQueue.Clear();
			
			var PicDir = Path.Combine (Util.BaseDir, "Library/Caches/Pictures");
			var TmpDir = Path.Combine (Util.BaseDir, "tmp/downloads/");
			var SmallPicDir = Path.Combine (PicDir, "Scaled/");
			var RoundedPicDir = Path.Combine (PicDir, "Rounded/");
			
			if(Directory.Exists(PicDir))
				foreach(string fil in Directory.GetFiles(PicDir))
				{
					File.Delete(fil);
				}
			
			if (Directory.Exists (SmallPicDir))
				foreach(string fil in Directory.GetFiles(SmallPicDir))
				{
					File.Delete(fil);
				}

			
			if (Directory.Exists (TmpDir))
				foreach(string fil in Directory.GetFiles(TmpDir))
				{
					File.Delete(fil);
				}

			
			if (Directory.Exists (RoundedPicDir))
				foreach(string fil in Directory.GetFiles(RoundedPicDir))
				{
					File.Delete(fil);
				}
			
		}
		
		public static UIImage GetImage (long id)
		{
			UIImage ret = null;
			
			lock (cache){
				
				if(cache.ContainsKey(id))
					ret = cache [id];
				
				if (ret != null)
					return ret;
			}
			
			if (pendingRequests.ContainsKey (id))
				return null;
			
			string picfile = RoundedPicDir + id + ".png";			
			if (File.Exists (picfile)){
				ret = UIImage.FromFileUncached (picfile);
				lock (cache)
				{
					cache [id] = ret;
				}
				
				return ret;
			} if (File.Exists (SmallPicDir + id + ".jpg"))
				return RoundedPic (id);
			else
				return null;
		}
		
		public static UIImage RequestImage (long id, string optionalUrl, IImageUpdated notify)
		{
			var pic = GetImage (id);
			if (pic == null){
				QueueRequestForImage (id, optionalUrl, notify);
				return DefaultImage;
			}
			
			return pic;
		}
		
		public static Uri GetImageUrlFromId (long id, string optionalUrl)
		{
			Uri url;
				
			if(!cache.ContainsKey(id) && optionalUrl != null && optionalUrl == string.Empty)
				return null;
			
			if (!Uri.TryCreate (optionalUrl, UriKind.Absolute, out url))
				return null;
			
			return url;
		}
		
		//
		// Requests that the picture for "id" be downloaded, the optional url prevents
		// one lookup, it can be null if not known
		//
		public static void QueueRequestForImage (long id, string optionalUrl, IImageUpdated notify)
		{	
			if (notify == null)
				throw new ArgumentNullException ("notify");
			
			Uri url = GetImageUrlFromId (id, optionalUrl);
			if (url == null)
				return;

			if (pendingRequests.ContainsKey (id)){
				pendingRequests [id].Add (notify);
				return;
			}
			var slot = new List<IImageUpdated> (MaxRequests);
			slot.Add (notify);
			pendingRequests [id] = slot;
			if (pendingRequests.Count >= MaxRequests){
				lock (requestQueue)
				{
					requestQueue.Enqueue (id);
				}
			} else {
				ThreadPool.QueueUserWorkItem (delegate { 
					try {
						StartImageDownload (id, url); 
					} catch (Exception e){
						Console.WriteLine (e);
					}
				});
			}
		}
				
		static void StartImageDownload (long id, Uri url)
		{
			do {
				var buffer = new byte [4*1024];
				
				using (var file = new FileStream (SmallPicDir+ id + ".jpg", FileMode.Create, FileAccess.Write, FileShare.Read)) {
	                	var req = WebRequest.Create (url) as HttpWebRequest;
					
	                using (var resp = req.GetResponse()) {
						using (var s = resp.GetResponseStream()) {
							int n;
							while ((n = s.Read (buffer, 0, buffer.Length)) > 0){
								file.Write (buffer, 0, n);
	                        }
							file.Flush();
						}
	                }
				}
				
				// Cluster all updates together
				bool doInvoke = false;
				lock (queuedUpdates){
					queuedUpdates.Add (id);
					
					// If this is the first queued update, must notify
					if (queuedUpdates.Count > 0)
						doInvoke = true;
				}
				
				// Try to get more jobs.
				lock (requestQueue){
					if (requestQueue.Count > 0){
						id = requestQueue.Dequeue ();
					} else
						id = -1;
				}
			
				url = GetImageUrlFromId (id, null);
				if (url == null)
					id = -1;
									
				if (doInvoke)
					nsDispatcher.BeginInvokeOnMainThread (NotifyImageListeners);
			} while (id != -1);
			
		}
		
		// Runs on the main thread
		static void NotifyImageListeners ()
		{
			try {
			lock (queuedUpdates){
				foreach (var qid in queuedUpdates){
					var list = pendingRequests [qid];
					long lID = qid;
					pendingRequests.Remove (lID);
					foreach (var pr in list){
						pr.UpdatedImage (lID);
					}	
				}
				queuedUpdates.Clear();
			}
			} catch (Exception e){
				Console.WriteLine (e);
			}
			         
		}
		
		static UIImage RoundedPic (long id)
		{
			lock (cache){
				string smallpic = SmallPicDir + id + ".jpg";
				
				using (var pic = UIImage.FromFileUncached (smallpic)){
					if (pic == null)
						return null;
					
					var cute = Graphics.RemoveSharpEdgesScale (pic);
					var bytes = cute.AsPNG ();
					NSError err;
					bytes.Save (RoundedPicDir + id + ".png", false, out err);
					
					// we might as well add it to the cache
					cache [id] = cute;
					
					return cute;
				}
			}
		}
		
		public static void ReclaimMemory()
		{
			lock (cache){
				if(cache.Count == 0)
					return;
				
				int iRemoveCnt = cache.Count/4;
				cache.ReclaimLRU(iRemoveCnt==0?1:iRemoveCnt);
				
			}
		}
	}
}