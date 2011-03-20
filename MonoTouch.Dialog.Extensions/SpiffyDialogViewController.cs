using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

namespace MonoTouch.Dialog.Extensions
{
	public class SpiffyDialogViewController : DialogViewController 
	{
		UIImage bgImage;
		
		public UIImage BgImage
		{
			get
			{
				return bgImage;
			}
			set
			{
				bgImage = value;
				LoadView();
			}
		}
		
		public SpiffyDialogViewController (RootElement root, bool pushing, UIImage bgImage) 
	    	: base (root, pushing)
	    {
			this.bgImage = bgImage;
	    }
		
		public override void LoadView ()
	    {
	        base.LoadView ();
			
			if(bgImage != null)
			{
		        var color = UIColor.FromPatternImage(bgImage);
		        
				if(TableView.RespondsToSelector(new Selector("backgroundView")))
					TableView.BackgroundView = new UIView();
				
				if(ParentViewController != null)
				{
					TableView.BackgroundColor = UIColor.Clear;
					ParentViewController.View.BackgroundColor = color;
				}
				else
				{
					TableView.BackgroundColor = color;
				}
				
			}
			
	    }
		
		public event EventHandler ViewAppearing;
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (ViewAppearing != null)
				ViewAppearing (this, EventArgs.Empty);
		}
		
	}
	
}
