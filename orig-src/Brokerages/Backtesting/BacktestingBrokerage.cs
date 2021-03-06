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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Brokerages.Backtesting
{
    /**
     * Represents a brokerage to be used during backtesting. This is intended to be only be used with the BacktestingTransactionHandler
    */
    public class BacktestingBrokerage : Brokerage
    {
        // flag used to indicate whether or not we need to scan for
        // fills, this is purely a performance concern is ConcurrentDictionary.IsEmpty
        // is not exactly the fastest operation and Scan gets called at least twice per
        // time loop
        private boolean _needsScan;
        // this is the algorithm under test
        protected final IAlgorithm Algorithm;
        private final ConcurrentMap<Integer, Order> _pending;
        private final object _needsScanLock = new object();

        /**
         * Creates a new BacktestingBrokerage for the specified algorithm
        */
         * @param algorithm The algorithm instance
        public BacktestingBrokerage(IAlgorithm algorithm)
            : base( "Backtesting Brokerage") {
            Algorithm = algorithm;
            _pending = new ConcurrentMap<Integer, Order>();
        }

        /**
         * Creates a new BacktestingBrokerage for the specified algorithm
        */
         * @param algorithm The algorithm instance
         * @param name The name of the brokerage
        protected BacktestingBrokerage(IAlgorithm algorithm, String name)
            : base(name) {
            Algorithm = algorithm;
            _pending = new ConcurrentMap<Integer, Order>();
        }

        /**
         * Gets the connection status
        */
         * 
         * The BacktestingBrokerage is always connected
         * 
        public @Override boolean IsConnected
        {
            get { return true; }
        }

        /**
         * Gets all open orders on the account
        */
        @returns The open orders returned from IB
        public @Override List<Order> GetOpenOrders() {
            return Algorithm.Transactions.GetOpenOrders();
        }

        /**
         * Gets all holdings for the account
        */
        @returns The current holdings from the account
        public @Override List<Holding> GetAccountHoldings() {
            // grab everything from the portfolio with a non-zero absolute quantity
            return (from security in Algorithm.Portfolio.Securities.Values.OrderBy(x -> x.Symbol) 
                    where security.Holdings.AbsoluteQuantity > 0 
                    select new Holding(security)).ToList();
        }

        /**
         * Gets the current cash balance for each currency held in the brokerage account
        */
        @returns The current cash balance for each currency available for trading
        public @Override List<Cash> GetCashBalance() {
            return Algorithm.Portfolio.CashBook.Values.ToList();
        }

        /**
         * Places a new order and assigns a new broker ID to the order
        */
         * @param order The order to be placed
        @returns True if the request for a new order has been placed, false otherwise
        public @Override boolean PlaceOrder(Order order) {
            if( order.Status == OrderStatus.New) {
                synchronized(_needsScanLock) {
                    _needsScan = true;
                    SetPendingOrder(order);
                }

                orderId = order.Id.toString();
                if( !order.BrokerId.Contains(orderId)) order.BrokerId.Add(orderId);

                // fire off the event that says this order has been submitted
                static final int orderFee = 0;
                submitted = new OrderEvent(order, Algorithm.UtcTime, orderFee) { Status = OrderStatus.Submitted };
                OnOrderEvent(submitted);

                return true;
            }
            return false;
        }

        /**
         * Updates the order with the same ID
        */
         * @param order The new order information
        @returns True if the request was made for the order to be updated, false otherwise
        public @Override boolean UpdateOrder(Order order) {
            if( true) {
                Order pending;
                if( !_pending.TryGetValue(order.Id, out pending)) {
                    // can't update something that isn't there
                    return false;
                }

                synchronized(_needsScanLock) {
                    _needsScan = true;
                    SetPendingOrder(order);
                }

                orderId = order.Id.toString();
                if( !order.BrokerId.Contains(orderId)) order.BrokerId.Add(orderId);

                // fire off the event that says this order has been updated
                static final int orderFee = 0;
                updated = new OrderEvent(order, Algorithm.UtcTime, orderFee) { Status = OrderStatus.Submitted };
                OnOrderEvent(updated);

                return true;
            }
        }

        /**
         * Cancels the order with the specified ID
        */
         * @param order The order to cancel
        @returns True if the request was made for the order to be canceled, false otherwise
        public @Override boolean CancelOrder(Order order) {
            Order pending;
            if( !_pending.TryRemove(order.Id, out pending)) {
                // can't cancel something that isn't there
                return false;
            }

            orderId = order.Id.toString();
            if( !order.BrokerId.Contains(orderId)) order.BrokerId.Add(order.Id.toString());

            // fire off the event that says this order has been canceled
            static final int orderFee = 0;
            canceled = new OrderEvent(order, Algorithm.UtcTime, orderFee) { Status = OrderStatus.Canceled };
            OnOrderEvent(canceled);

            return true;
        }

        /**
         * Scans all the outstanding orders and applies the algorithm model fills to generate the order events
        */
        public void Scan() {
            synchronized(_needsScanLock) {
                // there's usually nothing in here
                if( !_needsScan) {
                    return;
                }

                stillNeedsScan = false;

                // process each pending order to produce fills/fire events
                foreach (kvp in _pending.OrderBy(x -> x.Key)) {
                    order = kvp.Value;

                    if( order.Status.IsClosed()) {
                        // this should never actually happen as we always remove closed orders as they happen
                        _pending.TryRemove(order.Id, out order);
                        continue;
                    }

                    // all order fills are processed on the next bar (except for market orders)
                    if( order.Time == Algorithm.UtcTime && order.Type != OrderType.Market) {
                        stillNeedsScan = true;
                        continue;
                    }

                    fill = new OrderEvent(order, Algorithm.UtcTime, 0);

                    Security security;
                    if( !Algorithm.Securities.TryGetValue(order.Symbol, out security)) {
                        Log.Error( "BacktestingBrokerage.Scan(): Unable to process order: " + order.Id + ". The security no longer exists.");
                        // invalidate the order in the algorithm before removing
                        OnOrderEvent(new OrderEvent(order, Algorithm.UtcTime, BigDecimal.ZERO){Status = OrderStatus.Invalid});
                        _pending.TryRemove(order.Id, out order);
                        continue;
                    }

                    // check if we would actually be able to fill this
                    if( !Algorithm.BrokerageModel.CanExecuteOrder(security, order)) {
                        continue;
                    }

                    // verify sure we have enough cash to perform the fill
                    boolean sufficientBuyingPower;
                    try
                    {
                        sufficientBuyingPower = Algorithm.Transactions.GetSufficientCapitalForOrder(Algorithm.Portfolio, order);
                    }
                    catch (Exception err) {
                        // if we threw an error just mark it as invalid and remove the order from our pending list
                        Order pending;
                        _pending.TryRemove(order.Id, out pending);
                        order.Status = OrderStatus.Invalid;
                        OnOrderEvent(new OrderEvent(order, Algorithm.UtcTime, 0, "Error in GetSufficientCapitalForOrder"));

                        Log.Error(err);
                        Algorithm.Error( String.format( "Order Error: id: %1$s, Error executing margin models: %2$s", order.Id, err.Message));
                        continue;
                    }

                    //Before we check this queued order make sure we have buying power:
                    if( sufficientBuyingPower) {
                        //Model:
                        model = security.FillModel;

                        //Based on the order type: refresh its model to get fill price and quantity
                        try
                        {
                            switch (order.Type) {
                                case OrderType.Limit:
                                    fill = model.LimitFill(security, order as LimitOrder);
                                    break;

                                case OrderType.StopMarket:
                                    fill = model.StopMarketFill(security, order as StopMarketOrder);
                                    break;

                                case OrderType.Market:
                                    fill = model.MarketFill(security, order as MarketOrder);
                                    break;

                                case OrderType.StopLimit:
                                    fill = model.StopLimitFill(security, order as StopLimitOrder);
                                    break;

                                case OrderType.MarketOnOpen:
                                    fill = model.MarketOnOpenFill(security, order as MarketOnOpenOrder);
                                    break;

                                case OrderType.MarketOnClose:
                                    fill = model.MarketOnCloseFill(security, order as MarketOnCloseOrder);
                                    break;
                            }
                        }
                        catch (Exception err) {
                            Log.Error(err);
                            Algorithm.Error( String.format( "Order Error: id: %1$s, Transaction model failed to fill for order type: %2$s with error: %3$s",
                                order.Id, order.Type, err.Message));
                        }
                    }
                    else
                    {
                        //Flag order as invalid and push off queue:
                        order.Status = OrderStatus.Invalid;
                        Algorithm.Error( String.format( "Order Error: id: %1$s, Insufficient buying power to complete order (Value:%2$s).", order.Id,
                            order.GetValue(security).SmartRounding()));
                    }

                    // change in status or a new fill
                    if( order.Status != fill.Status || fill.FillQuantity != 0) {
                        //If the fill models come back suggesting filled, process the affects on portfolio
                        OnOrderEvent(fill);
                    }

                    if( fill.Status.IsClosed()) {
                        _pending.TryRemove(order.Id, out order);
                    }
                    else
                    {
                        stillNeedsScan = true;
                    }
                }
                
                // if we didn't fill then we need to continue to scan
                _needsScan = stillNeedsScan;
            }
        }

        /**
         * The BacktestingBrokerage is always connected. This is a no-op.
        */
        public @Override void Connect() {
            //NOP
        }

        /**
         * The BacktestingBrokerage is always connected. This is a no-op.
        */
        public @Override void Disconnect() {
            //NOP
        }

        /**
         * Sets the pending order as a clone to prevent object reference nastiness
        */
         * @param order The order to be added to the pending orders Map
        @returns 
        private void SetPendingOrder(Order order) {
            // only save off clones!
            _pending[order.Id] = order.Clone();
        }
    }
}