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

import java.util.concurrent.CopyOnWriteArrayList;

import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.Global.Resolution;
import com.quantconnect.lean.Global.SecurityType;
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

/// A base vehicle properties class for providing a common interface to all assets in QuantConnect.
/// 
/// Security object is intended to hold properties of the specific security asset. These properties can include trade start-stop dates, 
/// price, market hours, resolution of the security, the holdings information for this security and the specific fill model.
/// 
public class Security {
    
    private final Symbol _symbol;
    private LocalTimeKeeper _localTimeKeeper;
    // using concurrent bag to avoid list enumeration threading issues
    protected final CopyOnWriteArrayList<SubscriptionDataConfig> SubscriptionsBag;

    /// Gets all the subscriptions for this security
    public List<SubscriptionDataConfig> getSubscriptions() {
        return SubscriptionsBag;
    }

    /// <see cref="Symbol"/> for the asset.
    public Symbol getSymbol() {
        return _symbol;
    }

    /// Gets the Cash object used for converting the quote currency to the account currency
    private Cash QuoteCurrency;
    
    public Cash getQuoteCurrency() {
        return QuoteCurrency;
    }

    /**
     * Gets the symbol properties for this security
     */
    private SymbolProperties SymbolProperties;
    
    private SymbolProperties getSymbolProperties() {
        return SymbolProperties;
    }

    /**
     *  Type of the security.
     *  QuantConnect currently only supports Equities and Forex
     */
    public SecurityType getType() {
         return _symbol.ID.SecurityType;
    }

    /**
     *  Resolution of data requested for this security.
     *  Tick, second or minute resolution for QuantConnect assets.
     */
    public Resolution getResolution() {
         return SubscriptionsBag.stream().map( x -> x.resolution ).min( Resolution::compareTo ).orElse( Resolution.Daily );
    }

    /**
     *  Indicates the data will use previous bars when there was no trading in this time period. This was a configurable datastream setting set in initialization.
     */
    public boolean isFillDataForward() {
         return SubscriptionsBag.stream().anyMatch( x -> x.fillDataForward );
    }

    /**
     *  Indicates the security will continue feeding data after the primary market hours have closed. This was a configurable setting set in initialization.
     */
    public boolean isExtendedMarketHours() {
        return SubscriptionsBag.stream().anyMatch( x -> x.extendedMarketHours );
    }

    /**
     *  Gets the data normalization mode used for this security
     */
    public DataNormalizationMode getDataNormalizationMode() {
         return SubscriptionsBag.stream().map( x -> x.dataNormalizationMode ).findFirst().orElse( DataNormalizationMode.Adjusted );
    }

//    /**
//    /// Gets the subscription configuration for this security
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

    private boolean tradable;

    /**
     * Gets or sets whether or not this security should be considered tradable
     */
    public boolean isTradable() {
        return tradable;
    }

    public void setTradable( boolean tradable ) {
        this.tradable = tradable;
    }

    /**
     * Data cache for the security to store previous price information.
    /// <seealso cref="EquityCache"/>
    /// <seealso cref="ForexCache"/>
     */
    public SecurityCache Cache;
    {
        get; set;
    }

    /**
    /// Holdings class contains the portfolio, cash and processes order fills.
    */
    /// <seealso cref="EquityHolding"/>
    /// <seealso cref="ForexHolding"/>
    public SecurityHolding Holdings
    {
        get; 
        set;
    }

    /**
    /// Exchange class contains the market opening hours, along with pre-post market hours.
    */
    /// <seealso cref="EquityExchange"/>
    /// <seealso cref="ForexExchange"/>
    public SecurityExchange Exchange
    {
        get;
        set;
    }

    /**
    /// Transaction model class implements the fill models for the security. If the user does not define a model the default
    /// model is used for this asset class.
    */
    /// This is ignored in live trading and the real fill prices are used instead
    /// <seealso cref="EquityTransactionModel"/>
    /// <seealso cref="ForexTransactionModel"/>
    [Obsolete( "Security.Model has been made obsolete, use Security.TransactionModel instead.")]
    public virtual ISecurityTransactionModel Model
    {
        get { return TransactionModel; }
        set { TransactionModel = value; }
    }

    /**
    /// Transaction model class implements the fill models for the security. If the user does not define a model the default
    /// model is used for this asset class.
    */
    /// This is ignored in live trading and the real fill prices are used instead
    /// <seealso cref="EquityTransactionModel"/>
    /// <seealso cref="ForexTransactionModel"/>
    public ISecurityTransactionModel TransactionModel
    {
        // these methods provided for backwards compatibility
        get
        {
            // check if the FillModel/FeeModel/Slippage models are all the same reference
            if( FillModel is ISecurityTransactionModel 
             && ReferenceEquals(FillModel, FeeModel)
             && ReferenceEquals(FeeModel, SlippageModel)) {
                return (ISecurityTransactionModel) FillModel;
            }
            return new SecurityTransactionModel(FillModel, FeeModel, SlippageModel);
        }
        set
        {
            FeeModel = value;
            FillModel = value;
            SlippageModel = value;
        }
    }

    /**
    /// Fee model used to compute order fees for this security
    */
    public IFeeModel FeeModel
    {
        get;
        set;
    }

    /**
    /// Fill model used to produce fill events for this security
    */
    public IFillModel FillModel
    {
        get;
        set;
    }

    /**
    /// Slippage model use to compute slippage of market orders
    */
    public ISlippageModel SlippageModel
    {
        get;
        set;
    }

    /**
    /// Gets the portfolio model used by this security
    */
    public ISecurityPortfolioModel PortfolioModel
    {
        get;
        set;
    }

    /**
    /// Gets the margin model used for this security
    */
    public ISecurityMarginModel MarginModel
    {
        get;
        set;
    }

    /**
    /// Gets the settlement model used for this security
    */
    public ISettlementModel SettlementModel
    {
        get; 
        set;
    }

    /**
    /// Gets the volatility model used for this security
    */
    public IVolatilityModel VolatilityModel
    {
        get;
        set;
    }

    /**
    /// Customizable data filter to filter outlier ticks before they are passed into user event handlers. 
    /// By default all ticks are passed into the user algorithms.
    */
    /// TradeBars (seconds and minute bars) are prefiltered to ensure the ticks which build the bars are realistically tradeable
    /// <seealso cref="EquityDataFilter"/>
    /// <seealso cref="ForexDataFilter"/>
    public ISecurityDataFilter DataFilter
    {
        get; 
        set;
    }

    /**
    /// Construct a new security vehicle based on the user options.
    */
    public Security(SecurityExchangeHours exchangeHours, SubscriptionDataConfig config, Cash quoteCurrency, SymbolProperties symbolProperties)
        : this(config,
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
            new SecurityMarginModel(1m),
            new SecurityDataFilter()) {
    }

    /**
    /// Construct a new security vehicle based on the user options.
    */
    public Security(Symbol symbol, SecurityExchangeHours exchangeHours, Cash quoteCurrency, SymbolProperties symbolProperties)
        : this(symbol,
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
            new SecurityMarginModel(1m),
            new SecurityDataFilter()
            ) {
    }

    /**
    /// Construct a new security vehicle based on the user options.
    */
    protected Security(Symbol symbol,
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
        ISecurityDataFilter dataFilter
        ) {

        if( symbolProperties == null ) {
            throw new ArgumentNullException( "symbolProperties", "Security requires a valid SymbolProperties instance.");
        }

        if( symbolProperties.QuoteCurrency != quoteCurrency.Symbol) {
            throw new ArgumentException( "symbolProperties.QuoteCurrency must match the quoteCurrency.Symbol");
        }

        _symbol = symbol;
        SubscriptionsBag = new ConcurrentBag<SubscriptionDataConfig>();
        QuoteCurrency = quoteCurrency;
        SymbolProperties = symbolProperties;
        IsTradable = true;
        Cache = cache;
        Exchange = exchange;
        DataFilter = dataFilter;
        PortfolioModel = portfolioModel;
        MarginModel = marginModel;
        FillModel = fillModel;
        FeeModel = feeModel;
        SlippageModel = slippageModel;
        SettlementModel = settlementModel;
        VolatilityModel = volatilityModel;
        Holdings = new SecurityHolding(this);
    }


    /**
    /// Temporary convenience constructor
    */
    protected Security(SubscriptionDataConfig config,
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
        ISecurityDataFilter dataFilter
        )
        : this(config.Symbol,
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
            dataFilter
            ) {
        SubscriptionsBag.Add(config);
    }

    /**
    /// Read only property that checks if we currently own stock in the company.
    */
    public virtual boolean HoldStock 
    {
        get
        {
            //Get a boolean, true if we own this stock.
            return Holdings.AbsoluteQuantity > 0;
        }
    }

    /**
    /// Alias for HoldStock - Do we have any of this security
    */
    public virtual boolean Invested 
    {
        get
        {
            return HoldStock;
        }
    }

    /**
    /// Local time for this market 
    */
    public virtual DateTime LocalTime
    {
        get
        {
            if( _localTimeKeeper == null ) {
                throw new Exception( "Security.SetLocalTimeKeeper(LocalTimeKeeper) must be called in order to use the LocalTime property.");
            }
            return _localTimeKeeper.LocalTime;
        }
    }

    /**
    /// Get the current value of the security.
    */
    public virtual BigDecimal Price 
    {
        get { return Cache.Price; }
    }

    /**
    /// Leverage for this Security.
    */
    public virtual BigDecimal Leverage
    {
        get
        {
            return Holdings.Leverage;
        }
    }

    /**
    /// If this uses tradebar data, return the most recent high.
    */
    public virtual BigDecimal High
    {
        get { return Cache.High == 0 ? Price : Cache.High; }
    }

    /**
    /// If this uses tradebar data, return the most recent low.
    */
    public virtual BigDecimal Low
    {
        get { return Cache.Low == 0 ? Price : Cache.Low; }
    }

    /**
    /// If this uses tradebar data, return the most recent close.
    */
    public virtual BigDecimal Close 
    {
        get { return Cache.Close == 0 ? Price : Cache.Close; }
    }

    /**
    /// If this uses tradebar data, return the most recent open.
    */
    public virtual BigDecimal Open
    {
        get { return Cache.Open == 0 ? Price: Cache.Open; }
    }

    /**
    /// Access to the volume of the equity today
    */
    public virtual long Volume
    {
        get { return Cache.Volume; }
    }

    /**
    /// Gets the most recent bid price if available
    */
    public virtual BigDecimal BidPrice
    {
        get { return Cache.BidPrice == 0 ? Price : Cache.BidPrice; }
    }

    /**
    /// Gets the most recent bid size if available
    */
    public virtual long BidSize
    {
        get { return Cache.BidSize; }
    }

    /**
    /// Gets the most recent ask price if available
    */
    public virtual BigDecimal AskPrice
    {
        get { return Cache.AskPrice == 0 ? Price : Cache.AskPrice; }
    }

    /**
    /// Gets the most recent ask size if available
    */
    public virtual long AskSize
    {
        get { return Cache.AskSize; }
    }

    /**
    /// Get the last price update set to the security.
    */
    @returns BaseData object for this security
    public BaseData GetLastData() {
        return Cache.GetData();
    }

    /**
    /// Sets the <see cref="LocalTimeKeeper"/> to be used for this <see cref="Security"/>.
    /// This is the source of this instance's time.
    */
     * @param localTimeKeeper">The source of this <see cref="Security"/>'s time.
    public void SetLocalTimeKeeper(LocalTimeKeeper localTimeKeeper) {
        _localTimeKeeper = localTimeKeeper;
        Exchange.SetLocalDateTimeFrontier(localTimeKeeper.LocalTime);

        _localTimeKeeper.TimeUpdated += (sender, args) =>
        {
            //Update the Exchange/Timer:
            Exchange.SetLocalDateTimeFrontier(args.Time);
        };
    }
    
    /**
    /// Update any security properties based on the latest market data and time
    */
     * @param data">New data packet from LEAN
    public void SetMarketPrice(BaseData data) {
        //Add new point to cache:
        if( data == null ) return;
        Cache.AddData(data);
        Holdings.UpdateMarketPrice(Price);
        VolatilityModel.Update(this, data);
    }

    /**
    /// Update any security properties based on the latest realtime data and time
    */
     * @param data">New data packet from LEAN
    public void SetRealTimePrice(BaseData data) {
        //Add new point to cache:
            if( data == null ) return;
            Cache.AddData(data);
            Holdings.UpdateMarketPrice(Price);
        }
 
        /**
    /// Set the leverage parameter for this security
    */
     * @param leverage">Leverage for this asset
    public void SetLeverage( BigDecimal leverage) {
        MarginModel.SetLeverage(this, leverage);
    }

    /**
    /// Sets the data normalization mode to be used by this security
    */
    public void SetDataNormalizationMode(DataNormalizationMode mode) {
        foreach (subscription in SubscriptionsBag) {
            subscription.DataNormalizationMode = mode;
        }
    }

    /**
    /// Returns a String that represents the current object.
    */
    @returns 
    /// A String that represents the current object.
    /// 
    /// <filterpriority>2</filterpriority>
    public @Override String toString() {
        return Symbol.toString();
    }

    /**
    /// Adds the specified data subscription to this security.
    */
     * @param subscription">The subscription configuration to add. The Symbol and ExchangeTimeZone properties must match the existing Security object
    internal void AddData(SubscriptionDataConfig subscription) {
        if( subscription.Symbol != _symbol) throw new ArgumentException( "Symbols must match.", "subscription.Symbol");
        if( !subscription.ExchangeTimeZone.Equals(Exchange.TimeZone)) throw new ArgumentException( "ExchangeTimeZones must match.", "subscription.ExchangeTimeZone");
        SubscriptionsBag.Add(subscription);
    }
}