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
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.AlgoSeekOptionsConverter
{
    /**
     * Provides an implementation of <see cref="IStreamParser"/> that parses raw algo seek options data
    */
    public class AlgoSeekOptionsParser : IStreamParser
    {
        private static final int LogInterval = 1000000;

        static AlgoSeekOptionsParser() {
            Log.Error( "WARNING:: TEST MODE:: AWAITING FINAL FILE NAMING CONVENTION");
        }

        /**
         * Parses the specified input stream into an enumerable of data
        */
         * @param source The source of the stream
         * @param stream The input stream to be parsed
        @returns An enumerable of base data
        public IEnumerable<BaseData> Parse( String source, Stream stream) {
            count = 0L;
            referenceDate = DateTime.ParseExact(new FileInfo(source).Directory.Name, DateFormat.EightCharacter, null );
            
            using (reader = new StreamReader(stream)) {
                // skip the header row
                reader.ReadLine();

                String line;
                while ((line = reader.ReadLine()) != null ) {
                    count++;

                    if( count%LogInterval == 0) {
                        Log.Trace( "AlgoSeekOptionsParser.Parse(%1$s): Parsed {1,3}M lines.", source, count/LogInterval);
                    }

                    Tick tick;
                    try
                    {
                        // filter out bad lines as fast as possible
                        EventType eventType;
                        switch (line[13]) {
                            case 'T':
                                eventType = EventType.Trade;
                                break;
                            case 'F':
                                switch (line[15]) {
                                    case 'B':
                                        eventType = EventType.Bid;
                                        break;
                                    case 'O':
                                        eventType = EventType.Ask;
                                        break;
                                    default:
                                        continue;
                                }
                                break;
                            default:
                                continue;
                        }
                        
                        // parse csv check column count
                        static final int columns = 11;
                        csv = line Extensions.toCsv( columns);
                        if( csv.Count < columns) continue;

                        // ignoring time zones completely -- this is all in the 'data-time-zone'
                        timeString = csv[0];
                        hours = timeString.Substring(0, 2) Integer.parseInt(  );
                        minutes = timeString.Substring(3, 2) Integer.parseInt(  );
                        seconds = timeString.Substring(6, 2) Integer.parseInt(  );
                        millis = timeString.Substring(9, 3) Integer.parseInt(  );
                        time = referenceDate.Add(new TimeSpan(0, hours, minutes, seconds, millis));

                        // detail: PUT at 30.0000 on 2014-01-18
                        underlying = csv[4];

                        //FOR WINDOWS TESTING
                        //if( underlying.Equals( "AUX", StringComparison.OrdinalIgnoreCase)
                         //|| underlying.Equals( "CON", StringComparison.OrdinalIgnoreCase)
                         //|| underlying.Equals( "PRN", StringComparison.OrdinalIgnoreCase))
                        //{
                            //continue;
                        //}

                        optionRight = csv[5][0] == 'P' ? OptionRight.Put : OptionRight.Call;
                        expiry = DateTime.ParseExact(csv[6], "yyyyMMdd", null );
                        strike = csv[7] new BigDecimal(  )/10000m;
                        optionStyle = OptionStyle.American; // couldn't see this specified in the file, maybe need a reference file
                        sid = SecurityIdentifier.GenerateOption(expiry, underlying, Market.USA, strike, optionRight, optionStyle);
                        symbol = new Symbol(sid, underlying);
                        
                        price = csv[9] new BigDecimal(  ) / 10000m;
                        quantity = csv[8] Integer.parseInt(  );

                        tick = new Tick
                        {
                            Symbol = symbol,
                            Time = time,
                            TickType = eventType.TickType,
                            Exchange = csv[10],
                            Value = price
                        };
                        if( eventType.TickType == TickType.Quote) {
                            if( eventType.IsAsk) {
                                tick.AskPrice = price;
                                tick.AskSize = quantity;
                            }
                            else
                            {
                                tick.BidPrice = price;
                                tick.BidSize = quantity;
                            }
                        }
                        else
                        {
                            tick.Quantity = quantity;
                        }
                    }
                    catch (Exception err) {
                        Log.Error(err);
                        continue;
                    }

                    yield return tick;
                }
            }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public void Dispose() {
        }

        /**
         * Specifies the event types to be parsed from the raw data, all other data is ignored
        */
        class EventType
        {
            public static final EventType Trade = new EventType(false, TickType.Trade);
            public static final EventType Bid = new EventType(false, TickType.Quote);
            public static final EventType Ask = new EventType(true, TickType.Quote);
            public final boolean IsAsk;
            public final TickType TickType;
            private EventType( boolean isAsk, TickType tickType) {
                IsAsk = isAsk;
                TickType = tickType;
            }
        }
    }
}