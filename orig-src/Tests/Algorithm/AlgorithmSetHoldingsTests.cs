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
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Algorithm
{
    [TestFixture]
    public class AlgorithmSetHoldingsTests
    {
        public enum Position { Zero = 0, Long = 1, Short = -1 }
        public enum FeeType { None, Small, Large, InteractiveBrokers }
        public enum PriceMovement { Static, RisingSmall, FallingSmall, RisingLarge, FallingLarge }

        private readonly Map<FeeType, IFeeModel> _feeModels = new Map<FeeType, IFeeModel>
        {
            { FeeType.None, new ConstantFeeModel(0) },
            { FeeType.Small, new ConstantFeeModel(1) },
            { FeeType.Large, new ConstantFeeModel(100) },
            { FeeType.InteractiveBrokers, new InteractiveBrokersFeeModel() }
        };

        private readonly Symbol _symbol = Symbols.SPY;
        private static final BigDecimal Cash = 100000m;
        private static final BigDecimal VeryLowPrice = 155m;
        private static final BigDecimal LowPrice = 159m;
        private static final BigDecimal BasePrice = 160m;
        private static final BigDecimal HighPrice = 161m;
        private static final BigDecimal VeryHighPrice = 165m;

        public class Permuter<T>
        {
            private static void Permute(T[] row, int index, IReadOnlyList<List<T>> data, ICollection<T[]> result)
            {
                foreach (dataRow in data[index])
                {
                    row[index] = dataRow;
                    if (index >= data.Count - 1)
                    {
                        rowCopy = new T[row.Length];
                        row.CopyTo(rowCopy, 0);
                        result.Add(rowCopy);
                    }
                    else
                    {
                        Permute(row, index + 1, data, result);
                    }
                }
            }

            public static void Permute(List<List<T>> data, List<T[]> result)
            {
                if (data.Count == 0)
                    return;

                Permute(new T[data.Count], 0, data, result);
            }
        }

        public TestCaseData[] TestParameters
        {
            get
            {
                initialPositions = Enum.GetValues(typeof(Position)).Cast<Position>().ToList();
                finalPositions = Enum.GetValues(typeof(Position)).Cast<Position>().ToList();
                feeTypes = Enum.GetValues(typeof(FeeType)).Cast<FeeType>().ToList();
                priceMovements = Enum.GetValues(typeof(PriceMovement)).Cast<PriceMovement>().ToList();
                leverages = new List<Integer> { 1, 100 };

                data = new List<List<object>>
                {
                    initialPositions.Cast<object>().ToList(),
                    finalPositions.Cast<object>().ToList(),
                    feeTypes.Cast<object>().ToList(),
                    priceMovements.Cast<object>().ToList(),
                    leverages.Cast<object>().ToList()
                };
                permutations = new List<object[]>();
                Permuter<object>.Permute(data, permutations);

                ret = permutations
                    .Where(row => (Position)row[0] != (Position)row[1])     // initialPosition != finalPosition
                    .Select(row => new TestCaseData(row).SetName( String.Join("_", row)))
                    .ToArray();

                return ret;
            }
        }

        [Test, TestCaseSource("TestParameters")]
        public void Run(Position initialPosition, Position finalPosition, FeeType feeType, PriceMovement priceMovement, int leverage)
        {
            //Console.WriteLine("----------");
            //Console.WriteLine("PARAMETERS");
            //Console.WriteLine("Initial position: " + initialPosition);
            //Console.WriteLine("Final position: " + finalPosition);
            //Console.WriteLine("Fee type: " + feeType);
            //Console.WriteLine("Price movement: " + priceMovement);
            //Console.WriteLine("Leverage: " + leverage);
            //Console.WriteLine("----------");
            //Console.WriteLine();

            algorithm = new QCAlgorithm();

            security = algorithm.AddSecurity(_symbol.ID.SecurityType, _symbol.ID.Symbol);
            security.FeeModel = _feeModels[feeType];
            security.SetLeverage(leverage);

            algorithm.SetCash(Cash);

            Update(security, BasePrice);

            BigDecimal targetPercentage;
            OrderDirection orderDirection;
            MarketOrder order;
            BigDecimal orderFee;
            OrderEvent fill;
            int orderQuantity;
            BigDecimal freeMargin;
            BigDecimal requiredMargin;
            if (initialPosition != Position.Zero)
            {
                targetPercentage = (decimal)initialPosition;
                orderDirection = initialPosition == Position.Long ? OrderDirection.Buy : OrderDirection.Sell;
                orderQuantity = algorithm.CalculateOrderQuantity(_symbol, targetPercentage);
                order = new MarketOrder(_symbol, orderQuantity, DateTime.UtcNow);
                freeMargin = algorithm.Portfolio.GetMarginRemaining(_symbol, orderDirection);
                requiredMargin = security.MarginModel.GetInitialMarginRequiredForOrder(security, order);

                //Console.WriteLine("Current price: " + security.Price);
                //Console.WriteLine("Target percentage: " + targetPercentage);
                //Console.WriteLine("Order direction: " + orderDirection);
                //Console.WriteLine("Order quantity: " + orderQuantity);
                //Console.WriteLine("Free margin: " + freeMargin);
                //Console.WriteLine("Required margin: " + requiredMargin);
                //Console.WriteLine();

                Assert.That(Math.Abs(requiredMargin) <= freeMargin);

                orderFee = security.FeeModel.GetOrderFee(security, order);
                fill = new OrderEvent(order, DateTime.UtcNow, orderFee) { FillPrice = security.Price, FillQuantity = orderQuantity };
                algorithm.Portfolio.ProcessFill(fill);

                //Console.WriteLine("Portfolio.Cash: " + algorithm.Portfolio.Cash);
                //Console.WriteLine("Portfolio.TotalPortfolioValue: " + algorithm.Portfolio.TotalPortfolioValue);
                //Console.WriteLine();

                if (priceMovement == PriceMovement.RisingSmall)
                {
                    Update(security, HighPrice);
                }
                else if (priceMovement == PriceMovement.FallingSmall)
                {
                    Update(security, LowPrice);
                }
                else if (priceMovement == PriceMovement.RisingLarge)
                {
                    Update(security, VeryHighPrice);
                }
                else if (priceMovement == PriceMovement.FallingLarge)
                {
                    Update(security, VeryLowPrice);
                }
            }

            targetPercentage = (decimal)finalPosition;
            orderDirection = finalPosition == Position.Long || (finalPosition == Position.Zero && initialPosition == Position.Short) ? OrderDirection.Buy : OrderDirection.Sell;
            orderQuantity = algorithm.CalculateOrderQuantity(_symbol, targetPercentage);
            order = new MarketOrder(_symbol, orderQuantity, DateTime.UtcNow);
            freeMargin = algorithm.Portfolio.GetMarginRemaining(_symbol, orderDirection);
            requiredMargin = security.MarginModel.GetInitialMarginRequiredForOrder(security, order);

            //Console.WriteLine("Current price: " + security.Price);
            //Console.WriteLine("Target percentage: " + targetPercentage);
            //Console.WriteLine("Order direction: " + orderDirection);
            //Console.WriteLine("Order quantity: " + orderQuantity);
            //Console.WriteLine("Free margin: " + freeMargin);
            //Console.WriteLine("Required margin: " + requiredMargin);
            //Console.WriteLine();

            Assert.That(Math.Abs(requiredMargin) <= freeMargin);

            orderFee = security.FeeModel.GetOrderFee(security, order);
            fill = new OrderEvent(order, DateTime.UtcNow, orderFee) { FillPrice = security.Price, FillQuantity = orderQuantity };
            algorithm.Portfolio.ProcessFill(fill);

            //Console.WriteLine("Portfolio.Cash: " + algorithm.Portfolio.Cash);
            //Console.WriteLine("Portfolio.TotalPortfolioValue: " + algorithm.Portfolio.TotalPortfolioValue);
            //Console.WriteLine();
        }

        private static void Update(Security security, BigDecimal price)
        {
            security.SetMarketPrice(new TradeBar
            {
                Time = DateTime.Now, Symbol = security.Symbol, Open = price, High = price, Low = price, Close = price
            });
        }
    }
}
