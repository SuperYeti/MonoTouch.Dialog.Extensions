//
// Utilities for dealing with graphics
//
using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog.Extensions
{
	public static class Graphics
	{
        // Child proof the image by rounding the edges of the image
        internal static UIImage RemoveSharpEdges (UIImage image)
        {
			if (image == null)
				throw new ArgumentNullException ("image");
			
            UIGraphics.BeginImageContext (new SizeF (48, 48));
            var c = UIGraphics.GetCurrentContext ();

            c.BeginPath ();
            c.MoveTo (48, 24);
            c.AddArcToPoint (48, 48, 24, 48, 4);
            c.AddArcToPoint (0, 48, 0, 24, 4);
            c.AddArcToPoint (0, 0, 24, 0, 4);
            c.AddArcToPoint (48, 0, 48, 24, 4);
            c.ClosePath ();
            c.Clip ();

            image.Draw (new PointF (0, 0));
            var converted = UIGraphics.GetImageFromCurrentImageContext ();
            UIGraphics.EndImageContext ();
            return converted;
        }
		
		internal static UIImage RemoveSharpEdgesScale(UIImage image)
		{
			//Perform Image manipulation, make the image fit into a 48x48 tile without clipping.  
			
			float fWidth = image.Size.Width;
			float fHeight = image.Size.Height;
			float fTotal = fWidth>=fHeight?fWidth:fHeight;
			float fDifPercent = 48 / fTotal;
			float fNewWidth = fWidth*fDifPercent;
			float fNewHeight = fHeight*fDifPercent;
			
			SizeF newSize = new SizeF(fNewWidth,fNewHeight);
			
			UIGraphics.BeginImageContext (newSize);
	        var context = UIGraphics.GetCurrentContext ();
	        context.TranslateCTM (0, newSize.Height);
	        context.ScaleCTM (1f, -1f);
	
	        context.DrawImage (new RectangleF (0, 0, newSize.Width, newSize.Height), image.CGImage);
	
	        var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
	        UIGraphics.EndImageContext();
	
			return RemoveSharpEdges(scaledImage);
			
		}
	}
}
