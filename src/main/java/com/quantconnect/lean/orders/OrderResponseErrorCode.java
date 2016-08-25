/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.quantconnect.lean.orders;

/**
 * Error detail code
 */
public enum OrderResponseErrorCode {
    /**
     * No error
     */
    None( 0 ),

    /**
     * Unknown error
     */
    ProcessingError( -1 ),

    /**
     * Cannot submit because order already exists
     */
    OrderAlreadyExists( -2 ),

    /**
     * Not enough money to to submit order
     */
    InsufficientBuyingPower( -3 ),

    /**
     * Internal logic invalidated submit order
     */
    BrokerageModelRefusedToSubmitOrder( -4 ),

    /**
     * Brokerage submit error
     */
    BrokerageFailedToSubmitOrder( -5 ),

    /**
     * Brokerage update error
     */
    BrokerageFailedToUpdateOrder( -6 ),

    /**
     * Internal logic invalidated update order
     */
    BrokerageHandlerRefusedToUpdateOrder( -7 ),

    /**
     * Brokerage cancel error
     */
    BrokerageFailedToCancelOrder( -8 ),

    /**
     * Only pending orders can be canceled
     */
    InvalidOrderStatus( -9 ),

    /**
     * Missing order
     */
    UnableToFindOrder( -10 ),

    /**
     * Cannot submit or update orders with zero quantity
     */
    OrderQuantityZero( -11 ),

    /**
     * This type of request is unsupported
     */
    UnsupportedRequestType( -12 ),

    /**
     * Unknown error during pre order request validation
     */
    PreOrderChecksError( -13 ),

    /**
     * Security is missing. Probably did not subscribe.
     */
    MissingSecurity( -14 ),

    /**
     * Some order types require open exchange
     */
    ExchangeNotOpen( -15 ),

    /**
     * Zero security price is probably due to bad data
     */
    SecurityPriceZero( -16 ),

    /**
     * Need both currencies in cashbook to trade a pair
     */
    ForexBaseAndQuoteCurrenciesRequired( -17 ),

    /**
     * Need conversion rate to account currency
     */
    ForexConversionRateZero( -18 ),

    /**
     * Should not attempt trading without at least one data point
     */
    SecurityHasNoData( -19 ),

    /**
     * Transaction manager's cache is full
     */
    ExceededMaximumOrders( -20 ),

    /**
     * Need 11 minute buffer before exchange close
     */
    MarketOnCloseOrderTooLate( -21 ),

    /**
     * Request is invalid or null
     */
    InvalidRequest( -22 ),

    /**
     * Request was canceled by user
     */
    RequestCanceled( -23 ),

    /**
     * All orders are invalidated while algorithm is warming up
     */
    AlgorithmWarmingUp( -24 ),

    /**
     * Internal logic invalidated update order
     */
    BrokerageModelRefusedToUpdateOrder( -25 ),

    /**
     * ) ), Need quote currency in cashbook to trade
     */
    QuoteCurrencyRequired( -26 ),

    /**
     * Need conversion rate to account currency
     */
    ConversionRateZero( -27 ),

    /**
     * The order's symbol references a non-tradable security
     */
    NonTradableSecurity( -28 );

    private final int code;

    OrderResponseErrorCode( int code ) {
        this.code = code;
    }

    public int getCode() {
        return code;
    }

}