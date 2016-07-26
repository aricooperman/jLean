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
*/

using System;
using System.Collections.Concurrent;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Securities 
{
    /// <summary>
    /// Base class caching caching spot for security data and any other temporary properties.
    /// </summary>
    /// <remarks>
    /// This class is virtually unused and will soon be made obsolete. 
    /// This comment made in a remark to prevent obsolete errors in all users algorithms
    /// </remarks>
    public class SecurityCache
    {
        // this is used to prefer quote bar data over the tradebar data
        private DateTime _lastQuoteBarUpdate;
        private BaseData _lastData;
        private readonly ConcurrentMap<Type, BaseData> _dataByType = new ConcurrentMap<Type, BaseData>();

        /// <summary>
        /// Gets the most recent price submitted to this cache
        /// </summary>
        public BigDecimal Price { get; private set; }

        /// <summary>
        /// Gets the most recent open submitted to this cache
        /// </summary>
        public BigDecimal Open { get; private set; }

        /// <summary>
        /// Gets the most recent high submitted to this cache
        /// </summary>
        public BigDecimal High { get; private set; }

        /// <summary>
        /// Gets the most recent low submitted to this cache
        /// </summary>
        public BigDecimal Low { get; private set; }

        /// <summary>
        /// Gets the most recent close submitted to this cache
        /// </summary>
        public BigDecimal Close { get; private set; }

        /// <summary>
        /// Gets the most recent bid submitted to this cache
        /// </summary>
        public BigDecimal BidPrice { get; private set; }

        /// <summary>
        /// Gets the most recent ask submitted to this cache
        /// </summary>
        public BigDecimal AskPrice { get; private set; }

        /// <summary>
        /// Gets the most recent bid size submitted to this cache
        /// </summary>
        public long BidSize { get; private set; }

        /// <summary>
        /// Gets the most recent ask size submitted to this cache
        /// </summary>
        public long AskSize { get; private set; }

        /// <summary>
        /// Gets the most recent volume submitted to this cache
        /// </summary>
        public long Volume { get; private set; }

        /// <summary>
        /// Add a new market data point to the local security cache for the current market price.
        /// </summary>
        public void AddData(BaseData data)
        {
            _lastData = data;
            _dataByType[data.GetType()] = data;

            tick = data as Tick;
            if (tick != null)
            {
                if (tick.Value != 0) Price = tick.Value;

                if (tick.BidPrice != 0) BidPrice = tick.BidPrice;
                if (tick.BidSize != 0) BidSize = tick.BidSize;

                if (tick.AskPrice != 0) AskPrice = tick.AskPrice;
                if (tick.AskSize != 0) AskSize = tick.AskSize;
            }
            bar = data as IBar;
            if (bar != null)
            {
                if (_lastQuoteBarUpdate != data.EndTime)
                {
                    if (bar.Open != 0) Open = bar.Open;
                    if (bar.High != 0) High = bar.High;
                    if (bar.Low != 0) Low = bar.Low;
                    if (bar.Close != 0)
                    {
                        Price = bar.Close;
                        Close = bar.Close;
                    }
                }

                tradeBar = bar as TradeBar;
                if (tradeBar != null)
                {
                    if (tradeBar.Volume != 0) Volume = tradeBar.Volume;
                }
                quoteBar = bar as QuoteBar;
                if (quoteBar != null)
                {
                    _lastQuoteBarUpdate = quoteBar.EndTime;
                    if (quoteBar.Ask != null && quoteBar.Ask.Close != 0) AskPrice = quoteBar.Ask.Close;
                    if (quoteBar.Bid != null && quoteBar.Bid.Close != 0) BidPrice = quoteBar.Bid.Close;
                    if (quoteBar.LastBidSize != 0) BidSize = quoteBar.LastBidSize;
                    if (quoteBar.LastAskSize != 0) AskSize = quoteBar.LastAskSize;
                }
            }
            else
            {
                Price = data.Price;
            }
        }

        /// <summary>
        /// Get last data packet recieved for this security
        /// </summary>
        /// <returns>BaseData type of the security</returns>
        public BaseData GetData()
        {
            return _lastData;
        }

        /// <summary>
        /// Get last data packet recieved for this security of the specified ty[e
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <returns>The last data packet, null if none received of type</returns>
        public T GetData<T>()
            where T:BaseData
        {
            BaseData data;
            _dataByType.TryGetValue(typeof (T), out data);
            return data as T;
        }

        /// <summary>
        /// Reset cache storage and free memory
        /// </summary>
        public void Reset()
        {
            _dataByType.Clear();
        }
    }
}