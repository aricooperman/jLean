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
using QuantConnect.Data;

package com.quantconnect.lean.Indicators
{
    /**
    /// This indicator is capable of wiring up two separate indicators into a single indicator
    /// such that the output of each will be sent to a user specified function.
    */
    /// 
    /// This type is initialized such that there is no need to call the Update function. This indicator
    /// will have its values automatically updated each time a new piece of data is received from both
    /// the left and right indicators.
    /// 
    /// <typeparam name="T">The type of data input into this indicator</typeparam>
    public class CompositeIndicator<T> : IndicatorBase<T>
        where T : BaseData, new() {
        /**
        /// Delegate type used to compose the output of two indicators into a new value.
        */
        /// 
        /// A simple example would be to compute the difference between the two indicators (such as with MACD)
        /// (left, right) -> left - right
        /// 
         * @param left">The left indicator
         * @param right">The right indicator
        @returns And indicator result representing the composition of the two indicators
        public delegate IndicatorResult IndicatorComposer(IndicatorBase<T> left, IndicatorBase<T> right);

        /**function used to compose the individual indicators</summary>
        private final IndicatorComposer _composer;

        /**
        /// Gets the 'left' indicator for the delegate
        */
        public IndicatorBase<T> Left { get; private set; }

        /**
        /// Gets the 'right' indicator for the delegate
        */
        public IndicatorBase<T> Right { get; private set; }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Left.IsReady && Right.IsReady; }
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            Left.Reset();
            Right.Reset();
            base.Reset();
        }

        /**
        /// Creates a new CompositeIndicator capable of taking the output from the left and right indicators
        /// and producing a new value via the composer delegate specified
        */
         * @param name">The name of this indicator
         * @param left">The left indicator for the 'composer'
         * @param right">The right indidcator for the 'composoer'
         * @param composer">Function used to compose the left and right indicators
        public CompositeIndicator( String name, IndicatorBase<T> left, IndicatorBase<T> right, IndicatorComposer composer)
            : base(name) {
            _composer = composer;
            Left = left;
            Right = right;
            ConfigureEventHandlers();
        }

        /**
        /// Creates a new CompositeIndicator capable of taking the output from the left and right indicators
        /// and producing a new value via the composer delegate specified
        */
         * @param left">The left indicator for the 'composer'
         * @param right">The right indidcator for the 'composoer'
         * @param composer">Function used to compose the left and right indicators
        public CompositeIndicator(IndicatorBase<T> left, IndicatorBase<T> right, IndicatorComposer composer)
            : base( String.format( "COMPOSE(%1$s,%2$s)", left.Name, right.Name)) {
            _composer = composer;
            Left = left;
            Right = right;
            ConfigureEventHandlers();
        }

        /**
        /// Computes the next value of this indicator from the given state
        /// and returns an instance of the <see cref="IndicatorResult"/> class
        */
         * @param input">The input given to the indicator
        @returns An IndicatorResult object including the status of the indicator
        protected @Override IndicatorResult ValidateAndComputeNextValue(T input) {
            return _composer.Invoke(Left, Right);
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
        /// 
        /// Since this class @Overrides <see cref="ValidateAndComputeNextValue"/>, this method is a no-op
        /// 
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(T input) {
            // this should never actually be invoked
            return _composer.Invoke(Left, Right).Value;
        }

        /**
        /// Configures the event handlers for Left.Updated and Right.Updated to update this instance when
        /// they both have new data.
        */
        private void ConfigureEventHandlers() {
            // if either of these are constants then there's no reason
            boolean leftIsConstant = Left.GetType().IsSubclassOfGeneric(typeof (ConstantIndicator<>));
            boolean rightIsConstant = Right.GetType().IsSubclassOfGeneric(typeof (ConstantIndicator<>));

            // wire up the Updated events such that when we get a new piece of data from both left and right
            // we'll call update on this indicator. It's important to note that the CompositeIndicator only uses
            // the timestamp that gets passed into the Update function, his compuation is soley a function
            // of the left and right indicator via '_composer'

            IndicatorDataPoint newLeftData = null;
            IndicatorDataPoint newRightData = null;
            Left.Updated += (sender, updated) =>
            {
                newLeftData = updated;

                // if we have left and right data (or if right is a constant) then we need to update
                if( newRightData != null || rightIsConstant) {
                    Update(new T {Time = MaxTime(updated)});
                    // reset these to null after each update
                    newLeftData = null;
                    newRightData = null;
                }
            };

            Right.Updated += (sender, updated) =>
            {
                newRightData = updated;

                // if we have left and right data (or if left is a constant) then we need to update
                if( newLeftData != null || leftIsConstant) {
                    Update(new T {Time = MaxTime(updated)});
                    // reset these to null after each update
                    newLeftData = null;
                    newRightData = null;
                }
            };
        }

        private DateTime MaxTime(IndicatorDataPoint updated) {
            return new DateTime(Math.Max(updated.Time.Ticks, Math.Max(Right.Current.Time.Ticks, Left.Current.Time.Ticks)));
        }
    }
}