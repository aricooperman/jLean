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

package com.quantconnect.lean.securities;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Objects;
import java.util.concurrent.CopyOnWriteArrayList;

import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.orders.fees.IFeeModel;
import com.quantconnect.lean.orders.fills.IFillModel;
import com.quantconnect.lean.orders.slippage.ISlippageModel;
import com.quantconnect.lean.securities.interfaces.ISecurityDataFilter;
import com.quantconnect.lean.securities.interfaces.ISecurityTransactionModel;
import com.quantconnect.lean.Global.DataNormalizationMode;
import com.quantconnect.lean.Global.Resolution;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.LocalTimeKeeper;
import com.quantconnect.lean.Symbol;

//using System.Linq;
//using QuantConnect.Data;
//using QuantConnect.Orders.Fees;
//using QuantConnect.Orders.Fills;
//using QuantConnect.Orders.Slippage;
//using QuantConnect.Securities.Equity;
//using QuantConnect.Securities.Forex;
//using QuantConnect.Securities.Interfaces;

/**
 * A base vehicle properties class for providing a common interface to all assets in QuantConnect.
 * 
 * Security object is intended to hold properties of the specific security asset. These properties can include trade start-stop dates, 
 * price, market hours, resolution of the security, the holdings information for this security and the specific fill model.
 */
public class Security {
    
    private final Symbol symbol;

    // using concurrent bag to avoid list enumeration threading issues
    protected final CopyOnWriteArrayList<SubscriptionDataConfig> subscriptionsBag;
    
    private LocalTimeKeeper localTimeKeeper;
    private boolean tradable;
    private Cash quoteCurrency;
    private SymbolProperties symbolProperties;

    /**
     * Data cache for the security to store previous price information.
     * <seealso cref="EquityCache"/>
     * <seealso cref="ForexCache"/>
     */
    private SecurityCache cache;

    /**
     * Holdings class contains the portfolio, cash and processes order fills.
     * <seealso cref="EquityHolding"/>
     * <seealso cref="ForexHolding"/>
     */
    private SecurityHolding holdings;
    
    /**
     * Exchange class contains the market opening hours, along with pre-post market hours.
     * <seealso cref="EquityExchange"/>
     * <seealso cref="ForexExchange"/>
     */
    private SecurityExchange exchange;
    
    /**
     * Fee model used to compute order fees for this security
    */
    private IFeeModel feeModel;
    
    /**
     * Fill model used to produce fill events for this security
    */
    private IFillModel fillModel;

    /**
     * Slippage model use to compute slippage of market orders
     */
    private ISlippageModel slippageModel;

    /**
     * Gets the portfolio model used by this security
    */
    private ISecurityPortfolioModel portfolioModel;
    
    /**
     * Gets the margin model used for this security
    */
    private ISecurityMarginModel marginModel;
    
    /**
     * Gets the settlement model used for this security
    */
    private ISettlementModel settlementModel;
    
    /**
     * Gets the volatility model used for this security
     */
    private IVolatilityModel volatilityModel;

    /**
     * Customizable data filter to filter outlier ticks before they are passed into user event handlers. 
     * By default all ticks are passed into the user algorithms.
     * TradeBars (seconds and minute bars) are prefiltered to ensure the ticks which build the bars are realistically tradeable
     * <seealso cref="EquityDataFilter"/>
     * <seealso cref="ForexDataFilter"/>
     */
    private ISecurityDataFilter dataFilter;
    
    
    /**
     * Gets all the subscriptions for this security
     */
    public List<SubscriptionDataConfig> getSubscriptions() {
        return subscriptionsBag;
    }
    
    /**
     * <see cref="Symbol"/> for the asset.
     */
    public Symbol getSymbol() {
        return symbol;
    }
    
    /**
     * Gets the Cash object used for converting the quote currency to the account currency
     */
    public Cash getQuoteCurrency() {
        return quoteCurrency;
    }

    
    /**
     * Gets the symbol properties for this security
     */
    public SymbolProperties getSymbolProperties() {
        return symbolProperties;
    }

    /**
     *  Type of the security.
     *  QuantConnect currently only supports Equities and Forex
     */
    public SecurityType getType() {
         return symbol.getId().getSecurityType();
    }

    /**
     *  Resolution of data requested for this security.
     *  Tick, second or minute resolution for QuantConnect assets.
     */
    public Resolution getResolution() {
         return subscriptionsBag.stream().map( x -> x.resolution ).min( Resolution::compareTo ).orElse( Resolution.Daily );
    }

    /**
     *  Indicates the data will use previous bars when there was no trading in this time period. This was a configurable datastream setting set in initialization.
     */
    public boolean isFillDataForward() {
         return subscriptionsBag.stream().anyMatch( x -> x.fillDataForward );
    }

    /**
     *  Indicates the security will continue feeding data after the primary market hours have closed. This was a configurable setting set in initialization.
     */
    public boolean isExtendedMarketHours() {
        return subscriptionsBag.stream().anyMatch( x -> x.extendedMarketHours );
    }

    /**
     *  Gets the data normalization mode used for this security
     */
    public DataNormalizationMode getDataNormalizationMode() {
         return subscriptionsBag.stream().map( x -> x.dataNormalizationMode ).findFirst().orElse( DataNormalizationMode.Adjusted );
    }

//    /**
//     * Gets the subscription configuration for this security
//    */
//    [Obsolete( "This property returns only the first subscription. Use the 'Subscriptions' property for all of this security's subscriptions.")]
//    public SubscriptionDataConfig SubscriptionDataConfig
//    {
//        get { return SubscriptionsBag.FirstOrDefault(); }
//    }

    /**
     *  There has been at least one datapoint since our algorithm started running for us to determine price.
     */
    public boolean hasData() {
        return getLastData() != null; 
    }

    /**
     * Gets or sets whether or not this security should be considered tradable
     */
    public boolean isTradable() {
        return tradable;
    }

    public void setTradable( boolean tradable ) {
        this.tradable = tradable;
    }

    public SecurityCache getCache() {
        return cache;
    }

    public void setCache( SecurityCache cache ) {
        this.cache = cache;
    }

    public SecurityHolding getHoldings() {
        return holdings;
    }

    public void setHoldings( SecurityHolding holdings ) {
        this.holdings = holdings;
    }

    public SecurityExchange getExchange() {
        return exchange;
    }
    
    public void setExchange( SecurityExchange exchange ) {
        this.exchange = exchange;
    }

//    /**
//     * Transaction model class implements the fill models for the security. If the user does not define a model the default
//     * model is used for this asset class.
//     * This is ignored in live trading and the real fill prices are used instead
//     * <seealso cref="EquityTransactionModel"/>
//     * <seealso cref="ForexTransactionModel"/>
//     */
//    [Obsolete( "Security.Model has been made obsolete, use Security.TransactionModel instead.")]
//    public ISecurityTransactionModel Model
//    {
//        get { return TransactionModel; }
//        set { TransactionModel = value; }
//    }


    /**
     * Transaction model class implements the fill models for the security. If the user does not define a model the default
     * model is used for this asset class.
     * This is ignored in live trading and the real fill prices are used instead
     * <seealso cref="EquityTransactionModel"/>
     * <seealso cref="ForexTransactionModel"/>
     * 
     * these methods provided for backwards compatibility
     */
    public ISecurityTransactionModel getTransactionModel() {
        // check if the FillModel/FeeModel/Slippage models are all the same reference
        if( fillModel instanceof ISecurityTransactionModel 
                && fillModel == feeModel
                && feeModel == slippageModel )
            return (ISecurityTransactionModel) fillModel;
            
        return new SecurityTransactionModel( fillModel, feeModel, slippageModel );
    }
    
    public void setTransactionModel( ISecurityTransactionModel value ) {
        feeModel = value;
        fillModel = value;
        slippageModel = value;
    }

    public IFeeModel getFeeModel() {
        return feeModel;
    }

    public void setFeeModel( IFeeModel feeModel ) {
        this.feeModel = feeModel;
    }

    public IFillModel getFillModel() {
        return fillModel;
    }

    public void setFillModel( IFillModel fillModel ) {
        this.fillModel = fillModel;
    }

    public ISlippageModel getSlippageModel() {
        return slippageModel;
    }

    public void setSlippageModel( ISlippageModel slippageModel ) {
        this.slippageModel = slippageModel;
    }
    
    public ISecurityPortfolioModel getPortfolioModel() {
        return portfolioModel;
    }

    public void setPortfolioModel( ISecurityPortfolioModel portfolioModel ) {
        this.portfolioModel = portfolioModel;
    }

    public ISecurityMarginModel getMarginModel() {
        return marginModel;
    }

    public void setMarginModel( ISecurityMarginModel marginModel ) {
        this.marginModel = marginModel;
    }

    public ISettlementModel getSettlementModel() {
        return settlementModel;
    }

    public void setSettlementModel( ISettlementModel settlementModel ) {
        this.settlementModel = settlementModel;
    }

    public IVolatilityModel getVolatilityModel() {
        return volatilityModel;
    }

    public void setVolatilityModel( IVolatilityModel volatilityModel ) {
        this.volatilityModel = volatilityModel;
    }
    
    public ISecurityDataFilter getDataFilter() {
        return dataFilter;
    }

    public void setDataFilter( ISecurityDataFilter dataFilter ) {
        this.dataFilter = dataFilter;
    }

    /**
     * Construct a new security vehicle based on the user options.
    */
    public Security(SecurityExchangeHours exchangeHours, SubscriptionDataConfig config, Cash quoteCurrency, SymbolProperties symbolProperties ) {
        this( config,
                quoteCurrency,
                symbolProperties,
                new SecurityExchange(exchangeHours),
                new SecurityCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                Securities.VolatilityModel.Null,
                new SecurityMarginModel( BigDecimal.ONE ),
                new SecurityDataFilter() );
    }

    /**
     * Construct a new security vehicle based on the user options.
     */
    public Security(Symbol symbol, SecurityExchangeHours exchangeHours, Cash quoteCurrency, SymbolProperties symbolProperties ) {
        this( symbol,
                quoteCurrency,
                symbolProperties,
                new SecurityExchange(exchangeHours),
                new SecurityCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                Securities.VolatilityModel.Null,
                new SecurityMarginModel( BigDecimal.ONE ),
                new SecurityDataFilter() );
    }

    /**
     * Construct a new security vehicle based on the user options.
     */
    protected Security( Symbol symbol,
            Cash quoteCurrency,
            SymbolProperties symbolProperties,
            SecurityExchange exchange,
            SecurityCache cache,
            ISecurityPortfolioModel portfolioModel,
            IFillModel fillModel,
            IFeeModel feeModel,
            ISlippageModel slippageModel,
            ISettlementModel settlementModel,
            IVolatilityModel volatilityModel,
            ISecurityMarginModel marginModel,
            ISecurityDataFilter dataFilter ) {

        Objects.requireNonNull( symbolProperties, "Security requires a valid SymbolProperties instance.");

        if( !symbolProperties.getQuoteCurrency().equals( quoteCurrency.getSymbol() ) )
            throw new IllegalArgumentException( "symbolProperties.QuoteCurrency must match the quoteCurrency.Symbol" );

        this.symbol = symbol;
        this.subscriptionsBag = new CopyOnWriteArrayList<SubscriptionDataConfig>();
        this.quoteCurrency = quoteCurrency;
        this.symbolProperties = symbolProperties;
        this.tradable = true;
        this.cache = cache;
        this.exchange = exchange;
        this.dataFilter = dataFilter;
        this.portfolioModel = portfolioModel;
        this.marginModel = marginModel;
        this.fillModel = fillModel;
        this.feeModel = feeModel;
        this.slippageModel = slippageModel;
        this.settlementModel = settlementModel;
        this.volatilityModel = volatilityModel;
        this.holdings = new SecurityHolding( this );
    }


    /**
     * Temporary convenience constructor
     */
    protected Security( SubscriptionDataConfig config,
            Cash quoteCurrency,
            SymbolProperties symbolProperties,
            SecurityExchange exchange,
            SecurityCache cache,
            ISecurityPortfolioModel portfolioModel,
            IFillModel fillModel,
            IFeeModel feeModel,
            ISlippageModel slippageModel,
            ISettlementModel settlementModel,
            IVolatilityModel volatilityModel,
            ISecurityMarginModel marginModel,
            ISecurityDataFilter dataFilter ) {
        this( config.getSymbol(),
                quoteCurrency,
                symbolProperties,
                exchange,
                cache,
                portfolioModel,
                fillModel,
                feeModel,
                slippageModel,
                settlementModel,
                volatilityModel,
                marginModel,
                dataFilter );
        subscriptionsBag.add( config );
    }

    /**
     * Read only property that checks if we currently own stock in the company.
     */
    public boolean isHoldStock() {
        //Get a boolean, true if we own this stock.
        return holdings.getAbsoluteQuantity() > 0;
    }

    /**
     * Alias for HoldStock - Do we have any of this security
     */
    public boolean isInvested() {
        return isHoldStock();
    }

    /**
     * Local time for this market 
     */
    public LocalDateTime getLocalTime() {
        if( localTimeKeeper == null )
            throw new RuntimeException( "Security.SetLocalTimeKeeper(LocalTimeKeeper) must be called in order to use the LocalTime property." );

        return localTimeKeeper.getLocalTime();
    }

    /**
     * Get the current value of the security.
     */
    public BigDecimal getPrice() {
        return cache.getPrice();
    }

    /**
     * Leverage for this Security.
    */
    public BigDecimal getLeverage() {
        return holdings.getLeverage();
    }

    /**
     * If this uses tradebar data, return the most recent high.
     */
    public BigDecimal getHigh() {
        return cache.getHigh().signum() == 0 ? getPrice() : cache.getHigh();
    }

    /**
     * If this uses tradebar data, return the most recent low.
     */
    public BigDecimal getLow() {
        return cache.getLow().signum() == 0 ? getPrice() : cache.getLow();
    }

    /**
     * If this uses tradebar data, return the most recent close.
     */
    public BigDecimal getClose() {
         return cache.getClose().signum() == 0 ? getPrice() : cache.getClose();
    }

    /**
     * If this uses tradebar data, return the most recent open.
     */
    public BigDecimal getOpen() {
        return cache.getOpen().signum() == 0 ? getPrice(): cache.getOpen();
    }

    /**
     * Access to the volume of the equity today
     */
    public long getVolume() {
        return cache.getVolume();
    }

    /**
     * Gets the most recent bid price if available
     */
    public BigDecimal getBidPrice() {
        return cache.getBidPrice().signum() == 0 ? getPrice() : cache.getBidPrice();
    }

    /**
     * Gets the most recent bid size if available
     */
    public long getBidSize() {
        return cache.getBidSize();
    }

    /**
     * Gets the most recent ask price if available
     */
    public BigDecimal getAskPrice() {
        return cache.getAskPrice().signum() == 0 ? getPrice() : cache.getAskPrice();
    }

    /**
     * Gets the most recent ask size if available
     */
    public long getAskSize() {
        return cache.getAskSize();
    }

    /**
     * Get the last price update set to the security.
     * @returns BaseData object for this security
     */
    public BaseData getLastData() {
        return cache.getData();
    }

    /**
     * Sets the <see cref="LocalTimeKeeper"/> to be used for this <see cref="Security"/>.
     * This is the source of this instance's time.
     * @param localTimeKeeper The source of this <see cref="Security"/>'s time.
     */
    public void setLocalTimeKeeper( LocalTimeKeeper localTimeKeeper ) {
        this.localTimeKeeper = localTimeKeeper;
        exchange.setLocalDateTimeFrontier( localTimeKeeper.getLocalTime() );

        localTimeKeeper.addListener( (time,tz) -> {
            //Update the Exchange/Timer:
            exchange.setLocalDateTimeFrontier( time );
        } );
    }
    
    /**
     * Update any security properties based on the latest market data and time
     * @param data New data packet from LEAN
     */
    public void setMarketPrice( BaseData data ) {
        //Add new point to cache:
        if( data == null ) 
            return;
        
        cache.addData( data );
        holdings.updateMarketPrice( getPrice() );
        volatilityModel.update( this, data );
    }

    /**
     * Update any security properties based on the latest realtime data and time
     * @param data New data packet from LEAN
     */
    public void setRealTimePrice( BaseData data ) {
        //Add new point to cache:
        if( data == null ) 
            return;
        
        cache.addData( data );
        holdings.updateMarketPrice( getPrice() );
    }
 
    /**
     * Set the leverage parameter for this security
     * @param leverage Leverage for this asset
     */
    public void setLeverage( BigDecimal leverage ) {
        marginModel.setLeverage( this, leverage );
    }

    /**
     * Sets the data normalization mode to be used by this security
     */
    public void setDataNormalizationMode( DataNormalizationMode mode ) {
        for( SubscriptionDataConfig subscription : subscriptionsBag )
            subscription.dataNormalizationMode = mode;
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
    */
    @Override
    public String toString() {
        return symbol.toString();
    }

    /**
     * Adds the specified data subscription to this security.
     * @param subscription The subscription configuration to add. The Symbol and ExchangeTimeZone properties must match the existing Security object
     */
    void addData( SubscriptionDataConfig subscription ) {
        if( !subscription.getSymbol().equals( symbol ) ) 
            throw new IllegalArgumentException( "Symbols must match. (subscription.Symbol)" );
        
        if( !subscription.exchangeTimeZone.equals( exchange.getTimeZone() ) ) 
            throw new IllegalArgumentException( "ExchangeTimeZones must match. (subscription.ExchangeTimeZone)" );
        
        subscriptionsBag.add( subscription );
    }
}