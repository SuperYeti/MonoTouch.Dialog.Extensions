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
		
	    public SpiffyDialogViewController (RootElement root, bool pushing, UIImage bgImage) 
	    : base (root, pushing) 
	    {
	        this.bgImage = bgImage;
	    }
	
	    public override void LoadView ()
	    {
	        base.LoadView ();
	        var color = UIColor.FromPatternImage(bgImage);
	        
			if(TableView.RespondsToSelector(new Selector("backgroundView")))
				TableView.BackgroundView = new UIView();
			
			TableView.BackgroundColor = UIColor.Clear;
			
			ParentViewController.View.BackgroundColor = color;
			
	    }
		
	}
	
}
