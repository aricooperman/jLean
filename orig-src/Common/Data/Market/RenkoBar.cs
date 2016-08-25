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

package com.quantconnect.lean.Data.Market
{
    /**
     * Represents a bar sectioned not by time, but by some amount of movement in a value (for example, Closing price moving in $10 bar sizes)
    */
    public class RenkoBar : BaseData
    {
        /**
         * Gets the height of the bar
        */
        public BigDecimal BrickSize { get; private set; }

        /**
         * Gets the opening value that started this bar.
        */
        public BigDecimal Open { get; private set; }

        /**
         * Gets the closing value or the current value if the bar has not yet closed.
        */
        public BigDecimal Close
        {
            get { return Value; }
            private set { Value = value; }
        }

        /**
         * Gets the highest value encountered during this bar
        */
        public BigDecimal High { get; private set; }

        /**
         * Gets the lowest value encountered during this bar
        */
        public BigDecimal Low { get; private set; }

        /**
         * Gets the volume of trades during the bar.
        */
        public long Volume { get; private set; }

        /**
         * Gets the end time of this renko bar or the most recent update time if it <see cref="IsClosed"/>
        */
        public @Override DateTime EndTime { get; set; }

        /**
         * Gets the end time of this renko bar or the most recent update time if it <see cref="IsClosed"/>
        */
        [Obsolete( "RenkoBar.End is obsolete. Please use RenkoBar.EndTime property instead.")]
        public DateTime End
        {
            get { return EndTime; }
            set { EndTime = value; }
        }

        /**
         * Gets the time this bar started
        */
        public DateTime Start
        {
            get { return Time; }
            private set { Time = value; }
        }

        /**
         * Gets whether or not this bar is considered closed.
        */
        public boolean IsClosed { get; private set; }

        /**
         * Initializes a new default instance of the <see cref="RenkoBar"/> class.
        */
        public RenkoBar() {
        }

        /**
         * Initializes a new instance of the <see cref="RenkoBar"/> class with the specified values
        */
         * @param symbol The symbol of this data
         * @param time The start time of the bar
         * @param brickSize The size of each renko brick
         * @param open The opening price for the new bar
         * @param volume Any initial volume associated with the data
        public RenkoBar(Symbol symbol, DateTime time, BigDecimal brickSize, BigDecimal open, long volume) {
            Symbol = symbol;
            Start = time;
            EndTime = time;
            BrickSize = brickSize;
            Open = open;
            Close = open;
            Volume = volume;
            High = open;
            Low = open;
        }

        /**
         * Updates this <see cref="RenkoBar"/> with the specified values and returns whether or not this bar is closed
        */
         * @param time The current time
         * @param currentValue The current value
         * @param volumeSinceLastUpdate The volume since the last update called on this instance
        @returns True if this bar <see cref="IsClosed"/>
        public boolean Update(DateTime time, BigDecimal currentValue, long volumeSinceLastUpdate) {
            // can't update a closed renko bar
            if( IsClosed) return true;
            if( Start == DateTime.MinValue) Start = time;
            EndTime = time;

            // compute the min/max closes this renko bar can have
            BigDecimal lowClose = Open - BrickSize;
            BigDecimal highClose = Open + BrickSize;

            Close = Math.Min(highClose, Math.Max(lowClose, currentValue));
            Volume += volumeSinceLastUpdate;

            // determine if this data caused the bar to close
            if( currentValue <= lowClose  || currentValue >= highClose) {
                IsClosed = true;
            }

            if( Close > High) High = Close;
            if( Close < Low) Low = Close;

            return IsClosed;
        }

        /**
         * Reader Method :: using set of arguements we specify read out type. Enumerate
         * until the end of the data stream or file. E.g. Read CSV file line by line and convert
         * into data types.
        */
        @returns BaseData type set by Subscription Method.
         * @param config Config.
         * @param line Line.
         * @param date Date.
         * @param isLiveMode true if we're in live mode, false for backtesting mode
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            throw new NotSupportedException( "RenkoBar does not support the Reader function. This function should never be called on this type.");
        }

        /**
         * Return the URL String source of the file. This will be converted to a stream
        */
         * @param config Configuration object
         * @param date Date of this source file
         * @param isLiveMode true if we're in live mode, false for backtesting mode
        @returns String URL of source file.
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            throw new NotSupportedException( "RenkoBar does not support the GetSource function. This function should never be called on this type.");
        }

        /**
         * Return a new instance clone of this object, used in fill forward
        */
         * 
         * This base implementation uses reflection to copy all public fields and properties
         * 
        @returns A clone of the current object
        public @Override BaseData Clone() {
            return new RenkoBar
            {
                BrickSize = BrickSize,
                Open = Open,
                Volume = Volume,
                Close = Close,
                EndTime = EndTime,
                High = High,
                IsClosed = IsClosed,
                Low = Low,
                Time = Time,
                Value = Value,
                Symbol = Symbol,
                DataType = DataType
            };
        }
    }
}