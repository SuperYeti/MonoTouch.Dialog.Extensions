using System;
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace MonoTouch.Dialog.Extensions
{
	public class InchFractionPickerModel : UIPickerViewModel
	{
		public NSAction selected;
		
		static int minInches = 1;
		static int maxInches = 100;
		
		public static int MinInches
		{
			get{return minInches;}
			set
			{
				minInches = value;
				inches = null;
			}
		}
		
		public static int MaxInches
		{
			get{return maxInches;}
			set
			{
				maxInches = value;
				inches = null;
			}
		}
		
		static string[] inches;
			
		public static string[] Inches
		{
			get
			{
				if(inches == null || inches.Length <= 0)
				{
					List<string> inchTemp = new List<string>();
					
					inchTemp.Add("");
					
					for(int i = minInches; i <= maxInches; i++)
						inchTemp.Add(i.ToString());
				
					inches = inchTemp.ToArray();
					
				}
				
				return inches;
				
			}
			
		}
		
		static string[] fractions;
			
		public static string[] Fractions
		{
			get
			{
				if(fractions == null || fractions.Length <= 0)
				{
					fractions = new string[]{
						"",
						"1/32",
						"1/16",
						"3/32",
						"1/8",
						"5/32",
						"3/16",
						"7/32",
						"1/4",
						"9/32",
						"5/16",
						"11/32",
						"3/8",
						"13/32",
						"7/16",
						"15/32",
						"17/32",
						"9/16",
						"19/32",
						"5/8",
						"21/32",
						"11/16",
						"23/32",
						"3/4",
						"25/32",
						"13/16",
						"27/32",
						"7/8",
						"29/32",
						"15/16",
						"31/32"
					};
					
				}
				
				return fractions;
				
			}
			
		}
		
		decimal _RealValue;
		
		public decimal RealValue
		{
			get
			{
				return _RealValue;
			}
			set
			{
				_RealValue = Math.Round(value * 32, 0) / 32;
				
				selectedInches = (int)_RealValue;
				
				decimal dRemainder = _RealValue - selectedInches;
				
				selectedFraction = Fraction.ToFraction(dRemainder);
				
			}
			
		}
		
		public InchFractionPickerModel(decimal realValue, int minInches, int maxInches){
			RealValue = realValue;
			InchFractionPickerModel.MinInches = minInches;
			InchFractionPickerModel.MaxInches = maxInches;
			
		}
		
		public void UpdateSelection(UIPickerView uipv)
		{
			if(selectedInches != null)
			{
				for(int i = 0; i < Inches.Length; i++)
				{
					if(Inches[i] == selectedInches.ToString())
					{
						uipv.Select(i, 0, true);
						break;
					}
				}
			}
			
			if(selectedFraction != null)
			{
				for(int i = 0; i < Fractions.Length; i++)
				{
					if(Fractions[i] == selectedFraction.ToString())
					{
						uipv.Select(i, 1, true);
						break;
					}
				}
			}
			
		}
		
		public override int GetComponentCount(UIPickerView uipv)
		{
			return(2);
		}
		
		public override int GetRowsInComponent( UIPickerView uipv, int comp)
		{
			switch(comp)
			{
				case 0:
					return Inches.Length;
					
				case 1:
					return Fractions.Length;
					
				default:
					return 0;
			}
			
		}
		
		public override string GetTitle(UIPickerView uipv, int row, int comp)
		{
			switch(comp)
			{
				case 0:
					return Inches[row];
					
				case 1:
					return Fractions[row];
					
				default:
					return "";
			}
			
		}
		
		int selectedInches = 0;
		
		Fraction selectedFraction = new Fraction(0);
		
		public override void Selected(UIPickerView uipv, int row, int comp)
		{
			switch(comp)
			{
				case 0:
					int.TryParse(Inches[row], out selectedInches);
					break;
				
				case 1:
					selectedFraction = Fraction.ToFraction(Fractions[row]);
					break;
			}
			
			_RealValue = selectedInches + selectedFraction.ToDecimal();
			
		}
		
		public override float GetComponentWidth(UIPickerView uipv, int comp){
			switch(comp)
			{
				case 0:
					return (75f);
				case 1:
					return (75f);	
				default:
					return (0f);
			}
		}
		
		public override float GetRowHeight(UIPickerView uipv, int comp){
			return(40f); 
		}
	}
	
}

