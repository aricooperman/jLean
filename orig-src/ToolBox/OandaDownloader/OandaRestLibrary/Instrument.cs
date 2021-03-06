﻿/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 OANDA Corporation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
 * documentation files (the "Software"), to deal in the Software without restriction, including without 
 * limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
 * Software, and to permit persons to whom the Software is furnished  to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
 * the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

package com.quantconnect.lean.ToolBox.OandaDownloader.OandaRestLibrary
{
	public class IsOptionalAttribute : Attribute
	{
		public @Override String toString() {
			return "Is Optional";
		}
	}

	public class MaxValueAttribute : Attribute
	{
		public object Value { get; set; }
		public MaxValueAttribute(int i) {
			Value = i;
		}
	}

	public class MinValueAttribute : Attribute
	{
		public object Value { get; set; }
		public MinValueAttribute(int i) {
			Value = i;
		}
	}

	public class Instrument
    {
		public boolean HasInstrument;
	    private String _instrument;
        public String instrument 
		{
			get { return _instrument; }
			set 
			{ 
				_instrument = value;
				HasInstrument = true;
			}
		}

		public boolean HasdisplayName;
		private String _displayName;
		public String displayName
		{
			get { return _displayName; }
			set
			{
				_displayName = value;
				HasdisplayName = true;
			}
		}

		public boolean Haspip;
		private String _pip;
		public String pip
		{
			get { return _pip; }
			set
			{
				_pip = value;
				Haspip = true;
			}
		}

		[IsOptional]
		public boolean HaspipLocation;
		private int _pipLocation;
		public int pipLocation
		{
			get { return _pipLocation; }
			set
			{
				_pipLocation = value;
				HaspipLocation = true;
			}
		}

		[IsOptional]
		public boolean HasextraPrecision;
		private int _extraPrecision;
		public int extraPrecision
		{
			get { return _extraPrecision; }
			set
			{
				_extraPrecision = value;
				HasextraPrecision = true;
			}
		}

		public boolean HasmaxTradeUnits;
		private int _maxTradeUnits;
		public int maxTradeUnits
		{
			get { return _maxTradeUnits; }
			set
			{
				_maxTradeUnits = value;
				HasmaxTradeUnits = true;
			}
		}
    }
}
