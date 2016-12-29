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
using QuantConnect.Data.Market;

package com.quantconnect.lean.Data.Consolidators
{
    /**
     * This consolidator can transform a stream of <see cref="BaseData"/> instances into a stream of <see cref="RenkoBar"/>
    */
    public class RenkoConsolidator : DataConsolidator<IBaseData>
    {
        /**
         * Event handler that fires when a new piece of data is produced
        */
        public new EventHandler<RenkoBar> DataConsolidated; 
        
        private RenkoBar _currentBar;

        private final BigDecimal _barSize;
        private final boolean _evenBars;
        private final Func<IBaseData, decimal> _selector;
        private final Func<IBaseData, long> _volumeSelector;
        
        /**
         * Initializes a new instance of the <see cref="RenkoConsolidator"/> class using the specified <paramref name="barSize"/>.
         * The value selector will by default select <see cref="IBaseData.Value"/>
         * The volume selector will by default select zero.
        */
         * @param barSize The constant value size of each bar
         * @param evenBars When true bar open/close will be a multiple of the barSize
        public RenkoConsolidator( BigDecimal barSize, boolean evenBars = true) {
            _barSize = barSize;
            _selector = x -> x.Value;
            _volumeSelector = x -> 0;
            _evenBars = evenBars;
        }

        /**
         * Initializes a new instance of the <see cref="RenkoConsolidator" /> class.
        */
         * @param barSize The size of each bar in units of the value produced by <paramref name="selector"/>
         * @param selector Extracts the value from a data instance to be formed into a <see cref="RenkoBar"/>. The default
         * value is (x -> x.Value) the <see cref="IBaseData.Value"/> property on <see cref="IBaseData"/>
         * @param volumeSelector Extracts the volume from a data instance. The default value is null which does 
         * not aggregate volume per bar.
         * @param evenBars When true bar open/close will be a multiple of the barSize
        public RenkoConsolidator( BigDecimal barSize, Func<IBaseData, decimal> selector, Func<IBaseData, long> volumeSelector = null, boolean evenBars = true) {
            if( barSize < Extensions.GetDecimalEpsilon()) {
                throw new ArgumentOutOfRangeException( "barSize", "RenkoConsolidator bar size must be positve and greater than 1e-28");
            }

            _barSize = barSize;
            _evenBars = evenBars;
            _selector = selector ?? (x -> x.Value);
            _volumeSelector = volumeSelector ?? (x -> 0);
        }

        /**
         * Gets the bar size used by this consolidator
        */
        public BigDecimal BarSize
        {
            get { return _barSize; }
        }

        /**
         * Gets a clone of the data being currently consolidated
        */
        public @Override BaseData WorkingData
        {
            get { return _currentBar == null ? null : _currentBar.Clone(); }
        }

        /**
         * Gets <see cref="RenkoBar"/> which is the type emitted in the <see cref="IDataConsolidator.DataConsolidated"/> event.
        */
        public @Override Class OutputType
        {
            get { return typeof(RenkoBar); }
        }

        /**
         * Updates this consolidator with the specified data. This method is
         * responsible for raising the DataConsolidated event
        */
         * @param data The new data for the consolidator
        public @Override void Update(IBaseData data) {
            currentValue = _selector(data);
            volume = _volumeSelector(data);

            Optional<BigDecimal> close = null;
            
            // if we're already in a bar then update it
            if( _currentBar != null ) {
                _currentBar.Update(data.Time, currentValue, volume);

                // if the update caused this bar to close, fire the event and reset the bar
                if( _currentBar.IsClosed) {
                    close = _currentBar.Close;
                    OnDataConsolidated(_currentBar);
                    _currentBar = null;
                }
            }

            if( _currentBar == null ) {
                open = close ?? currentValue;
                if( _evenBars && !close.HasValue) {
                    open = Math.Ceiling(open/_barSize)*_barSize;
                }
                _currentBar = new RenkoBar(data.Symbol, data.Time, _barSize, open, volume);
            }
        }

        /**
         * Scans this consolidator to see if it should emit a bar due to time passing
        */
         * @param currentLocalTime The current time in the local time zone (same as <see cref="BaseData.Time"/>)
        public @Override void Scan(DateTime currentLocalTime) {
        }

        /**
         * Event invocator for the DataConsolidated event. This should be invoked
         * by derived classes when they have consolidated a new piece of data.
        */
         * @param consolidated The newly consolidated data
        protected void OnDataConsolidated(RenkoBar consolidated) {
            handler = DataConsolidated;
            if( handler != null ) handler(this, consolidated);

            base.OnDataConsolidated(consolidated);
        }
    }

    /**
     * Provides a type safe wrapper on the RenkoConsolidator class. This just allows us to define our selector functions with the real type they'll be receiving
    */
     * <typeparam name="TInput"></typeparam>
    public class RenkoConsolidator<TInput> : RenkoConsolidator
        where TInput : IBaseData
    {
        /**
         * Initializes a new instance of the <see cref="RenkoConsolidator" /> class.
        */
         * @param barSize The size of each bar in units of the value produced by <paramref name="selector"/>
         * @param selector Extracts the value from a data instance to be formed into a <see cref="RenkoBar"/>. The default
         * value is (x -> x.Value) the <see cref="IBaseData.Value"/> property on <see cref="IBaseData"/>
         * @param volumeSelector Extracts the volume from a data instance. The default value is null which does 
         * not aggregate volume per bar.
         * @param evenBars When true bar open/close will be a multiple of the barSize
        public RenkoConsolidator( BigDecimal barSize, Func<TInput, decimal> selector, Func<TInput, long> volumeSelector = null, boolean evenBars = true)
            : base(barSize, x -> selector((TInput)x), volumeSelector == null ? (Func<IBaseData, long>) null : x -> volumeSelector((TInput)x), evenBars) {
        }

        /**
         * Initializes a new instance of the <see cref="RenkoConsolidator"/> class using the specified <paramref name="barSize"/>.
         * The value selector will by default select <see cref="IBaseData.Value"/>
         * The volume selector will by default select zero.
        */
         * @param barSize The constant value size of each bar
         * @param evenBars When true bar open/close will be a multiple of the barSize
        public RenkoConsolidator( BigDecimal barSize, boolean evenBars = true)
            : base(barSize, evenBars) {
        }

        /**
         * Updates this consolidator with the specified data.
        */
         * 
         * Class safe shim method.
         * 
         * @param data The new data for the consolidator
        public void Update(TInput data) {
            base.Update(data);
        }
    }
}
