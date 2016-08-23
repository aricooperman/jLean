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

using QuantConnect.Brokerages;
using QuantConnect.Data.Market;
using QuantConnect.Securities;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
    /// This algorithm shows how to set a custom security initializer.
    /// A security initializer is run immediately after a new security object
    /// has been created and can be used to security models and other settings,
    /// such as data normalization mode
    */
    public class CustomSecurityInitializerAlgorithm : QCAlgorithm
    {
        public @Override void Initialize() {
            // set our initializer to our custom type
            SetBrokerageModel(BrokerageName.TradierBrokerage);
            SetSecurityInitializer(new CustomSecurityInitializer(BrokerageModel, DataNormalizationMode.Raw));

            SetStartDate(2012, 01, 01);
            SetEndDate(2013, 01, 01);

            AddSecurity(SecurityType.Equity, "SPY", Resolution.Hour);
        }

        public void OnData(TradeBars data) {
            if( !Portfolio.Invested) {
                SetHoldings( "SPY", 1);
            }
        }

        /**
        /// Our custom initializer that will set the data normalization mode.
        /// We sub-class the <see cref="BrokerageModelSecurityInitializer"/>
        /// so we can also take advantage of the default model/leverage setting
        /// behaviors
        */
        class CustomSecurityInitializer : BrokerageModelSecurityInitializer
        {
            private final DataNormalizationMode _dataNormalizationMode;

            /**
            /// Initializes a new instance of the <see cref="CustomSecurityInitializer"/> class
            /// with the specified normalization mode
            */
             * @param brokerageModel">The brokerage model used to get fill/fee/slippage/settlement models
             * @param dataNormalizationMode">The desired data normalization mode
            public CustomSecurityInitializer(IBrokerageModel brokerageModel, DataNormalizationMode dataNormalizationMode)
                : base(brokerageModel) {
                _dataNormalizationMode = dataNormalizationMode;
            }

            /**
            /// Initializes the specified security by setting up the models
            */
             * @param security">The security to be initialized
            public @Override void Initialize(Security security) {
                // first call the default implementation
                base.Initialize(security);

                // now apply our data normalization mode
                security.SetDataNormalizationMode(_dataNormalizationMode);
            }
        }
    }
}
