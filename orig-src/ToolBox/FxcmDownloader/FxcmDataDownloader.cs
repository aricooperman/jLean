﻿/*
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
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.fxcm.external.api.transport;
using com.fxcm.external.api.transport.listeners;
using com.fxcm.fix;
using com.fxcm.fix.pretrade;
using com.fxcm.messaging;
using java.util;
using QuantConnect.Brokerages.Fxcm;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using TimeZone = java.util.TimeZone;

package com.quantconnect.lean.ToolBox.FxcmDownloader
{
    /**
    /// FXCM Data Downloader class
    */
    public class FxcmDataDownloader : IDataDownloader, IGenericMessageListener, IStatusMessageListener
    {
        private final FxcmSymbolMapper _symbolMapper = new FxcmSymbolMapper();
        private final String _server;
        private final String _terminal;
        private final String _userName;
        private final String _password;

        
        private IGateway _gateway;
        private final object _locker = new object();
        private String _currentRequest;
        private static final int ResponseTimeout = 2500;
        private final Map<String, AutoResetEvent> _mapRequestsToAutoResetEvents = new Map<String, AutoResetEvent>();
        private final Map<String, TradingSecurity> _fxcmInstruments = new Map<String, TradingSecurity>();
        private final IList<BaseData> _currentBaseData = new List<BaseData>();

        /**
        /// Initializes a new instance of the <see cref="FxcmDataDownloader"/> class
        */
        public FxcmDataDownloader( String server, String terminal, String userName, String password) {
            _server = server;
            _terminal = terminal;
            _userName = userName;
            _password = password;
        }

        /**
        /// Converts a Java Date value to a UTC DateTime value
        */
         * @param javaDate">The Java date
        @returns A UTC DateTime value
        private static DateTime FromJavaDateUtc(Date javaDate) {
            cal = Calendar.getInstance();
            cal.setTimeZone(TimeZone.getTimeZone( "UTC"));
            cal.setTime(javaDate);

            // note that the Month component of java.util.Date  
            // from 0-11 (i.e. Jan == 0)
            return new DateTime(cal.get(Calendar.YEAR),
                                cal.get(Calendar.MONTH) + 1,
                                cal.get(Calendar.DAY_OF_MONTH),
                                cal.get(Calendar.HOUR_OF_DAY),
                                cal.get(Calendar.MINUTE),
                                cal.get(Calendar.SECOND),
                                cal.get(Calendar.MILLISECOND));
        }

        /**
        /// Converts a Java Date value to a UTC DateTime value
        */
         * @param utcDateTime">The UTC DateTime value
        @returns A UTC Java Date value
        private static Date ToJavaDateUtc(DateTime utcDateTime) {
            cal = Calendar.getInstance();
            cal.setTimeZone(TimeZone.getTimeZone( "UTC"));

            cal.set(Calendar.YEAR, utcDateTime.Year);
            cal.set(Calendar.MONTH, utcDateTime.Month - 1);
            cal.set(Calendar.DAY_OF_MONTH, utcDateTime.Day);
            cal.set(Calendar.HOUR_OF_DAY, utcDateTime.Hour);
            cal.set(Calendar.MINUTE, utcDateTime.Minute);
            cal.set(Calendar.SECOND, utcDateTime.Second);
            cal.set(Calendar.MILLISECOND, utcDateTime.Millisecond);

            return cal.getTime();
        }

        /**
        /// Checks if downloader can get the data for the Lean symbol
        */
         * @param symbol">The Lean symbol
        @returns Returns true if the symbol is available
        public boolean HasSymbol( String symbol) {
            return _symbolMapper.IsKnownLeanSymbol(Symbol.Create(symbol, GetSecurityType(symbol), Market.FXCM));
        }

        /**
        /// Gets the security type for the specified Lean symbol
        */
         * @param symbol">The Lean symbol
        @returns The security type
        public SecurityType GetSecurityType( String symbol) {
            return _symbolMapper.GetLeanSecurityType(symbol);
        }

        /**
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        */
         * @param symbol">Symbol for the data we're looking for.
         * @param resolution">Resolution of the data request
         * @param startUtc">Start time of the data in UTC
         * @param endUtc">End time of the data in UTC
        @returns Enumerable of base data for this symbol
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc) {
            if( !_symbolMapper.IsKnownLeanSymbol(symbol))
                throw new ArgumentException( "Invalid symbol requested: " + symbol.Value);

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new NotSupportedException( "SecurityType not available: " + symbol.ID.SecurityType);

            if( endUtc <= startUtc)
                throw new ArgumentException( "The end date must be greater than the start date.");

            Console.WriteLine( "Logging in...");

            // create the gateway
            _gateway = GatewayFactory.createGateway();

            // register the message listeners with the gateway
            _gateway.registerGenericMessageListener(this);
            _gateway.registerStatusMessageListener(this);

            // create local login properties
            loginProperties = new FXCMLoginProperties(_userName, _password, _terminal, _server);

            // log in
            _gateway.login(loginProperties);

            // initialize session
            RequestTradingSessionStatus();

            Console.WriteLine( "Downloading %1$s data from %2$s to %3$s...", resolution, startUtc.toString( "yyyyMMdd HH:mm:ss"), endUtc.toString( "yyyyMMdd HH:mm:ss"));

            //Find best FXCM  paramrs
            IFXCMTimingInterval interval = ToFXCMInterval(resolution);

            totalTicks = (endUtc - startUtc).Ticks;

            // download data
            totalBaseData = new List<BaseData>();

            end = endUtc;

            do // 
            {
                //show progress
                progressBar(Math.Abs((end - endUtc).Ticks), totalTicks, Console.WindowWidth / 2,'█');
                _currentBaseData.Clear();

                mdr = new MarketDataRequest();
                mdr.setSubscriptionRequestType(SubscriptionRequestTypeFactory.SNAPSHOT);
                mdr.setResponseFormat(IFixMsgTypeDefs.__Fields.MSGTYPE_FXCMRESPONSE);
                mdr.setFXCMTimingInterval(interval);
                mdr.setMDEntryTypeSet(MarketDataRequest.MDENTRYTYPESET_ALL);

                mdr.setFXCMStartDate(new UTCDate(ToJavaDateUtc(startUtc)));
                mdr.setFXCMStartTime(new UTCTimeOnly(ToJavaDateUtc(startUtc)));
                mdr.setFXCMEndDate(new UTCDate(ToJavaDateUtc(end)));
                mdr.setFXCMEndTime(new UTCTimeOnly(ToJavaDateUtc(end)));
                mdr.addRelatedSymbol(_fxcmInstruments[_symbolMapper.GetBrokerageSymbol(symbol)]);


                AutoResetEvent autoResetEvent;
                lock (_locker) {
                    _currentRequest = _gateway.sendMessage(mdr);
                    autoResetEvent = new AutoResetEvent(false);
                    _mapRequestsToAutoResetEvents[_currentRequest] = autoResetEvent;
                }
                if( !autoResetEvent.WaitOne(1000 * 5)) {
                    // no response, exit
                    break;
                }

                // Add data
                totalBaseData.InsertRange(0, _currentBaseData.Where(x -> x.Time.Date >= startUtc.Date));
                
                if( end != _currentBaseData[0].Time) {
                    // new end date = first datapoint date.
                    end = _currentBaseData[0].Time;
                }
                else
                {
                    break;
                }
               
              

            } while (end > startUtc);


            Console.WriteLine( "\nLogging out...");

            // log out
            _gateway.logout();

            // remove the message listeners
            _gateway.removeGenericMessageListener(this);
            _gateway.removeStatusMessageListener(this);

            return totalBaseData.ToList();

        }

        private IFXCMTimingInterval ToFXCMInterval(Resolution resolution) {
            IFXCMTimingInterval interval = null;
            
            switch (resolution) {
                case Resolution.Tick:
                    interval = FXCMTimingIntervalFactory.TICK;
                 
                    break;
                case Resolution.Second:
                    interval = FXCMTimingIntervalFactory.SEC10;
                   
                    break;
                case Resolution.Minute:
                    interval = FXCMTimingIntervalFactory.MIN1;
                    
                    break;
                case Resolution.Hour:
                    interval = FXCMTimingIntervalFactory.HOUR1;
                    
                    break;
                case Resolution.Daily:
                    interval = FXCMTimingIntervalFactory.DAY1;
                  
                    break;
            }

            return interval;
        }

        private void RequestTradingSessionStatus() {
            // Note: requestTradingSessionStatus() MUST be called just after login

            AutoResetEvent autoResetEvent;
            lock (_locker) {
                _currentRequest = _gateway.requestTradingSessionStatus();
                autoResetEvent = new AutoResetEvent(false);
                _mapRequestsToAutoResetEvents[_currentRequest] = autoResetEvent;
            }
            if( !autoResetEvent.WaitOne(ResponseTimeout))
                throw new TimeoutException( String.format( "FxcmBrokerage.LoadInstruments(): Operation took longer than %1$s seconds.", (decimal)ResponseTimeout / 1000));
        }

        #region IGenericMessageListener implementation

        /**
        /// Receives generic messages from the FXCM API
        */
         * @param message">Generic message received
        public void messageArrived(ITransportable message) {
            // Dispatch message to specific handler
            lock (_locker) {
                if( message is TradingSessionStatus)
                    OnTradingSessionStatus((TradingSessionStatus)message);

                else if( message is MarketDataSnapshot)
                    OnMarketDataSnapshot((MarketDataSnapshot)message);
            }
        }

        /**
        /// TradingSessionStatus message handler
        */
        private void OnTradingSessionStatus(TradingSessionStatus message) {
            if( message.getRequestID() == _currentRequest) {
                // load instrument list into a dictionary
                securities = message.getSecurities();
                while (securities.hasMoreElements()) {
                    security = (TradingSecurity)securities.nextElement();
                    _fxcmInstruments[security.getSymbol()] = security;
                }

                _mapRequestsToAutoResetEvents[_currentRequest].Set();
                _mapRequestsToAutoResetEvents.Remove(_currentRequest);
            }
        }

        /**
        /// MarketDataSnapshot message handler
        */
        private void OnMarketDataSnapshot(MarketDataSnapshot message) {
            if( message.getRequestID() == _currentRequest) {
                securityType = _symbolMapper.GetBrokerageSecurityType(message.getInstrument().getSymbol());
                symbol = _symbolMapper.GetLeanSymbol(message.getInstrument().getSymbol(), securityType, Market.FXCM);
                time = FromJavaDateUtc(message.getDate().toDate());


                if( message.getFXCMTimingInterval() == FXCMTimingIntervalFactory.TICK) {
                    bid = new BigDecimal( message.getBidClose());
                    ask = new BigDecimal( message.getAskClose());

                    tick = new Tick(time, symbol, bid, ask);

                    //Add tick
                    _currentBaseData.Add(tick);

                }
                else // it bars
                {
                    open = new BigDecimal( (message.getBidOpen() + message.getAskOpen()) / 2);
                    high = new BigDecimal( (message.getBidHigh() + message.getAskHigh()) / 2);
                    low = new BigDecimal( (message.getBidLow() + message.getAskLow()) / 2);
                    close = new BigDecimal( (message.getBidClose() + message.getAskClose()) / 2);

                    bar = new TradeBar(time, symbol, open, high, low, close, 0);

                    // add bar to list
                    _currentBaseData.Add(bar);
                }

                if( message.getFXCMContinuousFlag() == IFixValueDefs.__Fields.FXCMCONTINUOUS_END) {
                    _mapRequestsToAutoResetEvents[_currentRequest].Set();
                    _mapRequestsToAutoResetEvents.Remove(_currentRequest);
                }
            }
        }

        #endregion


        #region IStatusMessageListener implementation

        /**
        /// Receives status messages from the FXCM API
        */
         * @param message">Status message received
        public void messageArrived(ISessionStatus message) {
        }

        #endregion




        /**
        /// Aggregates a list of ticks at the requested resolution
        */
         * @param symbol">
         * @param ticks">
         * @param resolution">
        @returns 
        internal static IEnumerable<TradeBar> AggregateTicks(Symbol symbol, IEnumerable<Tick> ticks, Duration resolution) {
            return
                (from t in ticks
                 group t by t.Time.RoundDown(resolution)
                     into g
                 select new TradeBar
                 {
                     Symbol = symbol,
                     Time = g.Key,
                     Open = g.First().LastPrice,
                     High = g.Max(t -> t.LastPrice),
                     Low = g.Min(t -> t.LastPrice),
                     Close = g.Last().LastPrice
                 });
        }


        #region Console Helper

        /**
        /// Draw a progress bar 
        */
         * @param complete">
         * @param maxVal">
         * @param barSize">
         * @param progressCharacter">
        private static void progressBar(long complete, long maxVal, long barSize, char progressCharacter) {
          
            BigDecimal p   = (decimal)complete / (decimal)maxVal;
            int chars   = (int)Math.Floor(p / ((decimal)1 / (decimal)barSize));
            String bar = string.Empty;
            bar = bar.PadLeft(chars, progressCharacter);
            bar = bar.PadRight( Integer.parseInt( barSize)-1);
            
            Console.Write( String.format( "\r[%1$s] %2$s%", bar, (p * 100).toString( "N2")));           
        }

        #endregion

    }
}
