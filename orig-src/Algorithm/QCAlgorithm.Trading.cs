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
using System.Linq;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Forex;

package com.quantconnect.lean.Algorithm
{
    public partial class QCAlgorithm
    {
        private int _maxOrders = 10000;

        /**
        /// Transaction Manager - Process transaction fills and order management.
        */
        public SecurityTransactionManager Transactions { get; set; }

        /**
        /// Buy Stock (Alias of Order)
        */
         * @param symbol">string Symbol of the asset to trade
         * @param quantity">int Quantity of the asset to trade
        /// <seealso cref="Buy(Symbol, double)"/>
        public OrderTicket Buy(Symbol symbol, int quantity) {
            return Order(symbol, Math.Abs(quantity));
        }

        /**
        /// Buy Stock (Alias of Order)
        */
         * @param symbol">string Symbol of the asset to trade
         * @param quantity">double Quantity of the asset to trade
        /// <seealso cref="Buy(Symbol, int)"/>
        public OrderTicket Buy(Symbol symbol, double quantity) {
            return Order(symbol, Math.Abs(quantity));
        }

        /**
        /// Buy Stock (Alias of Order)
        */
         * @param symbol">string Symbol of the asset to trade
         * @param quantity">decimal Quantity of the asset to trade
        /// <seealso cref="Order(Symbol, double)"/>
        public OrderTicket Buy(Symbol symbol, BigDecimal quantity) {
            return Order(symbol, Math.Abs(quantity));
        }

        /**
        /// Buy Stock (Alias of Order)
        */
         * @param symbol">string Symbol of the asset to trade
         * @param quantity">float Quantity of the asset to trade
        /// <seealso cref="Buy(Symbol, double)"/>
        public OrderTicket Buy(Symbol symbol, float quantity) {
            return Order(symbol, Math.Abs(quantity));
        }

        /**
        /// Sell stock (alias of Order)
        */
         * @param symbol">string Symbol of the asset to trade
         * @param quantity">int Quantity of the asset to trade
        /// <seealso cref="Sell(Symbol, double)"/>
        public OrderTicket Sell(Symbol symbol, int quantity) {
            return Order(symbol, Math.Abs(quantity) * -1);
        }

        /**
        /// Sell stock (alias of Order)
        */
         * @param symbol">String symbol to sell
         * @param quantity">Quantity to order
        @returns int Order Id.
        public OrderTicket Sell(Symbol symbol, double quantity) {
            return Order(symbol, Math.Abs(quantity) * -1);
        }

        /**
        /// Sell stock (alias of Order)
        */
         * @param symbol">String symbol
         * @param quantity">Quantity to sell
        @returns int order id
        /// <seealso cref="Sell(Symbol, double)"/>
        public OrderTicket Sell(Symbol symbol, float quantity) {
            return Order(symbol, Math.Abs(quantity) * -1);
        }

        /**
        /// Sell stock (alias of Order)
        */
         * @param symbol">String symbol to sell
         * @param quantity">Quantity to sell
        @returns Int Order Id.
        public OrderTicket Sell(Symbol symbol, BigDecimal quantity) {
            return Order(symbol, Math.Abs(quantity) * -1);
        }

        /**
        /// Issue an order/trade for asset: Alias wrapper for Order( String, int);
        */
        /// <seealso cref="Order(Symbol, decimal)"/>
        public OrderTicket Order(Symbol symbol, double quantity) {
            return Order(symbol, (int) quantity);
        }

        /**
        /// Issue an order/trade for asset: Alias wrapper for Order( String, int);
        */
        /// 
        /// <seealso cref="Order(Symbol, double)"/>
        public OrderTicket Order(Symbol symbol, BigDecimal quantity) {
            return Order(symbol, (int) quantity);
        }

        /**
        /// Wrapper for market order method: submit a new order for quantity of symbol using type order.
        */
         * @param symbol">Symbol of the MarketType Required.
         * @param quantity">Number of shares to request.
         * @param asynchronous">Send the order asynchrously (false). Otherwise we'll block until it fills
         * @param tag">Place a custom order property or tag (e.g. indicator data).
        /// <seealso cref="MarketOrder(Symbol, int, bool, string)"/>
        public OrderTicket Order(Symbol symbol, int quantity, boolean asynchronous = false, String tag = "") {
            return MarketOrder(symbol, quantity, asynchronous, tag);
        }

        /**
        /// Market order implementation: Send a market order and wait for it to be filled.
        */
         * @param symbol">Symbol of the MarketType Required.
         * @param quantity">Number of shares to request.
         * @param asynchronous">Send the order asynchrously (false). Otherwise we'll block until it fills
         * @param tag">Place a custom order property or tag (e.g. indicator data).
        @returns int Order id
        public OrderTicket MarketOrder(Symbol symbol, int quantity, boolean asynchronous = false, String tag = "") {
            security = Securities[symbol];

            // check the exchange is open before sending a market order, if it's not open
            // then convert it into a market on open order
            if( !security.Exchange.ExchangeOpen) {
                mooTicket = MarketOnOpenOrder(security.Symbol, quantity, tag);
                anyNonDailySubscriptions = security.Subscriptions.Any(x -> x.Resolution != Resolution.Daily);
                if( mooTicket.SubmitRequest.Response.IsSuccess && !anyNonDailySubscriptions) {
                    Debug( "Converted OrderID: " + mooTicket.OrderId + " into a MarketOnOpen order.");
                }   
                return mooTicket;
            }

            request = CreateSubmitOrderRequest(OrderType.Market, security, quantity, tag);

            //Initialize the Market order parameters:
            preOrderCheckResponse = PreOrderChecks(request);
            if( preOrderCheckResponse.IsError) {
                return OrderTicket.InvalidSubmitRequest(Transactions, request, preOrderCheckResponse);
            }

            //Add the order and create a new order Id.
            ticket = Transactions.AddOrder(request);

            // Wait for the order event to process, only if the exchange is open
            if( !asynchronous) {
                Transactions.WaitForOrder(ticket.OrderId);
            }

            return ticket;
        }

        /**
        /// Market on open order implementation: Send a market order when the exchange opens
        */
         * @param symbol">The symbol to be ordered
         * @param quantity">The number of shares to required
         * @param tag">Place a custom order property or tag (e.g. indicator data).
        @returns The order ID
        public OrderTicket MarketOnOpenOrder(Symbol symbol, int quantity, String tag = "") {
            security = Securities[symbol];
            request = CreateSubmitOrderRequest(OrderType.MarketOnOpen, security, quantity, tag);
            response = PreOrderChecks(request);
            if( response.IsError) {
                return OrderTicket.InvalidSubmitRequest(Transactions, request, response);
            }

            return Transactions.AddOrder(request);
        }

        /**
        /// Market on close order implementation: Send a market order when the exchange closes
        */
         * @param symbol">The symbol to be ordered
         * @param quantity">The number of shares to required
         * @param tag">Place a custom order property or tag (e.g. indicator data).
        @returns The order ID
        public OrderTicket MarketOnCloseOrder(Symbol symbol, int quantity, String tag = "") {
            security = Securities[symbol];
            request = CreateSubmitOrderRequest(OrderType.MarketOnClose, security, quantity, tag);
            response = PreOrderChecks(request);
            if( response.IsError) {
                return OrderTicket.InvalidSubmitRequest(Transactions, request, response);
            }

            return Transactions.AddOrder(request);
        }

        /**
        /// Send a limit order to the transaction handler:
        */
         * @param symbol">String symbol for the asset
         * @param quantity">Quantity of shares for limit order
         * @param limitPrice">Limit price to fill this order
         * @param tag">String tag for the order (optional)
        @returns Order id
        public OrderTicket LimitOrder(Symbol symbol, int quantity, BigDecimal limitPrice, String tag = "") {
            security = Securities[symbol];
            request = CreateSubmitOrderRequest(OrderType.Limit, security, quantity, tag, limitPrice: limitPrice);
            response = PreOrderChecks(request);
            if( response.IsError) {
                return OrderTicket.InvalidSubmitRequest(Transactions, request, response);
            }

            return Transactions.AddOrder(request);
        }

        /**
        /// Create a stop market order and return the newly created order id; or negative if the order is invalid
        */
         * @param symbol">String symbol for the asset we're trading
         * @param quantity">Quantity to be traded
         * @param stopPrice">Price to fill the stop order
         * @param tag">Optional String data tag for the order
        @returns Int orderId for the new order.
        public OrderTicket StopMarketOrder(Symbol symbol, int quantity, BigDecimal stopPrice, String tag = "") {
            security = Securities[symbol];
            request = CreateSubmitOrderRequest(OrderType.StopMarket, security, quantity, tag, stopPrice: stopPrice);
            response = PreOrderChecks(request);
            if( response.IsError) {
                return OrderTicket.InvalidSubmitRequest(Transactions, request, response);
            }

            return Transactions.AddOrder(request);
        }

        /**
        /// Send a stop limit order to the transaction handler:
        */
         * @param symbol">String symbol for the asset
         * @param quantity">Quantity of shares for limit order
         * @param stopPrice">Stop price for this order
         * @param limitPrice">Limit price to fill this order
         * @param tag">String tag for the order (optional)
        @returns Order id
        public OrderTicket StopLimitOrder(Symbol symbol, int quantity, BigDecimal stopPrice, BigDecimal limitPrice, String tag = "") {
            security = Securities[symbol];
            request = CreateSubmitOrderRequest(OrderType.StopLimit, security, quantity, tag, stopPrice: stopPrice, limitPrice: limitPrice);
            response = PreOrderChecks(request);
            if( response.IsError) {
                return OrderTicket.InvalidSubmitRequest(Transactions, request, response);
            }

            //Add the order and create a new order Id.
            return Transactions.AddOrder(request);
        }

        /**
        /// Perform preorder checks to ensure we have sufficient capital, 
        /// the market is open, and we haven't exceeded maximum realistic orders per day.
        */
        @returns OrderResponse. If no error, order request is submitted.
        private OrderResponse PreOrderChecks(SubmitOrderRequest request) {
            response = PreOrderChecksImpl(request);
            if( response.IsError) {
                Error(response.ErrorMessage);
            }
            return response;
        }

        /**
        /// Perform preorder checks to ensure we have sufficient capital, 
        /// the market is open, and we haven't exceeded maximum realistic orders per day.
        */
        @returns OrderResponse. If no error, order request is submitted.
        private OrderResponse PreOrderChecksImpl(SubmitOrderRequest request) {
            //Most order methods use security objects; so this isn't really used. 
            // todo: Left here for now but should review 
            Security security;
            if( !Securities.TryGetValue(request.Symbol, out security)) {
                return OrderResponse.Error(request, OrderResponseErrorCode.MissingSecurity, "You haven't requested " + request.Symbol.toString() + " data. Add this with AddSecurity() in the Initialize() Method.");
            }

            //Ordering 0 is useless.
            if( request.Quantity == 0 || request.Symbol == null || request.Symbol == QuantConnect.Symbol.Empty || Math.Abs(request.Quantity) < security.SymbolProperties.LotSize) {
                return OrderResponse.ZeroQuantity(request);
            }

            if( !security.IsTradable) {
                return OrderResponse.Error(request, OrderResponseErrorCode.NonTradableSecurity, "The security with symbol '" + request.Symbol.toString() + "' is marked as non-tradable.");
            }

            price = security.Price;

            //Check the exchange is open before sending a market on close orders
            if( request.OrderType == OrderType.MarketOnClose && !security.Exchange.ExchangeOpen) {
                return OrderResponse.Error(request, OrderResponseErrorCode.ExchangeNotOpen, request.OrderType + " order and exchange not open.");
            }
            
            if( price == 0) {
                return OrderResponse.Error(request, OrderResponseErrorCode.SecurityPriceZero, request.Symbol.toString() + ": asset price is $0. If using custom data make sure you've set the 'Value' property.");
            }

            // check quote currency existence/conversion rate on all orders
            Cash quoteCash;
            quoteCurrency = security.QuoteCurrency.Symbol;
            if( !Portfolio.CashBook.TryGetValue(quoteCurrency, out quoteCash)) {
                return OrderResponse.Error(request, OrderResponseErrorCode.QuoteCurrencyRequired, request.Symbol.Value + ": requires " + quoteCurrency + " in the cashbook to trade.");
            }
            if( security.QuoteCurrency.ConversionRate == 0m) {
                return OrderResponse.Error(request, OrderResponseErrorCode.ConversionRateZero, request.Symbol.Value + ": requires " + quoteCurrency + " to have a non-zero conversion rate. This can be caused by lack of data.");
            }
            
            // need to also check base currency existence/conversion rate on forex orders
            if( security.Type == SecurityType.Forex) {
                Cash baseCash;
                baseCurrency = ((Forex) security).BaseCurrencySymbol;
                if( !Portfolio.CashBook.TryGetValue(baseCurrency, out baseCash)) {
                    return OrderResponse.Error(request, OrderResponseErrorCode.ForexBaseAndQuoteCurrenciesRequired, request.Symbol.Value + ": requires " + baseCurrency + " and " + quoteCurrency + " in the cashbook to trade.");
                }
                if( baseCash.ConversionRate == 0m) {
                    return OrderResponse.Error(request, OrderResponseErrorCode.ForexConversionRateZero, request.Symbol.Value + ": requires " + baseCurrency + " and " + quoteCurrency + " to have non-zero conversion rates. This can be caused by lack of data.");
                }
            }
            
            //Make sure the security has some data:
            if( !security.HasData) {
                return OrderResponse.Error(request, OrderResponseErrorCode.SecurityHasNoData, "There is no data for this symbol yet, please check the security.HasData flag to ensure there is at least one data point.");
            }
            
            //We've already processed too many orders: max 100 per day or the memory usage explodes
            if( Transactions.OrdersCount > _maxOrders) {
                Status = AlgorithmStatus.Stopped;
                return OrderResponse.Error(request, OrderResponseErrorCode.ExceededMaximumOrders, String.format( "You have exceeded maximum number of orders (%1$s), for unlimited orders upgrade your account.", _maxOrders));
            }
            
            if( request.OrderType == OrderType.MarketOnClose) {
                nextMarketClose = security.Exchange.Hours.GetNextMarketClose(security.LocalTime, false);
                // must be submitted with at least 10 minutes in trading day, add buffer allow order submission
                latestSubmissionTime = nextMarketClose.AddMinutes(-15.50);
                if( !security.Exchange.ExchangeOpen || Time > latestSubmissionTime) {
                    // tell the user we require a 16 minute buffer, on minute data in live a user will receive the 3:44->3:45 bar at 3:45,
                    // this is already too late to submit one of these orders, so make the user do it at the 3:43->3:44 bar so it's submitted
                    // to the brokerage before 3:45.
                    return OrderResponse.Error(request, OrderResponseErrorCode.MarketOnCloseOrderTooLate, "MarketOnClose orders must be placed with at least a 16 minute buffer before market close.");
                }
            }

            // passes all initial order checks
            return OrderResponse.Success(request);
        }

        /**
        /// Liquidate all holdings and cancel open orders. Called at the end of day for tick-strategies.
        */
         * @param symbolToLiquidate">Symbols we wish to liquidate
        @returns Array of order ids for liquidated symbols
        /// <seealso cref="MarketOrder"/>
        public List<Integer> Liquidate(Symbol symbolToLiquidate = null ) {
            orderIdList = new List<Integer>();
            symbolToLiquidate = symbolToLiquidate ?? QuantConnect.Symbol.Empty;

            foreach (symbol in Securities.Keys.OrderBy(x -> x.Value)) {
                // symbol not matching, do nothing
                if( symbol != symbolToLiquidate && symbolToLiquidate != QuantConnect.Symbol.Empty) 
                    continue;

                // get open orders
                orders = Transactions.GetOpenOrders(symbol);

                // get quantity in portfolio
                quantity = Portfolio[symbol].Quantity;

                // if there is only one open market order that would close the position, do nothing
                if( orders.Count == 1 && quantity != 0 && orders[0].Quantity == -quantity && orders[0].Type == OrderType.Market)
                    continue;

                // cancel all open orders
                marketOrdersQuantity = 0m;
                foreach (order in orders) {
                    if( order.Type == OrderType.Market) {
                        // pending market order
                        ticket = Transactions.GetOrderTicket(order.Id);
                        if( ticket != null ) {
                            // get remaining quantity
                            marketOrdersQuantity += ticket.Quantity - ticket.QuantityFilled;
                        }
                    }
                    else
                    {
                        Transactions.CancelOrder(order.Id);
                    }
                }

                // Liquidate at market price
                if( quantity != 0) {
                    // calculate quantity for closing market order
                    ticket = Order(symbol, -quantity - marketOrdersQuantity);
                    if( ticket.Status == OrderStatus.Filled) {
                        orderIdList.Add(ticket.OrderId);
                    }
                }
            }

            return orderIdList;
        }

        /**
        /// Maximum number of orders for the algorithm
        */
         * @param max">
        public void SetMaximumOrders(int max) {
            if( !_locked) {
                _maxOrders = max;
            }
        }

        /**
        /// Alias for SetHoldings to avoid the M-decimal errors.
        */
         * @param symbol">string symbol we wish to hold
         * @param percentage">double percentage of holdings desired
         * @param liquidateExistingHoldings">liquidate existing holdings if neccessary to hold this stock
        /// <seealso cref="MarketOrder"/>
        public void SetHoldings(Symbol symbol, double percentage, boolean liquidateExistingHoldings = false) {
            SetHoldings(symbol, (decimal)percentage, liquidateExistingHoldings);
        }

        /**
        /// Alias for SetHoldings to avoid the M-decimal errors.
        */
         * @param symbol">string symbol we wish to hold
         * @param percentage">float percentage of holdings desired
         * @param liquidateExistingHoldings">bool liquidate existing holdings if neccessary to hold this stock
         * @param tag">Tag the order with a short string.
        /// <seealso cref="MarketOrder"/>
        public void SetHoldings(Symbol symbol, float percentage, boolean liquidateExistingHoldings = false, String tag = "") {
            SetHoldings(symbol, (decimal)percentage, liquidateExistingHoldings, tag);
        }

        /**
        /// Alias for SetHoldings to avoid the M-decimal errors.
        */
         * @param symbol">string symbol we wish to hold
         * @param percentage">float percentage of holdings desired
         * @param liquidateExistingHoldings">bool liquidate existing holdings if neccessary to hold this stock
         * @param tag">Tag the order with a short string.
        /// <seealso cref="MarketOrder"/>
        public void SetHoldings(Symbol symbol, int percentage, boolean liquidateExistingHoldings = false, String tag = "") {
            SetHoldings(symbol, (decimal)percentage, liquidateExistingHoldings, tag);
        }

        /**
        /// Automatically place an order which will set the holdings to between 100% or -100% of *PORTFOLIO VALUE*.
        /// E.g. SetHoldings( "AAPL", 0.1); SetHoldings( "IBM", -0.2); -> Sets portfolio as long 10% APPL and short 20% IBM
        /// E.g. SetHoldings( "AAPL", 2); -> Sets apple to 2x leveraged with all our cash.
        */
         * @param symbol">Symbol indexer
         * @param percentage">decimal fraction of portfolio to set stock
         * @param liquidateExistingHoldings">bool flag to clean all existing holdings before setting new faction.
         * @param tag">Tag the order with a short string.
        /// <seealso cref="MarketOrder"/>
        public void SetHoldings(Symbol symbol, BigDecimal percentage, boolean liquidateExistingHoldings = false, String tag = "") {
            //Initialize Requirements:
            Security security;
            if( !Securities.TryGetValue(symbol, out security)) {
                Error(symbol.toString() + " not found in portfolio. Request this data when initializing the algorithm.");
                return;
            }

            //If they triggered a liquidate
            if( liquidateExistingHoldings) {
                foreach (kvp in Portfolio) {
                    holdingSymbol = kvp.Key;
                    holdings = kvp.Value;
                    if( holdingSymbol != symbol && holdings.AbsoluteQuantity > 0) {
                        //Go through all existing holdings [synchronously], market order the inverse quantity:
                        Order(holdingSymbol, -holdings.Quantity, false, tag);
                    }
                }
            }

            //Only place trade if we've got > 1 share to order.
            quantity = CalculateOrderQuantity(symbol, percentage);
            if( Math.Abs(quantity) > 0) {
                MarketOrder(symbol, quantity, false, tag);
            }
        }

        /**
        /// Calculate the order quantity to achieve target-percent holdings.
        */
         * @param symbol">Security object we're asking for
         * @param target">Target percentag holdings
        @returns Order quantity to achieve this percentage
        public int CalculateOrderQuantity(Symbol symbol, double target) {
            return CalculateOrderQuantity(symbol, (decimal)target);
        }

        /**
        /// Calculate the order quantity to achieve target-percent holdings.
        */
         * @param symbol">Security object we're asking for
         * @param target">Target percentag holdings, this is an unlevered value, so 
        /// if you have 2x leverage and request 100% holdings, it will utilize half of the 
        /// available margin
        @returns Order quantity to achieve this percentage
        public int CalculateOrderQuantity(Symbol symbol, BigDecimal target) {
            security = Securities[symbol];
            price = security.Price;

            // can't order it if we don't have data
            if( price == 0) return 0;

            // if targeting zero, simply return the negative of the quantity
            if( target == 0) return -security.Holdings.Quantity;

            // this is the value in dollars that we want our holdings to have
            targetPortfolioValue = target*Portfolio.TotalPortfolioValue;
            quantity = security.Holdings.Quantity;
            currentHoldingsValue = price*quantity;

            // remove directionality, we'll work in the land of absolutes
            targetOrderValue = Math.Abs(targetPortfolioValue - currentHoldingsValue);
            direction = targetPortfolioValue > currentHoldingsValue ? OrderDirection.Buy : OrderDirection.Sell;

            // determine the unit price in terms of the account currency
            unitPrice = new MarketOrder(symbol, 1, UtcTime).GetValue(security);

            // calculate the total margin available
            marginRemaining = Portfolio.GetMarginRemaining(symbol, direction);
            if( marginRemaining <= 0) return 0;

            // continue iterating while we do not have enough margin for the order
            BigDecimal marginRequired;
            BigDecimal orderValue;
            BigDecimal orderFees;
            feeToPriceRatio = 0;

            // compute the initial order quantity
            orderQuantity = (int)(targetOrderValue / unitPrice);
            iterations = 0;

            do
            {
                // decrease the order quantity
                if( iterations > 0) {
                    // if fees are high relative to price, we reduce the order quantity faster
                    if( feeToPriceRatio > 0)
                        orderQuantity -= feeToPriceRatio;
                    else
                        orderQuantity--;
                }

                // generate the order
                order = new MarketOrder(security.Symbol, orderQuantity, UtcTime);
                orderValue = order.GetValue(security);
                orderFees = security.FeeModel.GetOrderFee(security, order);
                feeToPriceRatio = (int)(orderFees / unitPrice);

                // calculate the margin required for the order
                marginRequired = security.MarginModel.GetInitialMarginRequiredForOrder(security, order);

                iterations++;

            } while (orderQuantity > 0 && (marginRequired > marginRemaining || orderValue + orderFees > targetOrderValue));

            //Rounding off Order Quantity to the nearest multiple of Lot Size
            if( orderQuantity %  Integer.parseInt( security.SymbolProperties.LotSize) != 0) {
                orderQuantity = orderQuantity - (orderQuantity %  Integer.parseInt( security.SymbolProperties.LotSize));
            }

            // add directionality back in
            return (direction == OrderDirection.Sell ? -1 : 1) * orderQuantity;
        }

        /**
        /// Obsolete implementation of Order method accepting a OrderType. This was deprecated since it 
        /// was impossible to generate other orders via this method. Any calls to this method will always default to a Market Order.
        */
         * @param symbol">Symbol we want to purchase
         * @param quantity">Quantity to buy, + is long, - short.
         * @param type">Order Type
         * @param asynchronous">Don't wait for the response, just submit order and move on.
         * @param tag">Custom data for this order
        @returns Integer Order ID.
        [Obsolete( "This Order method has been made obsolete, use Order( String, int, bool, string) method instead. Calls to the obsolete method will only generate market orders.")]
        public OrderTicket Order(Symbol symbol, int quantity, OrderType type, boolean asynchronous = false, String tag = "") {
            return Order(symbol, quantity, asynchronous, tag);
        }

        /**
        /// Obsolete method for placing orders. 
        */
         * @param symbol">
         * @param quantity">
         * @param type">
        [Obsolete( "This Order method has been made obsolete, use the specialized Order helper methods instead. Calls to the obsolete method will only generate market orders.")]
        public OrderTicket Order(Symbol symbol, BigDecimal quantity, OrderType type) {
            return Order(symbol, (int)quantity);
        }

        /**
        /// Obsolete method for placing orders.
        */
         * @param symbol">
         * @param quantity">
         * @param type">
        [Obsolete( "This Order method has been made obsolete, use the specialized Order helper methods instead. Calls to the obsolete method will only generate market orders.")]
        public OrderTicket Order(Symbol symbol, int quantity, OrderType type) {
            return Order(symbol, quantity);
        }

        private SubmitOrderRequest CreateSubmitOrderRequest(OrderType orderType, Security security, int quantity, String tag, BigDecimal stopPrice = 0m, BigDecimal limitPrice = 0m) {
            return new SubmitOrderRequest(orderType, security.Type, security.Symbol, quantity, stopPrice, limitPrice, UtcTime, tag);
        }
    }
}
