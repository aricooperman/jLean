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
﻿package com.quantconnect.lean.Brokerages.Oanda.DataType
{
#pragma warning disable 1591
    /**
     * Represents the Oanda Account.
    */
    public class Account
    {
		public boolean HasAccountId;
		private int _accountId;
		public int accountId
		{
			get { return _accountId; }
			set
			{
				_accountId = value;
				HasAccountId = true;
			}
		}

		public boolean HasAccountName;
		private String _accountName;
		public String accountName
		{
			get { return _accountName; }
			set
			{
				_accountName = value;
				HasAccountName = true;
			}
		}

		public boolean HasAccountCurrency;
		private String _accountCurrency;
		public String accountCurrency
		{
			get { return _accountCurrency; }
			set
			{
				_accountCurrency = value;
				HasAccountCurrency = true;
			}
		}

		public boolean HasMarginRate;
		private String _marginRate;
		public String marginRate
		{
			get { return _marginRate; }
			set
			{
				_marginRate = value;
				HasMarginRate = true;
			}
		}

		[IsOptional]
		public boolean HasBalance;
		private String _balance;
		public String balance
		{
			get { return _balance; }
			set
			{
				_balance = value;
				HasBalance = true;
			}
		}

		[IsOptional]
		public boolean HasUnrealizedPl;
		private String _unrealizedPl;
		public String unrealizedPl
		{
			get { return _unrealizedPl; }
			set
			{
				_unrealizedPl = value;
				HasUnrealizedPl = true;
			}
		}

		[IsOptional]
		public boolean HasRealizedPl;
		private String _realizedPl;
		public String realizedPl
		{
			get { return _realizedPl; }
			set
			{
				_realizedPl = value;
				HasRealizedPl = true;
			}
		}

		[IsOptional]
		public boolean HasMarginUsed;
		private String _marginUsed;
		public String marginUsed
		{
			get { return _marginUsed; }
			set
			{
				_marginUsed = value;
				HasMarginUsed = true;
			}
		}

		[IsOptional]
		public boolean HasMarginAvail;
		private String _marginAvail;
		public String marginAvail
		{
			get { return _marginAvail; }
			set
			{
				_marginAvail = value;
				HasMarginAvail = true;
			}
		}
		
		[IsOptional]
		public boolean HasOpenTrades;
		private String _openTrades;
		public String openTrades
		{
			get { return _openTrades; }
			set
			{
				_openTrades = value;
				HasOpenTrades = true;
			}
		}
		
		[IsOptional]
		public boolean HasOpenOrders;
		private String _openOrders;
		public String openOrders
		{
			get { return _openOrders; }
			set
			{
				_openOrders = value;
				HasOpenOrders = true;
			}
		}
    }
#pragma warning restore 1591
}