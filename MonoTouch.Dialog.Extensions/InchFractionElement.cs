using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog.Extensions
{
	public class InchFractionElement : StringElement {
		public decimal RealValue;
		public UIPickerView fractionPicker;
		
		public int minInches = 1;
		public int maxInches = 300;
		
		
		public InchFractionElement(string caption, decimal realValue) : base(caption)
		{
			RealValue = realValue;
			Value = realValue.ToString("F2");
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var iFeet = Math.Floor(RealValue / 12);
			var iInchesLeft = Math.Floor(RealValue % 12);
			var iRemainder = Fraction.ToFraction(RealValue - (iFeet * 12) - iInchesLeft);
			
			Value = iFeet.ToString() + "' " + iInchesLeft.ToString() + (iRemainder != 0 ? "-" + iRemainder.ToString() : "") + "\""; 
			return base.GetCell (tv);
		
		}
 
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				if (fractionPicker != null){
					fractionPicker.Dispose ();
					fractionPicker = null;
				}
			}
		}
		
		public virtual UIPickerView CreatePicker ()
		{
			var picker = new UIPickerView();
			
			picker.Model = new InchFractionPickerModel(RealValue, minInches, maxInches);
			
			return picker;
		}
		                                                                                                                                
		static RectangleF PickerFrameWithSize (SizeF size)
		{                                                                                                                                    
			var screenRect = UIScreen.MainScreen.ApplicationFrame;
			float fY = 0, fX = 0;
			
			switch (UIApplication.SharedApplication.StatusBarOrientation){
			case UIInterfaceOrientation.LandscapeLeft:
			case UIInterfaceOrientation.LandscapeRight:
				fX = (screenRect.Height - size.Width) /2;
				fY = (screenRect.Width - size.Height) / 2 -17;
				break;
				
			case UIInterfaceOrientation.Portrait:
			case UIInterfaceOrientation.PortraitUpsideDown:
				fX = (screenRect.Width - size.Width) / 2;
				fY = (screenRect.Height - size.Height) / 2 - 25;
				break;
			}
			
			return new RectangleF (fX, fY, size.Width, size.Height);
		}                                                                                                                                    

		class MyViewController : UIViewController {
			InchFractionElement container;
			
			public MyViewController (InchFractionElement container)
			{
				this.container = container;
			}
			
			public override void ViewWillAppear (bool animated)
			{
				base.ViewWillAppear (animated);
				
				var pickerModel = container.fractionPicker.Model as InchFractionPickerModel;
				
				if(pickerModel != null)
					pickerModel.UpdateSelection(container.fractionPicker);
				
			}
			
			public override void ViewWillDisappear (bool animated)
			{
				base.ViewWillDisappear (animated);
				container.RealValue = ((InchFractionPickerModel)container.fractionPicker.Model).RealValue;
				container.Value = container.RealValue.ToString("f");
			}
			
			public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
			{
				base.DidRotate (fromInterfaceOrientation);
				container.fractionPicker.Frame = PickerFrameWithSize (container.fractionPicker.SizeThatFits (SizeF.Empty));
			}
			
			public bool Autorotate { get; set; }
			
			public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
			{
				return Autorotate;
			}
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new MyViewController (this) {
				Autorotate = dvc.Autorotate
			};
			
			fractionPicker = CreatePicker();
			fractionPicker.Frame = PickerFrameWithSize(fractionPicker.SizeThatFits(SizeF.Empty));
			                            
			vc.View.BackgroundColor = UIColor.Black;
			vc.View.AddSubview (fractionPicker);
			dvc.ActivateController (vc);
		}
	}
	
}

