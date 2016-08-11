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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Brokerages.InteractiveBrokers;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using QuantConnect.Orders;

package com.quantconnect.lean.Tests.Brokerages.InteractiveBrokers
{
    [TestFixture]
    [Ignore("These tests require the IBController and IB TraderWorkstation to be installed.")]
    public class InteractiveBrokersBrokerageTests
    {
        private readonly List<Order> _orders = new List<Order>(); 
        private InteractiveBrokersBrokerage _interactiveBrokersBrokerage;
        private static final int buyQuantity = 100;
        private static final SecurityType Type = SecurityType.Forex;

        [SetUp]
        public void InitializeBrokerage()
        {
            InteractiveBrokersGatewayRunner.Start(Config.Get("ib-controller-dir"), 
                Config.Get("ib-tws-dir"), 
                Config.Get("ib-user-name"), 
                Config.Get("ib-password"), 
                Config.GetBool("ib-use-tws")
                );

            // grabs account info from configuration
            _interactiveBrokersBrokerage = new InteractiveBrokersBrokerage(new OrderProvider(_orders), new SecurityProvider());
            _interactiveBrokersBrokerage.Connect();
        }

        [TearDown]
        public void Teardown()
        {
            try
            { // give the tear down a header so we can easily find it in the logs
                Log.Trace("-----");
                Log.Trace("InteractiveBrokersBrokerageTests.Teardown(): Starting teardown...");
                Log.Trace("-----");

                canceledResetEvent = new ManualResetEvent(false);
                filledResetEvent = new ManualResetEvent(false);
                _interactiveBrokersBrokerage.OrderStatusChanged += (sender, orderEvent) =>
                {
                    if (orderEvent.Status == OrderStatus.Filled)
                    {
                        filledResetEvent.Set();
                    }
                    if (orderEvent.Status == OrderStatus.Canceled)
                    {
                        canceledResetEvent.Set();
                    }
                };

                // cancel all open orders

                Log.Trace("InteractiveBrokersBrokerageTests.Teardown(): Canceling open orders...");

                orders = _interactiveBrokersBrokerage.GetOpenOrders();
                foreach (order in orders)
                {
                    _interactiveBrokersBrokerage.CancelOrder(order);
                    canceledResetEvent.WaitOne(3000);
                    canceledResetEvent.Reset();
                }

                Log.Trace("InteractiveBrokersBrokerageTests.Teardown(): Liquidating open positions...");

                // liquidate all positions
                holdings = _interactiveBrokersBrokerage.GetAccountHoldings();
                foreach (holding in holdings.Where(x => x.Quantity != 0))
                {
                    //liquidate = new MarketOrder(holding.Symbol, (int) -holding.Quantity, DateTime.UtcNow, type: holding.Type);
                    //_interactiveBrokersBrokerage.PlaceOrder(liquidate);
                    //filledResetEvent.WaitOne(3000);
                    //filledResetEvent.Reset();
                }

                openOrdersText = _interactiveBrokersBrokerage.GetOpenOrders().Select(x => x.Symbol.toString() + " " + x.Quantity);
                Log.Trace("InteractiveBrokersBrokerageTests.Teardown(): Open orders: " + string.Join(", ", openOrdersText));
                //Assert.AreEqual(0, actualOpenOrderCount, "Failed to verify that there are zero open orders.");

                holdingsText = _interactiveBrokersBrokerage.GetAccountHoldings().Where(x => x.Quantity != 0).Select(x => x.Symbol.toString() + " " + x.Quantity);
                Log.Trace("InteractiveBrokersBrokerageTests.Teardown(): Account holdings: " + string.Join(", ", holdingsText));
                //Assert.AreEqual(0, holdingsCount, "Failed to verify that there are zero account holdings.");

                _interactiveBrokersBrokerage.Dispose();
                _interactiveBrokersBrokerage = null;
            }
            finally
            {
                InteractiveBrokersGatewayRunner.Stop();
            }
        }

        [Test]
        public void ClientConnects()
        {
            ib = _interactiveBrokersBrokerage;
            Assert.IsTrue(ib.IsConnected);
        }

        [Test]
        public void PlacedOrderHasNewBrokerageOrderID()
        {
            ib = _interactiveBrokersBrokerage;

            order = new MarketOrder(Symbols.USDJPY, buyQuantity, DateTime.UtcNow);
            _orders.Add(order);
            ib.PlaceOrder(order);

            brokerageID = order.BrokerId.Single();
            Assert.AreNotEqual(0, brokerageID);

            order = new MarketOrder(Symbols.USDJPY, buyQuantity, DateTime.UtcNow);
            _orders.Add(order);
            ib.PlaceOrder(order);

            Assert.AreNotEqual(brokerageID, order.BrokerId.Single());
        }

        [Test]
        public void ClientPlacesMarketOrder()
        {
            boolean orderFilled = false;
            manualResetEvent = new ManualResetEvent(false);
            ib = _interactiveBrokersBrokerage;

            ib.OrderStatusChanged += (sender, orderEvent) =>
            {
                if (orderEvent.Status == OrderStatus.Filled)
                {
                    orderFilled = true;
                    manualResetEvent.Set();
                }
            };

            order = new MarketOrder(Symbols.USDJPY, buyQuantity, DateTime.UtcNow) {Id = 1};
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOne(2500);
            orderFromIB = AssertOrderOpened(orderFilled, ib, order);
            Assert.AreEqual(OrderType.Market, orderFromIB.Type);
        }

        [Test]
        public void ClientSellsMarketOrder()
        {
            boolean orderFilled = false;
            manualResetEvent = new ManualResetEvent(false);

            ib = _interactiveBrokersBrokerage;

            ib.OrderStatusChanged += (sender, orderEvent) =>
            {
                if (orderEvent.Status == OrderStatus.Filled)
                {
                    orderFilled = true;
                    manualResetEvent.Set();
                }
            };

            // sell a single share
            order = new MarketOrder(Symbols.USDJPY, -buyQuantity, DateTime.UtcNow);
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOne(2500);

            orderFromIB = AssertOrderOpened(orderFilled, ib, order);
            Assert.AreEqual(OrderType.Market, orderFromIB.Type);
        }

        [Test]
        public void ClientPlacesLimitOrder()
        {
            boolean orderFilled = false;
            manualResetEvent = new ManualResetEvent(false);
            ib = _interactiveBrokersBrokerage;

            BigDecimal price = 100m;
            BigDecimal delta = 85.0m; // if we can't get a price then make the delta huge
            ib.OrderStatusChanged += (sender, orderEvent) =>
            {
                if (orderEvent.Status == OrderStatus.Filled)
                {
                    orderFilled = true;
                    manualResetEvent.Set();
                }
                price = orderEvent.FillPrice;
                delta = 0.02m;
            };

            // get the current market price, couldn't get RequestMarketData to fire tick events
            int id = 0;
            Order order = new MarketOrder(Symbols.USDJPY, buyQuantity, DateTime.UtcNow) { Id = ++id };
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOne(2000);
            manualResetEvent.Reset();

            // make a box around the current price +- a little

            order = new LimitOrder(Symbols.USDJPY, buyQuantity, price - delta, DateTime.UtcNow, null) { Id = ++id };
            _orders.Add(order);
            ib.PlaceOrder(order);

            order = new LimitOrder(Symbols.USDJPY, -buyQuantity, price + delta, DateTime.UtcNow, null) { Id = ++id };
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOne(1000);

            orderFromIB = AssertOrderOpened(orderFilled, ib, order);
            Assert.AreEqual(OrderType.Limit, orderFromIB.Type);
        }

        [Test]
        public void ClientPlacesStopLimitOrder()
        {
            boolean orderFilled = false;
            manualResetEvent = new ManualResetEvent(false);
            ib = _interactiveBrokersBrokerage;

            BigDecimal fillPrice = 100m;
            BigDecimal delta = 85.0m; // if we can't get a price then make the delta huge
            ib.OrderStatusChanged += (sender, args) =>
            {
                orderFilled = true;
                fillPrice = args.FillPrice;
                delta = 0.02m;
                manualResetEvent.Set();
            };

            // get the current market price, couldn't get RequestMarketData to fire tick events
            int id = 0;
            Order order = new MarketOrder(Symbols.USDJPY, buyQuantity, DateTime.UtcNow) { Id = ++id };
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOne(2000);
            manualResetEvent.Reset();
            Assert.IsTrue(orderFilled);

            orderFilled = false;

            // make a box around the current price +- a little

            order = new StopMarketOrder(Symbols.USDJPY, buyQuantity, fillPrice - delta, DateTime.UtcNow) { Id = ++id };
            _orders.Add(order);
            ib.PlaceOrder(order);

            order = new StopMarketOrder(Symbols.USDJPY, -buyQuantity, fillPrice + delta, DateTime.UtcNow) { Id = ++id };
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOne(1000);

            orderFromIB = AssertOrderOpened(orderFilled, ib, order);
            Assert.AreEqual(OrderType.StopMarket, orderFromIB.Type);
        }

        [Test]
        public void ClientUpdatesLimitOrder()
        {
            int id = 0;
            ib = _interactiveBrokersBrokerage;

            boolean filled = false;
            ib.OrderStatusChanged += (sender, args) =>
            {
                if (args.Status == OrderStatus.Filled)
                {
                    filled = true;
                }
            };

            static final BigDecimal limitPrice = 10000m;
            order = new LimitOrder(Symbols.USDJPY, -buyQuantity, limitPrice, DateTime.UtcNow) {Id = ++id};
            _orders.Add(order);
            ib.PlaceOrder(order);

            stopwatch = Stopwatch.StartNew();
            while (!filled && stopwatch.Elapsed.TotalSeconds < 10)
            {
                //Thread.MemoryBarrier();
                Thread.Sleep(1000);
                order.LimitPrice = order.LimitPrice/2;
                ib.UpdateOrder(order);
            }

            Assert.IsTrue(filled);
        }

        [Test]
        public void ClientCancelsLimitOrder()
        {
            orderedResetEvent = new ManualResetEvent(false);
            canceledResetEvent = new ManualResetEvent(false);

            ib = _interactiveBrokersBrokerage;

            ib.OrderStatusChanged += (sender, orderEvent) =>
            {
                if (orderEvent.Status == OrderStatus.Submitted)
                {
                    orderedResetEvent.Set();
                }
                if (orderEvent.Status == OrderStatus.Canceled)
                {
                    canceledResetEvent.Set();
                }
            };

            // try to sell a single share at a ridiculous price, we'll cancel this later
            order = new LimitOrder(Symbols.USDJPY, -buyQuantity, 100000, DateTime.UtcNow, null);
            _orders.Add(order);
            ib.PlaceOrder(order);
            orderedResetEvent.WaitOneAssertFail(2500, "Limit order failed to be submitted.");

            Thread.Sleep(500);

            ib.CancelOrder(order);

            canceledResetEvent.WaitOneAssertFail(2500, "Canceled event did not fire.");

            openOrders = ib.GetOpenOrders();
            cancelledOrder = openOrders.FirstOrDefault(x => x.BrokerId.Contains(order.BrokerId[0]));
            Assert.IsNull(cancelledOrder);
        }

        [Test]
        public void ClientFiresSingleOrderFilledEvent()
        {
            ib = _interactiveBrokersBrokerage;

            order = new MarketOrder(Symbols.USDJPY, buyQuantity, new DateTime()) {Id = 1};
            _orders.Add(order);

            int orderFilledEventCount = 0;
            orderFilledResetEvent = new ManualResetEvent(false);
            ib.OrderStatusChanged += (sender, fill) =>
            {
                if (fill.Status == OrderStatus.Filled)
                {
                    orderFilledEventCount++;
                    orderFilledResetEvent.Set();
                }

                // mimic what the transaction handler would do
                order.Status = fill.Status;
            };

            ib.PlaceOrder(order);

            orderFilledResetEvent.WaitOneAssertFail(2500, "Didnt fire order filled event");

            // wait a little to see if we get multiple fill events
            Thread.Sleep(2000);

            Assert.AreEqual(1, orderFilledEventCount);
        }

        [Test]
        public void GetsAccountHoldings()
        {
            ib = _interactiveBrokersBrokerage;

            Thread.Sleep(500);

            previousHoldings = ib.GetAccountHoldings().ToDictionary(x => x.Symbol);

            foreach (holding in previousHoldings)
            {
                Console.WriteLine(holding.Value);
            }

            Log.Trace("Quantity: " + previousHoldings[Symbols.USDJPY].Quantity);

            boolean hasSymbol = previousHoldings.ContainsKey(Symbols.USDJPY);

            // wait for order to complete before request account holdings
            orderResetEvent = new ManualResetEvent(false);
            ib.OrderStatusChanged += (sender, fill) =>
            {
                if (fill.Status == OrderStatus.Filled) orderResetEvent.Set();
            };

            // buy some currency
            static final int quantity = -buyQuantity;
            order = new MarketOrder(Symbols.USDJPY, quantity, DateTime.UtcNow);
            _orders.Add(order);
            ib.PlaceOrder(order);

            // wait for the order to go through
            orderResetEvent.WaitOneAssertFail(3000, "Didn't receive order event");

            // ib is slow to update tws
            Thread.Sleep(5000);

            newHoldings = ib.GetAccountHoldings().ToDictionary(x => x.Symbol);
            Log.Trace("New Quantity: " + newHoldings[Symbols.USDJPY].Quantity);

            if (hasSymbol)
            {
                Assert.AreEqual(previousHoldings[Symbols.USDJPY].Quantity, newHoldings[Symbols.USDJPY].Quantity - quantity);
            }
            else
            {
                Assert.IsTrue(newHoldings.ContainsKey(Symbols.USDJPY));
                Assert.AreEqual(newHoldings[Symbols.USDJPY].Quantity, quantity);
            }
        }

        [Test]
        public void GetsCashBalanceAfterConnect()
        {
            ib = _interactiveBrokersBrokerage;
            cashBalance = ib.GetCashBalance();
            Assert.IsTrue(cashBalance.Any(x => x.Symbol == "USD"));
            foreach (cash in cashBalance)
            {
                Console.WriteLine(cash);
                if (cash.Symbol == "USD")
                {
                    Assert.AreNotEqual(0m, cashBalance.Single(x => x.Symbol == "USD"));
                }
            }
        }

        [Test]
        public void FiresMultipleAccountBalanceEvents()
        {
            ib = _interactiveBrokersBrokerage;

            orderEventFired = new ManualResetEvent(false);
            ib.OrderStatusChanged += (sender, args) =>
            {
                if (args.Status == OrderStatus.Filled)
                {
                    orderEventFired.Set();
                }
            };

            cashBalanceUpdates = new List<decimal>();
            accountChangedFired = new ManualResetEvent(false);
            ib.AccountChanged += (sender, args) =>
            {
                cashBalanceUpdates.Add(args.CashBalance);
                accountChangedFired.Set();
            };

            static final int orderCount = 3;
            for (int i = 0; i < orderCount; i++)
            {
                order = new MarketOrder(Symbols.USDJPY, buyQuantity*(i + 1), new DateTime()) {Id = i + 1};
                _orders.Add(order);
                ib.PlaceOrder(order);

                orderEventFired.WaitOneAssertFail(1500, "Didnt receive order event #" + i);
                orderEventFired.Reset();

                accountChangedFired.WaitOneAssertFail(1500, "Didnt receive account event #" + i);
                accountChangedFired.Reset();
            }

            Assert.AreEqual(orderCount, cashBalanceUpdates.Count);
        }

        [Test]
        public void GetsCashBalanceAfterTrade()
        {
            ib = _interactiveBrokersBrokerage;

            BigDecimal balance = ib.GetCashBalance().Single(x => x.Symbol == "USD").Amount;

            // wait for our order to fill
            manualResetEvent = new ManualResetEvent(false);
            ib.AccountChanged += (sender, orderEvent) => manualResetEvent.Set();

            order = new MarketOrder(Symbols.USDJPY, buyQuantity, DateTime.UtcNow);
            _orders.Add(order);
            ib.PlaceOrder(order);

            manualResetEvent.WaitOneAssertFail(1500, "Didn't receive account changed event");

            BigDecimal balanceAfterTrade = ib.GetCashBalance().Single(x => x.Symbol == "USD").Amount;

            Assert.AreNotEqual(balance, balanceAfterTrade);
        }

        [Test]
        public void GetExecutions()
        {
            ib = _interactiveBrokersBrokerage;

            orderEventFired = new ManualResetEvent(false);
            ib.OrderStatusChanged += (sender, args) =>
            {
                if (args.Status == OrderStatus.Filled)
                {
                    orderEventFired.Set();
                }
            };

            order = new MarketOrder(Symbols.USDJPY, -buyQuantity, new DateTime());
            _orders.Add(order);
            ib.PlaceOrder(order);
            orderEventFired.WaitOne(1500);

            executions = ib.GetExecutions(null, null, null, DateTime.UtcNow.AddDays(-1), null);

            Assert.IsTrue(executions.Any(x => order.BrokerId.Any(id => executions.Any(e => e.OrderId == int.Parse(id)))));
        }

        [Test]
        public void GetOpenOrders()
        {
            ib = _interactiveBrokersBrokerage;

            orderEventFired = new ManualResetEvent(false);
            ib.OrderStatusChanged += (sender, args) =>
            {
                if (args.Status == OrderStatus.Submitted)
                {
                    orderEventFired.Set();
                }
            };

            order = new LimitOrder(Symbols.USDJPY, buyQuantity, 0.01m, DateTime.UtcNow);
            _orders.Add(order);
            ib.PlaceOrder(order);

            orderEventFired.WaitOne(1500);

            Thread.Sleep(250);

            openOrders = ib.GetOpenOrders();

            Assert.AreNotEqual(0, openOrders.Count);
        }

        [Test, Ignore("This test requires disconnecting the internet to test for connection resiliency")]
        public void ClientReconnectsAfterInternetDisconnect()
        {
            ib = _interactiveBrokersBrokerage;
            Assert.IsTrue(ib.IsConnected);

            tenMinutes = TimeSpan.FromMinutes(10);
            
            Console.WriteLine("------");
            Console.WriteLine("Waiting for internet disconnection ");
            Console.WriteLine("------");

            // spin while we manually disconnect the internet
            while (ib.IsConnected)
            {
                Thread.Sleep(2500);
                Console.Write(".");
            }
            
            stopwatch = Stopwatch.StartNew();

            Console.WriteLine("------");
            Console.WriteLine("Trying to reconnect ");
            Console.WriteLine("------");

            // spin until we're reconnected
            while (!ib.IsConnected && stopwatch.Elapsed < tenMinutes)
            {
                Thread.Sleep(2500);
                Console.Write(".");
            }

            Assert.IsTrue(ib.IsConnected);
        }

        private static Order AssertOrderOpened( boolean orderFilled, InteractiveBrokersBrokerage ib, Order order)
        {
            // if the order didn't fill check for it as an open order
            if (!orderFilled)
            {
                // find the right order and return it
                foreach (openOrder in ib.GetOpenOrders())
                {
                    if (openOrder.BrokerId.Any(id => order.BrokerId.Any(x => x == id)))
                    {
                        return openOrder;
                    }
                }
                Assert.Fail("The order was not filled and was unable to be located via GetOpenOrders()");
            }

            Assert.Pass("The order was successfully filled!");
            return null;
        }

    }
}