/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using Newtonsoft.Json;

package com.quantconnect.lean.Brokerages.Tradier
{
    /**
     * Inside "Account" User-account balance information.
    */
    public class TradierBalance
    {
    //     * Account Number
    //    @JsonProperty( "account_number")]
    //    public long AccountNumber;

         * Balances of the Tradier Account:
        @JsonProperty( "balances")]
        public TradierBalanceDetails Balances;
    }

    /**
     * Trader Balance Detail:
    */
    public class TradierBalanceDetails 
    {
        ///Account Number
        @JsonProperty( "account_number")]
        public long AccountNumber;

        ///Account Class (margin, cash, pdt)
        @JsonProperty( "account_type")]
        public TradierAccountType Type;

        ///The amount of cash that could be withdrawn or invested in new investments, cash that is not required to support existing positions
        @JsonProperty( "cash_available")]
        public BigDecimal CashAvailable;

         * The ProfitLoss of the current trading day’s closed positions.
        @JsonProperty( "close_pl")]
        public BigDecimal ClosingProfitLoss;

         * The option requirement of current account positions.
        @JsonProperty( "current_requirement")]
        public BigDecimal CurrentRequirement;

         * Dividend Balance
        @JsonProperty( "dividend_balance")]
        public BigDecimal DividendBalance;

         * Equity Value
        @JsonProperty( "equity")]
        public BigDecimal Equity;

         * Long Liquid Value
        @JsonProperty( "long_liquid_value")]
        public BigDecimal LongLiquidValue;

         * Long Market Value
        @JsonProperty( "long_market_value")]
        public BigDecimal LongMarketValue;

         * Market Value
        @JsonProperty( "market_value")]
        public BigDecimal MarketValue;

         * Net Value
        @JsonProperty( "net_value")]
        public BigDecimal NetValue;

         * The Profit Loss of current account positions.
        @JsonProperty( "open_pl")]
        public BigDecimal OpenProfitLoss;

         * The value of long options held in the account.
        @JsonProperty( "option_long_value")]
        public BigDecimal OptionLongValue;

         * Option Requirement
        @JsonProperty( "option_requirement")]
        public BigDecimal OptionRequirement;

         * The value of short options held in the account.
        @JsonProperty( "option_short_value")]
        public BigDecimal OptionShortValue;

         * The amount of cash that is being held for open orders.
        @JsonProperty( "pending_cash")]
        public BigDecimal PendingCash;

         * The amount of open orders.
        @JsonProperty( "pending_orders_count")]
        public int PendingOrdersCount;

        ///Short Liquid Value
        @JsonProperty( "short_liquid_value")]
        public BigDecimal ShortLiquidValue;

        ///Short Market Value
        @JsonProperty( "short_market_value")]
        public BigDecimal ShortMarketValue;

        ///The value of long stocks held in the account.
        @JsonProperty( "stock_long_value")]
        public BigDecimal StockLongValue;

        ///The amount of funds that are not currently available for trading.
        @JsonProperty( "uncleared_funds")]
        public BigDecimal UnclearedFunds;

        ///Cash that is in the account from recent stock or option sales, but has not yet settled; 
        ///cash from stock sales occurring during the last 3 trading days or from option sales occurring during the previous trading day.
        @JsonProperty( "unsettled_funds")]
        public BigDecimal UnsettledFunds;

        ///The total amount of cash in the account.
        @JsonProperty( "total_cash")]
        public BigDecimal TotalCash;

        ///The total account value.
        @JsonProperty( "total_equity")]
        public BigDecimal TotalEquity;

         * Settings class for PDT specific accounts:
        @JsonProperty( "cash")]
        public TradierAccountTypeCash CashTypeSettings;

         * Settings class for PDT specific accounts:
        @JsonProperty( "pdt")]
        public TradierAccountTypeDayTrader PatternTraderTypeSettings;

         * Settings class for margin specific accounts
        @JsonProperty( "margin")]
        public TradierAccountTypeMargin MarginTypeSettings;
    }

    /**
     * Common Account Settings.
    */
    public class TradierAccountTypeSettings
    {
        ///The amount that the account is in deficit for trades that have occurred but not been paid for.
        @JsonProperty( "fed_call")]
        public BigDecimal FedCall;

        ///The amount that the account is under the minimum equity required in the account to support the current holdings.
        @JsonProperty( "maintenance_call")]
        public BigDecimal MaintenanceCall;

        ///The amount of funds available to purchase fully marginable securities.
        @JsonProperty( "stock_buying_power")]
        public BigDecimal StockBuyingPower;

        ///The amount of funds available to purchase non-marginable securities.
        @JsonProperty( "option_buying_power")]
        public BigDecimal OptionBuyingPower;

        ///The value of short stocks held in the account.
        @JsonProperty( "stock_short_value")]
        public BigDecimal StockShortValue;

        ///Constructor
        public TradierAccountTypeSettings() { }
    }

    /**
     * Account Class Day Trader Settings:
    */
    public class TradierAccountTypeDayTrader : TradierAccountTypeSettings
    {
        ///The total amount of funds available for the purchase of fully marginable stock during that trading day, a portion of these funds cannot be held overnight.
        @JsonProperty( "day_trade_buying_power")]
        public BigDecimal DayTradeBuyingPower;

         * Constructor
        public TradierAccountTypeDayTrader() { }
    }

    /**
     * Account Class Margin Settings:
    */
    public class TradierAccountTypeMargin : TradierAccountTypeSettings
    {
        ///"Sweep"
        @JsonProperty( "sweep")]
        public int Sweep;

         * Constructor
        public TradierAccountTypeMargin() { }
    }

    /**
     * Account Class Margin Settings:
    */
    public class TradierAccountTypeCash
    {
        ///"Sweep"
        @JsonProperty( "sweep")]
        public int Sweep;

        ///"Cash Available"
        @JsonProperty( "cash_available")]
        public BigDecimal CashAvailable;

        ///"Unsettled."
        @JsonProperty( "unsettled_funds")]
        public BigDecimal UnsettledFunds;

         * Constructor
        public TradierAccountTypeCash() { }
    }
}
