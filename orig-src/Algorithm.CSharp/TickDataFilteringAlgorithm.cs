using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Securities.Interfaces;

package com.quantconnect.lean.Algorithm.Examples
{
    /**
     * Tick Filter Example
    */
    public class TickDataFilteringAlgorithm : QCAlgorithm
    {
        /**
         * Initialize the tick filtering example algorithm
        */
        public @Override void Initialize() {
            SetCash(25000);
            SetStartDate(2013, 10, 07);
            SetEndDate(2013, 10, 11);
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Tick);

            //Add our custom data filter.
            Securities["SPY"].DataFilter = new ExchangeDataFilter(this);
        }

        /**
         * Data arriving here will now be filtered.
        */
         * @param data Ticks data array
        public void OnData(Ticks data) {
            if( !data.ContainsKey( "SPY")) return;
            spyTickList = data["SPY"];

            //Ticks return a list of ticks this second
            foreach (tick in spyTickList) {
                Log(tick.Exchange);
            }

            if( !Portfolio.Invested) {
                SetHoldings( "SPY", 1);
            }
        }
    }
    /**
     * Exchange filter class 
    */
    public class ExchangeDataFilter : ISecurityDataFilter
    {
        private IAlgorithm _algo;

        /**
         * Save instance of the algorithm namespace
        */
         * @param algo">
        public ExchangeDataFilter(IAlgorithm algo) {
            _algo = algo;
        }

        /**
         * Global Market Short Codes and their full versions: (used in tick objects)
         * https://github.com/QuantConnect/QCAlgorithm/blob/master/QuantConnect.Common/Global.cs
        */
        public static class MarketCodesFilter
        {
             * US Market Codes
            public static Map<String,String> US = new Map<String,String>() {
                {"A", "American Stock Exchange"},
                {"B", "Boston Stock Exchange"},
                {"C", "National Stock Exchange"},
                {"D", "FINRA ADF"},
                {"I", "International Securities Exchange"},
                {"J", "Direct Edge A"},
                {"K", "Direct Edge X"},
                {"M", "Chicago Stock Exchange"},
                {"N", "New York Stock Exchange"},
                {"P", "Nyse Arca Exchange"},
                {"Q", "NASDAQ OMX"},
                {"T", "NASDAQ OMX"},
                {"U", "OTC Bulletin Board"},
                {"u", "Over-the-Counter trade in Non-NASDAQ issue"},
                {"W", "Chicago Board Options Exchange"},
                {"X", "Philadelphia Stock Exchange"},
                {"Y", "BATS Y-Exchange, Inc"},
                {"Z", "BATS Exchange, Inc"}
            };

             * Canada Market Short Codes:
            public static Map<String,String> Canada = new Map<String,String>() {
                {"T", "Toronto"},
                {"V", "Venture"}
            };

            /**
             * Select allowed exchanges for this filter: e.g. top 4
            */
            public static List<String> AllowedExchanges = new List<String>() { 
                "P",    //NYSE ARCA - SPY PRIMARY EXCHANGE
                        //https://www.google.com/finance?q=NYSEARCA%3ASPY&ei=XcA2VKCSLs228waMhYCIBg
            };
        }


        /**
         * Filter out a tick from this vehicle, with this new data:
        */
         * @param data New data packet:
         * @param asset Vehicle of this filter.
        public boolean Filter(Security asset, BaseData data) {
            // TRUE -->  Accept Tick
            // FALSE --> Reject Tick
            tick = data as Tick;

            // This is a tick bar
            if( tick != null ) {
                if( tick.Exchange == "P") //MarketCodesFilter.AllowedExchanges.Contains() {
                    return true;
                }
            }

            //Only allow those exchanges through.
            return false;
        }

    }
}