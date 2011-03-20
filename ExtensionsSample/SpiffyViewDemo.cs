using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Extensions;

namespace ExtensionsSample
{
	public class SpiffyViewDemo : IImageUpdated
	{
		UINavigationController navigation;
		UIImage imgBG;
		SpiffyDialogViewController sdvc;
		long id;
		RootElement root;
		
		public SpiffyViewDemo (UINavigationController navigation)
		{
			this.navigation = navigation;
			
		}
		
		void IImageUpdated.UpdatedImage (long onId)
		{
			// Discard notifications that might have been queued for an old cell
			if(this.id != onId)
				return;

			imgBG = ImageStore.GetImage (onId);
			
			ShowMe(root, imgBG);
			
		}
		
		public void Show(long imageId, string imageUrl)
		{
			id = imageId;
			
			root = new RootElement("Spiffy Dialog View Controller Demo")
			{
				new Section()
				{
					new MultilineElement(string.Format("This is a Spiffy Dialog View Controller with a cool background. Loaded from {0}", imageUrl))
				}
			};
			
			imgBG = ImageStore.RequestImage(imageId, imageUrl, this);
			
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

