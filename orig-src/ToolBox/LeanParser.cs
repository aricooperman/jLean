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
using System.Collections.Generic;
using System.IO;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.ToolBox.AlgoSeekOptionsConverter;

package com.quantconnect.lean.ToolBox
{
    /**
     * Provides an implementation of <see cref="IStreamParser"/> that reads files in the lean format
    */
    public class LeanParser : IStreamParser
    {
        /**
         * Parses the specified input stream into an enumerable of data
        */
         * @param source The source file corresponding the the stream
         * @param stream The input stream to be parsed
        @returns An enumerable of base data
        public IEnumerable<BaseData> Parse( String source, Stream stream) {
            pathComponents = LeanDataPathComponents.Parse(source);
            tickType = pathComponents.Filename.toLowerCase().Contains( "_trade")
                ? TickType.Trade
                : TickType.Quote;

            dataType = GetDataType(pathComponents.SecurityType, pathComponents.Resolution, tickType);
            factory = (BaseData) Activator.CreateInstance(dataType);
            
            // ignore time zones here, i.e, we're going to emit data in the data time zone
            config = new SubscriptionDataConfig(dataType, pathComponents.Symbol, pathComponents.Resolution, Global.UTC_ZONE_ID, Global.UTC_ZONE_ID, false, true, false);
            using (reader = new StreamReader(stream)) {
                String line;
                while ((line = reader.ReadLine()) != null ) {
                    yield return factory.Reader(config, line, pathComponents.Date, false);
                }
            }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public void Dispose() {
        }

        private Class GetDataType(SecurityType securityType, Resolution resolution, TickType tickType) {
            if( resolution == Resolution.Tick) {
                return typeof (Tick);
            }

            switch (securityType) {
                case SecurityType.Base:
                case SecurityType.Equity:
                case SecurityType.Cfd:
                case SecurityType.Forex:
                    return typeof (TradeBar);

                case SecurityType.Option:
                    if( tickType == TickType.Trade) return typeof (TradeBar);
                    if( tickType == TickType.Quote) return typeof (QuoteBar);
                    break;
            }
            parameters = String.join( " | ", securityType, resolution, tickType);
            throw new UnsupportedOperationException( "LeanParser.GetDataType does has not yet implemented: " + parameters);
        }
    }
}