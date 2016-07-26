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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Util;

package com.quantconnect.lean.Orders
{
    /// <summary>
    /// Provides an implementation of <see cref="JsonConverter"/> that can deserialize Orders
    /// </summary>
    public class OrderJsonConverter : JsonConverter
    {
        private static readonly Lazy<IMapFileProvider> MapFileProvider = new Lazy<IMapFileProvider>(() =>
            Composer.Instance.GetExportedValueByTypeName<IMapFileProvider>(Config.Get("map-file-provider", "LocalDiskMapFileProvider"))
            );

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter"/> can write JSON.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter"/> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override boolean CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override boolean CanConvert(Type objectType)
        {
            return typeof(Order).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("The OrderJsonConverter does not implement a WriteJson method;.");
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            jObject = JObject.Load(reader);

            order = CreateOrderFromJObject(jObject);

            return order;
        }

        /// <summary>
        /// Create an order from a simple JObject
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns>Order Object</returns>
        public static Order CreateOrderFromJObject(JObject jObject)
        {
            // create order instance based on order type field
            orderType = (OrderType) jObject["Type"].Value<Integer>();
            order = CreateOrder(orderType, jObject);

            // populate common order properties
            order.Id = jObject["Id"].Value<Integer>();
            order.Quantity = jObject["Quantity"].Value<Integer>();
            order.Status = (OrderStatus) jObject["Status"].Value<Integer>();
            order.Time = jObject["Time"].Value<DateTime>();
            order.Tag = jObject["Tag"].Value<String>();
            order.Quantity = jObject["Quantity"].Value<Integer>();
            order.Price = jObject["Price"].Value<decimal>();
            securityType = (SecurityType) jObject["SecurityType"].Value<Integer>();
            order.BrokerId = jObject["BrokerId"].Select(x => x.Value<String>()).ToList();
            order.ContingentId = jObject["ContingentId"].Value<Integer>();

            market = Market.USA;
            if (securityType == SecurityType.Forex) market = Market.FXCM;

            if (jObject.SelectTokens("Symbol.ID").Any())
            {
                sid = SecurityIdentifier.Parse(jObject.SelectTokens("Symbol.ID").Single().Value<String>());
                ticker = jObject.SelectTokens("Symbol.Value").Single().Value<String>();
                order.Symbol = new Symbol(sid, ticker);
            }
            else if (jObject.SelectTokens("Symbol.Value").Any())
            {
                // provide for backwards compatibility
                ticker = jObject.SelectTokens("Symbol.Value").Single().Value<String>();
                order.Symbol = Symbol.Create(ticker, securityType, market);
            }
            else
            {
                tickerstring = jObject["Symbol"].Value<String>();
                order.Symbol = Symbol.Create(tickerstring, securityType, market);
            }
            return order;
        }
        
        /// <summary>
        /// Creates an order of the correct type
        /// </summary>
        private static Order CreateOrder(OrderType orderType, JObject jObject)
        {
            Order order;
            switch (orderType)
            {
                case OrderType.Market:
                    order = new MarketOrder();
                    break;

                case OrderType.Limit:
                    order = new LimitOrder {LimitPrice = jObject["LimitPrice"].Value<decimal>()};
                    break;

                case OrderType.StopMarket:
                    order = new StopMarketOrder
                    {
                        StopPrice = jObject["StopPrice"].Value<decimal>()
                    };
                    break;

                case OrderType.StopLimit:
                    order = new StopLimitOrder
                    {
                        LimitPrice = jObject["LimitPrice"].Value<decimal>(),
                        StopPrice = jObject["StopPrice"].Value<decimal>()
                    };
                    break;

                case OrderType.MarketOnOpen:
                    order = new MarketOnOpenOrder();
                    break;

                case OrderType.MarketOnClose:
                    order = new MarketOnCloseOrder();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return order;
        }
    }
}
