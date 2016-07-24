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

using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace QuantConnect.Tests
{
    [TestFixture, Category("TravisExclude")]
    public class RegressionTests
    {
        [Test, TestCaseSource("GetRegressionTestParameters")]
        public void AlgorithmStatisticsRegression(AlgorithmStatisticsTestParameters parameters)
        {
            AlgorithmRunner.RunLocalBacktest(parameters.Algorithm, parameters.Statistics, parameters.Language);
        }

        private static TestCaseData[] GetRegressionTestParameters()
        {
            basicTemplateStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "1"},
                {"Average Win", "0%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "264.956%"},
                {"Drawdown", "2.200%"},
                {"Expectancy", "0"},
                {"Net Profit", "0%"},
                {"Sharpe Ratio", "4.411"},
                {"Loss Rate", "0%"},
                {"Win Rate", "0%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "0.002"},
                {"Beta", "1"},
                {"Annual Standard Deviation", "0.193"},
                {"Annual Variance", "0.037"},
                {"Information Ratio", "6.816"},
                {"Tracking Error", "0"},
                {"Treynor Ratio", "0.851"},
                {"Total Fees", "$3.09"}
            };

            basicTemplateOptionsStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "2"},
                {"Average Win", "18.70%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "79228162514264337593543950335%"},
                {"Drawdown", "40.500%"},
                {"Expectancy", "0"},
                {"Net Profit", "18.700%"},
                {"Sharpe Ratio", "0"},
                {"Loss Rate", "0%"},
                {"Win Rate", "100%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "0"},
                {"Beta", "0"},
                {"Annual Standard Deviation", "0"},
                {"Annual Variance", "0"},
                {"Information Ratio", "0"},
                {"Tracking Error", "0"},
                {"Treynor Ratio", "0"},
                {"Total Fees", "$0.00"},
            };

            limitFillRegressionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "34"},
                {"Average Win", "0.02%"},
                {"Average Loss", "-0.02%"},
                {"Compounding Annual Return", "8.350%"},
                {"Drawdown", "0.400%"},
                {"Expectancy", "0.447"},
                {"Net Profit", "0.103%"},
                {"Sharpe Ratio", "1.747"},
                {"Loss Rate", "31%"},
                {"Win Rate", "69%"},
                {"Profit-Loss Ratio", "1.10"},
                {"Alpha", "-0.077"},
                {"Beta", "0.152"},
                {"Annual Standard Deviation", "0.03"},
                {"Annual Variance", "0.001"},
                {"Information Ratio", "-4.87"},
                {"Tracking Error", "0.164"},
                {"Treynor Ratio", "0.343"},
                {"Total Fees", "$34.00"}
            };

            updateOrderRegressionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "21"},
                {"Average Win", "0%"},
                {"Average Loss", "-1.71%"},
                {"Compounding Annual Return", "-8.289%"},
                {"Drawdown", "16.700%"},
                {"Expectancy", "-1"},
                {"Net Profit", "-15.892%"},
                {"Sharpe Ratio", "-1.225"},
                {"Loss Rate", "100%"},
                {"Win Rate", "0%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "0.011"},
                {"Beta", "-0.469"},
                {"Annual Standard Deviation", "0.056"},
                {"Annual Variance", "0.003"},
                {"Information Ratio", "-1.573"},
                {"Tracking Error", "0.152"},
                {"Treynor Ratio", "0.147"},
                {"Total Fees", "$21.00"}
            };

            regressionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "5433"},
                {"Average Win", "0.00%"},
                {"Average Loss", "0.00%"},
                {"Compounding Annual Return", "-3.886%"},
                {"Drawdown", "0.100%"},
                {"Expectancy", "-0.991"},
                {"Net Profit", "-0.054%"},
                {"Sharpe Ratio", "-30.336"},
                {"Loss Rate", "100%"},
                {"Win Rate", "0%"},
                {"Profit-Loss Ratio", "2.40"},
                {"Alpha", "-0.022"},
                {"Beta", "-0.001"},
                {"Annual Standard Deviation", "0.001"},
                {"Annual Variance", "0"},
                {"Information Ratio", "-4.198"},
                {"Tracking Error", "0.174"},
                {"Treynor Ratio", "35.023"},
                {"Total Fees", "$5433.00"}
            };

            universeSelectionRegressionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "4"},
                {"Average Win", "0.70%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "-56.034%"},
                {"Drawdown", "3.800%"},
                {"Expectancy", "0"},
                {"Net Profit", "-3.755%"},
                {"Sharpe Ratio", "-3.629"},
                {"Loss Rate", "0%"},
                {"Win Rate", "100%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "-0.424"},
                {"Beta", "1.25"},
                {"Annual Standard Deviation", "0.173"},
                {"Annual Variance", "0.03"},
                {"Information Ratio", "-3.62"},
                {"Tracking Error", "0.128"},
                {"Treynor Ratio", "-0.502"},
                {"Total Fees", "$2.00"}
            };

            customDataRegressionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "1"},
                {"Average Win", "0%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "155.210%"},
                {"Drawdown", "99.900%"},
                {"Expectancy", "0"},
                {"Net Profit", "0%"},
                {"Sharpe Ratio", "0.453"},
                {"Loss Rate", "0%"},
                {"Win Rate", "0%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "46.332"},
                {"Beta", "73.501"},
                {"Annual Standard Deviation", "118.922"},
                {"Annual Variance", "14142.47"},
                {"Information Ratio", "0.452"},
                {"Tracking Error", "118.915"},
                {"Treynor Ratio", "0.733"},
                {"Total Fees", "$0.00"}
            };

            addRemoveSecurityRegressionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "5"},
                {"Average Win", "0.49%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "307.853%"},
                {"Drawdown", "1.400%"},
                {"Expectancy", "0"},
                {"Net Profit", "1.814%"},
                {"Sharpe Ratio", "6.474"},
                {"Loss Rate", "0%"},
                {"Win Rate", "100%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "0.306"},
                {"Beta", "0.718"},
                {"Annual Standard Deviation", "0.141"},
                {"Annual Variance", "0.02"},
                {"Information Ratio", "1.077"},
                {"Tracking Error", "0.062"},
                {"Treynor Ratio", "1.275"},
                {"Total Fees", "$25.20"}
            };

            dropboxBaseDataUniverseSelectionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "67"},
                {"Average Win", "1.13%"},
                {"Average Loss", "-0.69%"},
                {"Compounding Annual Return", "17.718%"},
                {"Drawdown", "5.100%"},
                {"Expectancy", "0.813"},
                {"Net Profit", "17.718%"},
                {"Sharpe Ratio", "1.38"},
                {"Loss Rate", "31%"},
                {"Win Rate", "69%"},
                {"Profit-Loss Ratio", "1.64"},
                {"Alpha", "0.055"},
                {"Beta", "0.379"},
                {"Annual Standard Deviation", "0.099"},
                {"Annual Variance", "0.01"},
                {"Information Ratio", "-0.703"},
                {"Tracking Error", "0.11"},
                {"Treynor Ratio", "0.359"},
                {"Total Fees", "$300.15"}
            };

            dropboxUniverseSelectionStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "49"},
                {"Average Win", "1.58%"},
                {"Average Loss", "-1.03%"},
                {"Compounding Annual Return", "21.281%"},
                {"Drawdown", "8.200%"},
                {"Expectancy", "0.646"},
                {"Net Profit", "21.281%"},
                {"Sharpe Ratio", "1.362"},
                {"Loss Rate", "35%"},
                {"Win Rate", "65%"},
                {"Profit-Loss Ratio", "1.52"},
                {"Alpha", "0.012"},
                {"Beta", "0.705"},
                {"Annual Standard Deviation", "0.12"},
                {"Annual Variance", "0.014"},
                {"Information Ratio", "-0.51"},
                {"Tracking Error", "0.101"},
                {"Treynor Ratio", "0.232"},
                {"Total Fees", "$232.92"}
            };

            parameterizedStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "1"},
                {"Average Win", "0%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "278.616%"},
                {"Drawdown", "0.300%"},
                {"Expectancy", "0"},
                {"Net Profit", "0%"},
                {"Sharpe Ratio", "11.017"},
                {"Loss Rate", "0%"},
                {"Win Rate", "0%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "0.553"},
                {"Beta", "0.364"},
                {"Annual Standard Deviation", "0.078"},
                {"Annual Variance", "0.006"},
                {"Information Ratio", "0.101"},
                {"Tracking Error", "0.127"},
                {"Treynor Ratio", "2.367"},
                {"Total Fees", "$3.09"},
            };

            historyAlgorithmStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "1"},
                {"Average Win", "0%"},
                {"Average Loss", "0%"},
                {"Compounding Annual Return", "372.677%"},
                {"Drawdown", "1.100%"},
                {"Expectancy", "0"},
                {"Net Profit", "0%"},
                {"Sharpe Ratio", "4.521"},
                {"Loss Rate", "0%"},
                {"Win Rate", "0%"},
                {"Profit-Loss Ratio", "0"},
                {"Alpha", "0.774"},
                {"Beta", "0.182"},
                {"Annual Standard Deviation", "0.193"},
                {"Annual Variance", "0.037"},
                {"Information Ratio", "1.319"},
                {"Tracking Error", "0.247"},
                {"Treynor Ratio", "4.798"},
                {"Total Fees", "$3.09"},
            };

            coarseFundamentalTop5AlgorithmStatistics = new Dictionary<string, string>
            {
                {"Total Trades", "8"},
                {"Average Win", "1.15%"},
                {"Average Loss", "-0.60%"},
                {"Compounding Annual Return", "-0.660%"},
                {"Drawdown", "2.600%"},
                {"Expectancy", "-0.271"},
                {"Net Profit", "-0.660%"},
                {"Sharpe Ratio", "-0.255"},
                {"Loss Rate", "75%"},
                {"Win Rate", "25%"},
                {"Profit-Loss Ratio", "1.92"},
                {"Alpha", "-0.009"},
                {"Beta", "0.037"},
                {"Annual Standard Deviation", "0.02"},
                {"Annual Variance", "0"},
                {"Information Ratio", "-0.967"},
                {"Tracking Error", "0.1"},
                {"Treynor Ratio", "-0.142"},
                {"Total Fees", "$8.61"},
            };

            return new List<AlgorithmStatisticsTestParameters>
            {
                // CSharp
                new AlgorithmStatisticsTestParameters("AddRemoveSecurityRegressionAlgorithm", addRemoveSecurityRegressionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("BasicTemplateAlgorithm", basicTemplateStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("BasicTemplateOptionsAlgorithm", basicTemplateOptionsStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("CustomDataRegressionAlgorithm", customDataRegressionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("DropboxBaseDataUniverseSelectionAlgorithm", dropboxBaseDataUniverseSelectionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("DropboxUniverseSelectionAlgorithm", dropboxUniverseSelectionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("LimitFillRegressionAlgorithm", limitFillRegressionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("ParameterizedAlgorithm", parameterizedStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("RegressionAlgorithm", regressionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("UniverseSelectionRegressionAlgorithm", universeSelectionRegressionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("UpdateOrderRegressionAlgorithm", updateOrderRegressionStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("HistoryAlgorithm", historyAlgorithmStatistics, Language.CSharp),
                new AlgorithmStatisticsTestParameters("CoarseFundamentalTop5Algorithm", coarseFundamentalTop5AlgorithmStatistics, Language.CSharp),

                // FSharp
                // new AlgorithmStatisticsTestParameters("BasicTemplateAlgorithm", basicTemplateStatistics, Language.FSharp),

                // VisualBasic
                // new AlgorithmStatisticsTestParameters("BasicTemplateAlgorithm", basicTemplateStatistics, Language.VisualBasic),
            }.Select(x => new TestCaseData(x).SetName(x.Language + "/" + x.Algorithm)).ToArray();
        }

        public class AlgorithmStatisticsTestParameters
        {
            public readonly String Algorithm;
            public readonly Dictionary<string, string> Statistics;
            public readonly Language Language;

            public AlgorithmStatisticsTestParameters( String algorithm, Dictionary<string, string> statistics, Language language)
            {
                Algorithm = algorithm;
                Statistics = statistics;
                Language = language;
            }
        }
    }

}