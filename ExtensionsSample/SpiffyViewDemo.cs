using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Extensions;
using MonoTouch.Dialog.Utilities;
using System.IO;

namespace ExtensionsSample
{
	public class SpiffyViewDemo : IImageUpdated
	{
		UINavigationController navigation;
		UIImage imgBG;
		SpiffyDialogViewController sdvc;
		Uri uri;
		RootElement root;
		
		public SpiffyViewDemo (UINavigationController navigation)
		{
			this.navigation = navigation;
			
		}
		
		void IImageUpdated.UpdatedImage (Uri uri)
		{
			// Discard notifications that might have been queued for an old cell
			if(this.uri != uri)
				return;
			
			string picfile = uri.IsFile ? uri.LocalPath : ImageLoader.BaseDir + ImageLoader.md5 (uri.AbsoluteUri);
			if (File.Exists (picfile))
				imgBG = UIImage.FromFileUncached (picfile);
						
			ShowMe(root, imgBG);
			
		}
		
		public void Show(Uri imageUrl)
		{
			this.uri = imageUrl;
			
			root = new RootElement("Spiffy Dialog View Controller Demo")
			{
				new Section()
				{
					new MultilineElement(string.Format("This is a Spiffy Dialog View Controller with a cool background. Loaded from {0}", imageUrl))
				}
			};
			
			imgBG = ImageLoader.DefaultRequestImage(imageUrl, this);
			
			if(imgBG != null)
				ShowMe(root, imgBG);
			
		}
		
		private void ShowMe(RootElement root, UIImage imgBG)
		{
			sdvc = new SpiffyDialogViewController(root, true, imgBG);
				
			navigation.PushViewController(sdvc, true);
		
		}
		
	}
	
}

