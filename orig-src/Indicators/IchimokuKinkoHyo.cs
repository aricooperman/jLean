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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators
{
    /**
     * This indicator computes the Ichimoku Kinko Hyo indicator. It consists of the following main indicators:
     * Tenkan-sen: (Highest High + Lowest Low) / 2 for the specific period (normally 9)
     * Kijun-sen: (Highest High + Lowest Low) / 2 for the specific period (normally 26)
     * Senkou A Span: (Tenkan-sen + Kijun-sen )/ 2 from a specific number of periods ago (normally 26)
     * Senkou B Span: (Highest High + Lowest Low) / 2 for the specific period (normally 52), from a specific number of periods ago (normally 26)
    */
    public class IchimokuKinkoHyo : TradeBarIndicator
    {
        /**
         * The Tenkan-sen component of the Ichimoku indicator
        */
        public IndicatorBase<TradeBar> Tenkan { get; private set; }

        /**
         * The Kijun-sen component of the Ichimoku indicator
        */
        public IndicatorBase<TradeBar> Kijun { get; private set; }

        /**
         * The Senkou A Span component of the Ichimoku indicator
        */
        public IndicatorBase<TradeBar> SenkouA { get; private set; }

        /**
         * The Senkou B Span component of the Ichimoku indicator
        */
        public IndicatorBase<TradeBar> SenkouB { get; private set; }

        /**
         * The Tenkan-sen Maximum component of the Ichimoku indicator
        */
        public IndicatorBase<IndicatorDataPoint> TenkanMaximum { get; private set; }
        /**
         * The Tenkan-sen Minimum component of the Ichimoku indicator
        */
        public IndicatorBase<IndicatorDataPoint> TenkanMinimum { get; private set; }
        /**
         * The Kijun-sen Maximum component of the Ichimoku indicator
        */
        public IndicatorBase<IndicatorDataPoint> KijunMaximum { get; private set; }
        /**
         * The Kijun-sen Minimum component of the Ichimoku indicator
        */
        public IndicatorBase<IndicatorDataPoint> KijunMinimum { get; private set; }
        /**
         * The Senkou B Maximum component of the Ichimoku indicator
        */
        public IndicatorBase<IndicatorDataPoint> SenkouBMaximum { get; private set; }
        /**
         * The Senkou B Minimum component of the Ichimoku indicator
        */
        public IndicatorBase<IndicatorDataPoint> SenkouBMinimum { get; private set; }
        /**
         * The Delayed Tenkan Senkou A component of the Ichimoku indicator
        */
        public WindowIndicator<IndicatorDataPoint> DelayedTenkanSenkouA { get; private set; }
        /**
         * The Delayed Kijun Senkou A component of the Ichimoku indicator
        */
        public WindowIndicator<IndicatorDataPoint> DelayedKijunSenkouA { get; private set; }
        /**
         * The Delayed Maximum Senkou B component of the Ichimoku indicator
        */
        public WindowIndicator<IndicatorDataPoint> DelayedMaximumSenkouB { get; private set; }
        /**
         * The Delayed Minimum Senkou B component of the Ichimoku indicator
        */
        public WindowIndicator<IndicatorDataPoint> DelayedMinimumSenkouB { get; private set; }

        /**
         * Creates a new IchimokuKinkoHyo indicator from the specific periods
        */
         * @param name The name of this indicator
         * @param tenkanPeriod The Tenkan-sen period
         * @param kijunPeriod The Kijun-sen period
         * @param senkouAPeriod The Senkou A Span period
         * @param senkouBPeriod The Senkou B Span period
         * @param senkouADelayPeriod The Senkou A Span delay
         * @param senkouBDelayPeriod The Senkou B Span delay
        public IchimokuKinkoHyo( String name, int tenkanPeriod = 9, int kijunPeriod = 26, int senkouAPeriod = 26, int senkouBPeriod = 52, int senkouADelayPeriod = 26, int senkouBDelayPeriod = 26)
            : base(name) {
            TenkanMaximum = new Maximum(name + "_TenkanMax", tenkanPeriod);
            TenkanMinimum = new Minimum(name + "_TenkanMin", tenkanPeriod);
            KijunMaximum = new Maximum(name + "_KijunMax", kijunPeriod);
            KijunMinimum = new Minimum(name + "_KijunMin", kijunPeriod);
            SenkouBMaximum = new Maximum(name + "_SenkouBMaximum", senkouBPeriod);
            SenkouBMinimum = new Minimum(name + "_SenkouBMinimum", senkouBPeriod);
            DelayedTenkanSenkouA = new Delay(name + "DelayedTenkan", senkouADelayPeriod);
            DelayedKijunSenkouA = new Delay(name + "DelayedKijun", senkouADelayPeriod);
            DelayedMaximumSenkouB = new Delay(name + "DelayedMax", senkouBDelayPeriod);
            DelayedMinimumSenkouB = new Delay(name + "DelayedMin", senkouBDelayPeriod);


            SenkouA = new FunctionalIndicator<TradeBar>(
                name + "_SenkouA",
                input -> computeSenkouA(senkouAPeriod, input),
                senkouA -> DelayedTenkanSenkouA.IsReady && DelayedKijunSenkouA.IsReady,
                () =>
                {
                    Tenkan.Reset();
                    Kijun.Reset();
                });

            SenkouB = new FunctionalIndicator<TradeBar>(
                name + "_SenkouB",
                input -> computeSenkouB(senkouBPeriod, input),
                senkouA -> DelayedMaximumSenkouB.IsReady && DelayedMinimumSenkouB.IsReady,
                () =>
                {
                    Tenkan.Reset();
                    Kijun.Reset();
                });


            Tenkan = new FunctionalIndicator<TradeBar>(
                name + "_Tenkan",
                input -> ComputeTenkan(tenkanPeriod, input),
                tenkan -> TenkanMaximum.IsReady && TenkanMinimum.IsReady,
                () =>
                {
                    TenkanMaximum.Reset();
                    TenkanMinimum.Reset();
                });

            Kijun = new FunctionalIndicator<TradeBar>(
                name + "_Kijun",
                input -> ComputeKijun(kijunPeriod, input),
                kijun -> KijunMaximum.IsReady && KijunMinimum.IsReady,
                () =>
                {
                    KijunMaximum.Reset();
                    KijunMinimum.Reset();
                });
        }

        private BigDecimal computeSenkouB(int period, TradeBar input) {
            senkouB = DelayedMaximumSenkouB.Samples >= period ? (DelayedMaximumSenkouB + DelayedMinimumSenkouB) / 2 : new decimal(0.0);
            return senkouB;
        }

        private BigDecimal computeSenkouA(int period, TradeBar input) {
            senkouA = DelayedKijunSenkouA.Samples >= period ? (DelayedTenkanSenkouA + DelayedKijunSenkouA) / 2 : new decimal(0.0);
            return senkouA;
        }

        private BigDecimal ComputeTenkan(int period, TradeBar input) {
            tenkan = TenkanMaximum.Samples >= period ? (TenkanMaximum.Current.Value + TenkanMinimum.Current.Value) / 2 : new decimal(0.0);

            return tenkan;
        }

        private BigDecimal ComputeKijun(int period, TradeBar input) {
            kijun = KijunMaximum.Samples >= period ? (KijunMaximum + KijunMinimum) / 2 : new decimal(0.0);
            return kijun;
        }

        /**
         * Returns true if all of the sub-components of the Ichimoku indicator is ready
        */
        public @Override boolean IsReady
        {

            get { return Tenkan.IsReady && Kijun.IsReady && SenkouA.IsReady && SenkouB.IsReady; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {


            TenkanMaximum.Update(input.Time, input.High);
            TenkanMinimum.Update(input.Time, input.Low);
            Tenkan.Update(input);
            KijunMaximum.Update(input.Time, input.High);
            KijunMinimum.Update(input.Time, input.Low);
            Kijun.Update(input);
            DelayedTenkanSenkouA.Update(input.Time, Tenkan.Current.Value);
            DelayedKijunSenkouA.Update(input.Time, Kijun.Current.Value);
            SenkouA.Update(input);
            SenkouBMaximum.Update(input.Time, input.High);
            SenkouBMinimum.Update(input.Time, input.Low);
            DelayedMaximumSenkouB.Update(input.Time, SenkouBMaximum.Current.Value);
            DelayedMinimumSenkouB.Update(input.Time, SenkouBMinimum.Current.Value);
            SenkouB.Update(input);
            return input.Close;
        }
        
        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            base.Reset();
            TenkanMaximum.Reset();
            TenkanMinimum.Reset();
            Tenkan.Reset();
            KijunMaximum.Reset();
            KijunMinimum.Reset();
            Kijun.Reset();
            DelayedTenkanSenkouA.Reset();
            DelayedKijunSenkouA.Reset();
            SenkouA.Reset();
            SenkouBMaximum.Reset();
            SenkouBMinimum.Reset();
            DelayedMaximumSenkouB.Reset();
            DelayedMinimumSenkouB.Reset();
            SenkouB.Reset();
        }
    }
}
