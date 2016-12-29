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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using QuantConnect.Brokerages.Oanda.DataType;
using QuantConnect.Brokerages.Oanda.DataType.Communications;
using QuantConnect.Brokerages.Oanda.Framework;
using QuantConnect.Orders;
using QuantConnect.Securities;
using Order = QuantConnect.Orders.Order;

package com.quantconnect.lean.Brokerages.Oanda
{
    /**
     * Oanda Brokerage - REST API related functions
    */
    public partial class OandaBrokerage
    {
        /**
         * Gets the list of available tradable instruments/products from Oanda
        */
        @returns 
        private List<Instrument> GetInstruments(List<String> instrumentNames = null ) {
            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.Rates) + "instruments?accountId=" + _accountId;
            if( instrumentNames != null ) {
                requestString += "&instruments=" + Uri.EscapeDataString( String.Join( ",", instrumentNames));
            }
            return MakeRequest<InstrumentsResponse>(requestString).instruments;
        }

        private static void PopulateOrderRequestParameters(Order order, Map<String,String> requestParams) {
            if( order.Direction != OrderDirection.Buy && order.Direction != OrderDirection.Sell) {
                throw new Exception( "Invalid Order Direction");
            }

            requestParams.Add( "side", order.Direction == OrderDirection.Buy ? "buy" : "sell");

            if( order.Type == OrderType.Market) {
                requestParams.Add( "type", "market");
            }

            if( order.Type == OrderType.Limit) {
                requestParams.Add( "type", "limit");
                requestParams.Add( "price", ((LimitOrder)order).LimitPrice.toString(CultureInfo.InvariantCulture));
                switch (order.Direction) {
                    case OrderDirection.Buy:
                        //Limit Order Does not like Lower Bound Values == Limit Price value
                        //Don't set bounds when placing limit orders. 
                        //Orders can be submitted with lower and upper bounds. If the market price on execution falls outside these bounds, it is considered a "Bounds Violation" and the order is cancelled.
                        break;

                    case OrderDirection.Sell:
                        //Limit Order Does not like Lower Bound Values == Limit Price value
                        //Don't set bounds when placing limit orders. 
                        //Orders can be submitted with lower and upper bounds. If the market price on execution falls outside these bounds, it is considered a "Bounds Violation" and the order is cancelled.
                        break;
                }

                //3 months is the max expiry for Oanda, and OrderDuration.GTC is only currently available
                requestParams.Add( "expiry", XmlConvert.toString(DateTime.Now.AddMonths(3), XmlDateTimeSerializationMode.Utc));
            }

            //this type should contain a stop and a limit to that stop.
            if( order.Type == OrderType.StopLimit) {
                requestParams.Add( "type", "stop");
                requestParams.Add( "price", ((StopLimitOrder)order).StopPrice.toString(CultureInfo.InvariantCulture));
                switch (order.Direction) {
                    case OrderDirection.Buy:
                        requestParams.Add( "upperBound", ((StopLimitOrder)order).LimitPrice.toString(CultureInfo.InvariantCulture));
                        break;
                    case OrderDirection.Sell:
                        requestParams.Add( "lowerBound", ((StopLimitOrder)order).LimitPrice.toString(CultureInfo.InvariantCulture));
                        break;
                }

                //3 months is the max expiry for Oanda, and OrderDuration.GTC is only currently available
                requestParams.Add( "expiry", XmlConvert.toString(DateTime.Now.AddMonths(3), XmlDateTimeSerializationMode.Utc));
            }

            if( order.Type == OrderType.StopMarket) {
                requestParams.Add( "type", "marketIfTouched");
                requestParams.Add( "price", ((StopMarketOrder)order).StopPrice.toString(CultureInfo.InvariantCulture));

                //3 months is the max expiry for Oanda, and OrderDuration.GTC is only currently available
                requestParams.Add( "expiry", XmlConvert.toString(DateTime.Now.AddMonths(3), XmlDateTimeSerializationMode.Utc));
            }
        }

        /**
         * Event handler for streaming events
        */
         * @param data The event object
        private void OnEventReceived(Event data) {
            if( data.IsHeartbeat()) {
                synchronized(_lockerConnectionMonitor) {
                    _lastHeartbeatUtcTime = DateTime.UtcNow;
                }
                return;
            }

            if( data.transaction != null ) {
                if( data.transaction.type.equals( "ORDER_FILLED") {
                    qcOrder = _orderProvider.GetOrderByBrokerageId(data.transaction.orderId);
                    qcOrder.PriceCurrency = _securityProvider.GetSecurity(qcOrder.Symbol).SymbolProperties.QuoteCurrency;

                    static final int orderFee = 0;
                    fill = new OrderEvent(qcOrder, DateTime.UtcNow, orderFee, "Oanda Fill Event") {
                        Status = OrderStatus.Filled,
                        FillPrice = (decimal)data.transaction.price,
                        FillQuantity = data.transaction.units
                    };

                    // flip the quantity on sell actions
                    if( qcOrder.Direction == OrderDirection.Sell) {
                        fill.FillQuantity *= -1;
                    }
                    OnOrderEvent(fill);
                }
            }
        }

        /**
         * Modify the specified order, updating it with the parameters provided
        */
         * @param orderId the identifier of the order to update
         * @param requestParams the parameters to update (name, value pairs)
        private void UpdateOrder(long orderId, Map<String,String> requestParams) {
            orderRequest = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + _accountId + "/orders/" + orderId;

            order = MakeRequest<DataType.Order>(orderRequest);
            if( order != null && order.id > 0) {
                requestString = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + _accountId + "/orders/" + orderId;
                MakeRequestWithBody<DataType.Order>(requestString, "PATCH", requestParams);
            }
            else
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "UpdateFailed", "Failed to update Oanda order id: " + orderId + "."));
                OnOrderEvent(new OrderEvent(ConvertOrder(order), DateTime.UtcNow, 0) {
                    Status = OrderStatus.Invalid,
                    Message = String.format( "Order currently does not exist with order id: %1$s.", orderId)
                });
            }
        }

        /**
         * Retrieves the current rate for each of a list of instruments
        */
         * @param instruments the list of instruments to check
        @returns List of Price objects with the current price for each instrument
        public List<Price> GetRates(List<String> instruments) {
            requestBuilder = new StringBuilder(EndpointResolver.ResolveEndpoint(_environment, Server.Rates) + "prices?instruments=");
            requestBuilder.Append( String.Join( ",", instruments));
            requestString = requestBuilder.toString().Replace( ",", "%2C");

            return MakeRequest<PricesResponse>(requestString).prices;
        }

        /**
         * Posts an order on the given account with the given parameters
        */
         * @param requestParams the parameters to use in the request
        @returns PostOrderResponse with details of the results (throws if if fails)
        private PostOrderResponse PostOrderAsync(Map<String,String> requestParams) {
            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + _accountId + "/orders";
            return MakeRequestWithBody<PostOrderResponse>(requestString, "POST", requestParams);
        }

        /**
         * Retrieves the list of open orders belonging to the account
        */
         * @param requestParams optional additional parameters for the request (name, value pairs)
        @returns List of Order objects (or empty list, if no orders)
        private List<DataType.Order> GetOrderList(Map<String,String> requestParams = null ) {
            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + _accountId + "/orders";
            ordersResponse = MakeRequest<OrdersResponse>(requestString, "GET", requestParams);
            orders = new List<DataType.Order>();
            orders.AddRange(ordersResponse.orders);
            return orders;
        }

        private void CancelOrder(long orderId) {
            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + _accountId + "/orders/" + orderId;
            MakeRequest<DataType.Order>(requestString, "DELETE");
        }

        private static Stream GetResponseStream(WebResponse response) {
            stream = response.GetResponseStream();
            if( response.Headers["Content-Encoding"].equals( "gzip") {	// if we received a gzipped response, handle that
                if( stream != null ) stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return stream;
        }

        /**
         * Initializes a streaming rates session with the given instruments on the given account
        */
         * @param instruments list of instruments to stream rates for
         * @param accountId the account ID you want to stream on
        @returns the WebResponse object that can be used to retrieve the rates as they stream
        public WebResponse StartRatesSession(List<Instrument> instruments, int accountId) {
            instrumentList = String.join( ",", instruments.Select(x -> x.instrument));

            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.StreamingRates) + 
                "prices?accountId=" + accountId + "&instruments=" + Uri.EscapeDataString(instrumentList);

            request = WebRequest.CreateHttp(requestString);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + _accessToken;

            try
            {
                response = request.GetResponse();
                return response;
            }
            catch (WebException ex) {
                response = (HttpWebResponse)ex.Response;
                stream = new StreamReader(response.GetResponseStream());
                result = stream.ReadToEnd();
                throw new Exception(result);
            }
        }
        
        /**
         * Initializes a streaming events session which will stream events for the given accounts
        */
         * @param accountId the account IDs you want to stream on
        @returns the WebResponse object that can be used to retrieve the events as they stream
        public WebResponse StartEventsSession(List<Integer> accountId = null ) {
            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.StreamingEvents) + "events";

            if( accountId != null && accountId.Count > 0) {
                accountIds = String.join( ",", accountId);
                requestString += "?accountIds=" + WebUtility.UrlEncode(accountIds);
            }

            request = WebRequest.CreateHttp(requestString);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + _accessToken;

            try
            {
                response = request.GetResponse();
                return response;
            }
            catch (WebException ex) {
                response = (HttpWebResponse)ex.Response;
                stream = new StreamReader(response.GetResponseStream());
                result = stream.ReadToEnd();
                throw new Exception(result);
            }
        }

        /**
         * Primary (internal) request handler
        */
         * <typeparam name="T The response type</typeparam>
         * @param requestString the request to make
         * @param method method for the request (defaults to GET)
         * @param requestParams optional parameters (note that if provided, it's assumed the requestString doesn't contain any)
        @returns response via type T
        private T MakeRequest<T>( String requestString, String method = "GET", Map<String,String> requestParams = null ) {
            if( requestParams != null && requestParams.Count > 0) {
                parameters = CreateParamString(requestParams);
                requestString = requestString + "?" + parameters;
            }
            request = WebRequest.CreateHttp(requestString);
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + _accessToken;
            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            request.Method = method;

            try
            {
                using (response = request.GetResponse()) {
                    serializer = new DataContractJsonSerializer(typeof(T));
                    stream = GetResponseStream(response);
                    return (T)serializer.ReadObject(stream);
                }
            }
            catch (WebException ex) {
                stream = GetResponseStream(ex.Response);
                reader = new StreamReader(stream);
                result = reader.ReadToEnd();
                throw new Exception(result);
            }
        }

        /**
         * Secondary (internal) request handler. differs from primary in that parameters are placed in the body instead of the request string
        */
         * <typeparam name="T response type</typeparam>
         * @param method method to use (usually POST or PATCH)
         * @param requestParams the parameters to pass in the request body
         * @param requestString the request to make
        @returns response, via type T
        private T MakeRequestWithBody<T>( String requestString, String method, Map<String,String> requestParams) {
            // Create the body
            requestBody = CreateParamString(requestParams);
            request = WebRequest.CreateHttp(requestString);
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + _accessToken;
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";

            using (writer = new StreamWriter(request.GetRequestStream())) {
                // Write the body
                writer.WriteAsync(requestBody);
            }

            // Handle the response
            try
            {
                using (response = request.GetResponse()) {
                    serializer = new DataContractJsonSerializer(typeof(T));
                    return (T)serializer.ReadObject(response.GetResponseStream());
                }
            }
            catch (WebException ex) {
                response = (HttpWebResponse)ex.Response;
                stream = new StreamReader(response.GetResponseStream());
                result = stream.ReadToEnd();
                throw new Exception(result);
            }
        }

        /**
         * Retrieves the current non-zero positions for a given account
        */
         * @param accountId positions will be retrieved for this account id
        @returns List of Position objects with the details for each position (or empty list iff no positions)
        private List<Position> GetPositions(int accountId) {
            requestString = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + accountId + "/positions";
            positionResponse = MakeRequest<PositionsResponse>(requestString);
            positions = new List<Position>();
            positions.AddRange(positionResponse.positions);
            return positions;
        }

        /**
         * Helper function to create the parameter String out of a dictionary of parameters
        */
         * @param requestParams the parameters to convert
        @returns string containing all the parameters for use in requests
        private static String CreateParamString(Map<String,String> requestParams) {
            return String.join( "&", requestParams.Select(x -> WebUtility.UrlEncode(x.Key) + "=" + WebUtility.UrlEncode(x.Value)));
        }

        /**
         * Converts the specified Oanda order into a qc order.
         * The 'task' will have a value if we needed to issue a rest call for the stop price, otherwise it will be null
        */
        private Order ConvertOrder(DataType.Order order) {
            Order qcOrder;
            switch (order.type) {
                case "limit":
                    qcOrder = new LimitOrder();
                    if( order.side.equals( "buy") {
                        ((LimitOrder)qcOrder).LimitPrice = new BigDecimal( order.lowerBound);
                    }

                    if( order.side.equals( "sell") {
                        ((LimitOrder)qcOrder).LimitPrice = new BigDecimal( order.upperBound);
                    }

                    break;
                case "stop":
                    qcOrder = new StopLimitOrder();
                    if( order.side.equals( "buy") {
                        ((StopLimitOrder)qcOrder).LimitPrice = new BigDecimal( order.lowerBound);
                    }

                    if( order.side.equals( "sell") {
                        ((StopLimitOrder)qcOrder).LimitPrice = new BigDecimal( order.upperBound);
                    }
                    break;
                case "marketIfTouched":
                    //when market reaches the price sell at market.
                    qcOrder = new StopMarketOrder { Price = new BigDecimal( order.price), StopPrice = new BigDecimal( order.price) };
                    break;
                case "market":
                    qcOrder = new MarketOrder();
                    break;

                default:
                    throw new NotSupportedException( "The Oanda order type " + order.type + " is not supported.");
            }
            securityType = _symbolMapper.GetBrokerageSecurityType(order.instrument);
            qcOrder.Symbol = _symbolMapper.GetLeanSymbol(order.instrument, securityType, Market.Oanda);
            qcOrder.Quantity = ConvertQuantity(order);
            qcOrder.Status = OrderStatus.None;
            qcOrder.BrokerId.Add(order.id.toString());
            orderByBrokerageId = _orderProvider.GetOrderByBrokerageId(order.id);
            if( orderByBrokerageId != null ) {
                qcOrder.Id = orderByBrokerageId.Id;
            }
            qcOrder.Duration = OrderDuration.Custom;
            qcOrder.DurationValue = XmlConvert.ToDateTime(order.expiry, XmlDateTimeSerializationMode.Utc);
            qcOrder.Time = XmlConvert.ToDateTime(order.time, XmlDateTimeSerializationMode.Utc);

            return qcOrder;
        }

        /**
         * Converts the Oanda order quantity into a qc quantity
        */
         * 
         * Oanda quantities are always positive and use the direction to denote +/-, where as qc
         * order quantities determine the direction
         * 
        private int ConvertQuantity(DataType.Order order) {
            switch (order.side) {
                case "buy":
                    return order.units;

                case "sell":
                    return -order.units;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /**
         * Converts the Oanda position into a QuantConnect holding.
        */
         * @param position The position.
        @returns 
        private Holding ConvertHolding(Position position) {
            securityType = _symbolMapper.GetBrokerageSecurityType(position.instrument);

            return new Holding
            {
                Symbol = _symbolMapper.GetLeanSymbol(position.instrument, securityType, Market.Oanda),
                Class = securityType,
                AveragePrice = (decimal)position.avgPrice,
                ConversionRate = 1.0m,
                CurrencySymbol = "$",
                Quantity = position.side.equals( "sell" ? -position.units : position.units
            };
        }

        /**
         * Gets the current conversion rate into USD
        */
         * Synchronous, blocking
        private BigDecimal GetUsdConversion( String currency) {
            if( currency.equals( "USD")
                return 1m;

            // determine the correct symbol to choose
            normalSymbol = currency + "_USD";
            invertedSymbol = "USD_" + currency;
            isInverted = _oandaInstruments.ContainsKey(invertedSymbol);
            oandaSymbol = isInverted ? invertedSymbol : normalSymbol;

            quote = GetRates(new List<String> { oandaSymbol }).First();
            rate = (decimal)(quote.bid + quote.ask) / 2;

            return isInverted ? 1 / rate : rate;
        }

    }
}
