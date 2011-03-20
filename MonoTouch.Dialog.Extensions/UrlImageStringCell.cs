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
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Utilities;

namespace MonoTouch.Dialog.Extensions
{
	//TODO: Must fix this.  While merging the LRU, ImageLoader, and Graphics cores into MT.d
	//it was a breaking change.  I will fix it later.
	/*
	public class UrlImageStringCell : UITableViewCell, IImageUpdated {
		// Do these as static to reuse across all instances
		const int textSize = 15;
		
		const int PicSize = 48;
		const int PicXPad = 5;
		const int PicYPad = 5;
		
		const int TextLeftStart = 2 * PicXPad + PicSize;
		
		const int TextHeightPadding = 4;
		const int TextYOffset = 0;
		const int MinHeight = PicSize + 2 * PicYPad;
		
		static UIFont textFont = UIFont.SystemFontOfSize (textSize);
		
		long id;
		string imageUrl;
		string caption;
		
		UILabel textLabel;
		UIImageView imageView;
		
		public UrlImageStringCell (IntPtr handle) : base (handle) {
			Console.WriteLine (Environment.StackTrace);
		}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public UrlImageStringCell (UITableViewCellStyle style, NSString ident, string caption, long id, string imageUrl) : base (style, ident)
		{
			this.caption = caption;
			this.id = id;
			this.imageUrl = imageUrl;
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
			
			textLabel = new UILabel () {
				Font = textFont,
				TextAlignment = UITextAlignment.Left,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap
			};
			
			imageView = new UIImageView (new RectangleF (PicXPad, PicYPad, PicSize, PicSize));
			
			UpdateCell (caption, id, imageUrl);
			
			ContentView.Add (textLabel);
			ContentView.Add (imageView);
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (string caption, long id, string imageUrl)
		{
			this.caption = caption;
			this.id = id;
			this.imageUrl = imageUrl;
			
			textLabel.Text = caption;
			
			var img = ImageStore.GetImage (id);
			if (img == null)
				ImageStore.QueueRequestForImage(id, imageUrl, this);
			
			imageView.Image = img == null ? ImageStore.DefaultImage : img;
		}

		public static float GetCellHeight (RectangleF bounds, string caption)
		{
			bounds.Height = 999;
			
			// Keep the same as LayoutSubviews
			bounds.X = TextLeftStart;
			bounds.Width -= TextLeftStart+TextHeightPadding;
			
			using (var nss = new NSString (caption)){
				var dim = nss.StringSize (textFont, bounds.Size, UILineBreakMode.WordWrap);
				return Math.Max (dim.Height + TextYOffset + 2*TextHeightPadding, MinHeight);
			}
		}
		
		// 
		// Layouts the views, called before the cell is shown
		//
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var full = ContentView.Bounds;
			var tmp = full;

			tmp = full;
			tmp.Y += TextYOffset;
			tmp.Height -= TextYOffset;
			tmp.X = TextLeftStart;
			tmp.Width -= TextLeftStart+TextHeightPadding;
			textLabel.Frame = tmp;
		}
		
		void IImageUpdated.UpdatedImage (Uri uri)
		{
			// Discard notifications that might have been queued for an old cell
			if(this.id != onId)
				return;

			imageView.Alpha = 0;
			imageView.Image = ImageStore.GetImage (onId);
			imageView.SizeToFit();

			UIView.BeginAnimations (null, IntPtr.Zero);
			UIView.SetAnimationDuration (0.5);
			
			imageView.Alpha = 1;
			UIView.CommitAnimations ();
			
			throw new NotImplementedException("UrlImageStringCell::IImageUpdated.UpdatedImage must implement broken with latest merge from trunk");
		}
	}
	
	// 
	// A MonoTouch.Dialog.Element that renders a TweetCell
	//
	public class UrlImageStringElement : Element, IElementSizing {
		static NSString key = new NSString ("UrlImageStringElement");
		string caption;
		long id;
		string imageUrl;
		
		public UrlImageStringElement (string caption, long id, string imageUrl) : base (null)
		{
			this.caption = caption;
			this.id = id;
			this.imageUrl = imageUrl;
		}
		
		// Gets a cell on demand, reusing cells
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key) as UrlImageStringCell;
			if (cell == null)
				cell = new UrlImageStringCell (UITableViewCellStyle.Default, key,caption,id,imageUrl);
			else
				cell.UpdateCell (caption,id,imageUrl);
			
			return cell;
		}
		
		public event NSAction Tapped;
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
				Tapped ();
			tableView.DeselectRow (indexPath, true);
		}
				
		#region IElementSizing implementation
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return UrlImageStringCell.GetCellHeight (tableView.Bounds, caption);
		}
		#endregion
	}
	*/
}