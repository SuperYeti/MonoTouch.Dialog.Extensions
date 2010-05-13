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
			
            UIGraphics.BeginImageContext (image.Size);
			float imgWidth = image.Size.Width;
			float imgHeight = image.Size.Height;

            var c = UIGraphics.GetCurrentContext ();

            c.BeginPath ();
            c.MoveTo (imgWidth, imgHeight/2);
            c.AddArcToPoint (imgWidth, imgHeight, imgWidth/2, imgHeight, 4);
            c.AddArcToPoint (0, imgHeight, 0, imgHeight/2, 4);
            c.AddArcToPoint (0, 0, imgWidth/2, 0, 4);
            c.AddArcToPoint (imgWidth, 0, imgWidth, imgHeight/2, 4);
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
