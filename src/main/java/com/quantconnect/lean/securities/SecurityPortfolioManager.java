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
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang3.NotImplementedException;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderEvent;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.SubmitOrderRequest;

/**
 * Portfolio manager class groups popular properties and makes them accessible through one interface.
 * It also provide indexing by the vehicle symbol to get the Security.Holding objects.
 */
public class SecurityPortfolioManager implements Map<Symbol,SecurityHolding>, ISecurityProvider {
    
    /**
     * Local access to the securities collection for the portfolio summation.
     */
    public SecurityManager Securities;

    /**
     * Local access to the transactions collection for the portfolio summation and updates.
     */
    public SecurityTransactionManager Transactions;

    /**
     * The list of pending funds waiting for settlement time
     */
    private final List<UnsettledCashAmount> _unsettledCashAmounts;

    // The _unsettledCashAmounts list has to be synchronized because order fills are happening on a separate thread
    private final Object _unsettledCashAmountsLocker = new Object();

    // Record keeping variables
    private final Cash _baseCurrencyCash;
    private final Cash _baseCurrencyUnsettledCash;

    private CashBook CashBook;
    private CashBook UnsettledCashBook;
    private MarginCallModel MarginCallModel;

    /**
     * Initialize security portfolio manager.
     */
    public SecurityPortfolioManager( SecurityManager securityManager, SecurityTransactionManager transactions ) {
        Securities = securityManager;
        Transactions = transactions;
        MarginCallModel = new MarginCallModel( this );

        CashBook = new CashBook();
        UnsettledCashBook = new CashBook();
        _unsettledCashAmounts = new ArrayList<UnsettledCashAmount>();

        _baseCurrencyCash = CashBook.get( CashBook.ACCOUNT_CURRENCY );
        _baseCurrencyUnsettledCash = UnsettledCashBook.get( CashBook.ACCOUNT_CURRENCY );

        // default to $100,000.00
        _baseCurrencyCash.setAmount( BigDecimal.valueOf( 100_000 ) );
    }
    
    /**
     * Gets the cash book that keeps track of all currency holdings (only settled cash)
     */
    public CashBook getCashBook() {
        return CashBook;
    }
    
    /**
     * Gets the cash book that keeps track of all currency holdings (only unsettled cash)
     */
    public CashBook getUnsettledCashBook() {
        return UnsettledCashBook;
    }

    /**
     * Add a new securities string-security to the portfolio.
     * @param symbol Symbol of Map
     * @param holding SecurityHoldings object
     * @throws NotImplementedException Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.
     * This method is not implemented and using it will throw an exception
     */
    public void add( Symbol symbol, SecurityHolding holding ) { 
        throw new NotImplementedException( "Portfolio object is an adaptor for Security Manager. To add a new asset add the required data during initialization." ); 
    }

    /**
     * Add a new securities key value pair to the portfolio.
     * @param pair Key value pair of Map
     * <exception cref="NotImplementedException Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
     * This method is not implemented and using it will throw an exception
     */
    public void add( Entry<Symbol,SecurityHolding> pair ) { throw new NotImplementedException( "Portfolio object is an adaptor for Security Manager. To add a new asset add the required data during initialization."); }

    /**
     * Clear the portfolio of securities objects.
     * <exception cref="NotImplementedException Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
     * This method is not implemented and using it will throw an exception
     */
    public void clear() { throw new NotImplementedException( "Portfolio object is an adaptor for Security Manager and cannot be cleared."); }

    /**
     * Remove this keyvalue pair from the portfolio.
     * <exception cref="NotImplementedException Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
     * @param pair Key value pair of Map
     * This method is not implemented and using it will throw an exception
     */
    public boolean remove( Entry<Symbol,SecurityHolding> pair ) { throw new NotImplementedException( "Portfolio object is an adaptor for Security Manager and objects cannot be removed."); }

    /**
     * Remove this symbol from the portfolio.
     * <exception cref="NotImplementedException Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
     * @param symbol Symbol of Map
     * This method is not implemented and using it will throw an exception
     */
    public boolean rmove( Symbol symbol ) { throw new NotImplementedException( "Portfolio object is an adaptor for Security Manager and objects cannot be removed."); }

    /**
     * Check if the portfolio contains this symbol string.
     * @param symbol String search symbol for the security
     * @returns Boolean true if portfolio contains this symbol
     */
    public boolean containsKey( Symbol symbol ) {
        return Securities.containsKey( symbol );
    }

    /**
     * Check if the key-value pair is in the portfolio.
     * Map implementation calling the underlying Securities collection
     * @param pair Pair we're searching for
     * @returns True if we have this object
     */
    public boolean contains(KeyValuePair<Symbol, SecurityHolding> pair) {
        return Securities.ContainsKey(pair.Key);
    }

    /**
     * Count the securities objects in the portfolio.
     * Map implementation calling the underlying Securities collection
     */
    public int size() {
        return Securities.size();
    }

    /**
     * Check if the underlying securities array is read only.
     * Map implementation calling the underlying Securities collection
     */
    public boolean isReadOnly() {
        return Securities.isReadOnly();
    }

//    /**
//     * Copy contents of the portfolio collection to a new destination.
//     * Map implementation calling the underlying Securities collection
//     * @param array Destination array
//     * @param index Position in array to start copying
//    */
//    public void copyTo( KeyValuePair<Symbol, SecurityHolding>[] array, int index) {
//        array = new KeyValuePair<Symbol, SecurityHolding>[Securities.Count];
//        i = 0;
//        for( asset : Securities ) {
//            if( i >= index) {
//                array[i] = new KeyValuePair<Symbol, SecurityHolding>(asset.Key, asset.Value.Holdings);
//            }
//            i++;
//        }
//    }

    /**
     * Symbol keys collection of the underlying assets in the portfolio.
     * Map implementation calling the underlying securities key symbols
     */
    public Set<Symbol> keySet() {
        return Securities.keySet();
    }

    /**
     * Collection of securities objects in the portfolio.
     * Map implementation calling the underlying securities values collection
     */
    public Collection<SecurityHolding> values() {
         return Securities.values().stream().map( asset -> asset.getHoldings() ).collect( Collectors.toList() );
    }

    /**
     * Attempt to get the value of the securities holding class if this symbol exists.
     * @param symbol String search symbol
     * @param holding Holdings object of this security
     * Map implementation
     * @returns Boolean true if successful locating and setting the holdings object
     */
    public SecurityHolding get( Symbol symbol ) {
        final Security security = Securities.get( symbol );
        final boolean success = security != null;
        return success ? security.getHoldings() : null;
    }

    /**
     * Get the enumerator for the underlying securities collection.
     * Map implementation
     * @returns Enumerable key value pair
    */
    IEnumerator<KeyValuePair<Symbol, SecurityHolding>> IEnumerable<KeyValuePair<Symbol, SecurityHolding>>.GetEnumerator() {
        return Securities.Select(x -> new KeyValuePair<Symbol, SecurityHolding>(x.Key, x.Value.Holdings)).GetEnumerator();
    }

    /**
     * Get the enumerator for the underlying securities collection.
     * Map implementation
     * @returns Enumerator
     */
    IEnumerator IEnumerable.GetEnumerator() {
        return Securities.Select(x -> new KeyValuePair<Symbol, SecurityHolding>(x.Key, x.Value.Holdings)).GetEnumerator();
    }

    /**
     * Sum of all currencies in account in US dollars (only settled cash)
     * 
     * This should not be mistaken for margin available because Forex uses margin
     * even though the total cash value is not impact
     * 
     */
    public BigDecimal getCash() {
        return CashBook.getTotalValueInAccountCurrency();
    }

    /**
     * Sum of all currencies in account in US dollars (only unsettled cash)
     * 
     * This should not be mistaken for margin available because Forex uses margin
     * even though the total cash value is not impact
     * 
     */
    public BigDecimal getUnsettledCash() {
        return UnsettledCashBook.getTotalValueInAccountCurrency();
    }

    /**
     * Absolute value of cash discounted from our total cash by the holdings we own.
     * When account has leverage the actual cash removed is a fraction of the purchase price according to the leverage
     */
    public BigDecimal getTotalUnleveredAbsoluteHoldingsCost() {
        //Sum of unlevered cost of holdings
        return Securities.values().stream().map( p -> p.getHoldings().getUnleveredAbsoluteHoldingsCost() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Gets the total absolute holdings cost of the portfolio. This sums up the individual 
     * absolute cost of each holding
     */
    public BigDecimal getTotalAbsoluteHoldingsCost() {
        return Securities.values().stream().map( x -> x.getHoldings().getAbsoluteHoldingsCost() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Absolute sum the individual items in portfolio.
    */
    public BigDecimal getTotalHoldingsValue() {
        //Sum sum of holdings
        return Securities.values().stream().map( position -> position.getHoldings().getAbsoluteHoldingsValue() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Boolean flag indicating we have any holdings in the portfolio.
     * Assumes no asset can have $0 price and uses the sum of total holdings value
     * <seealso cref="Invested"/>
     */
    public boolean isHoldStock() {
        return getTotalHoldingsValue().signum() > 0;
    }

    /**
     * Alias for HoldStock. Check if we have and holdings.
     * <seealso cref="HoldStock"/>
     */
    public boolean isInvested() {
        return isHoldStock();
    }

    /**
     * Get the total unrealised profit in our portfolio from the individual security unrealized profits.
     */
    public BigDecimal getTotalUnrealisedProfit() {
        return Securities.values().stream().map( position -> position.getHoldings().getUnrealizedProfit() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Get the total unrealised profit in our portfolio from the individual security unrealized profits.
     * Added alias for American spelling
     */
    public BigDecimal getTotalUnrealizedProfit() {
        return getTotalUnrealisedProfit();
    }

    /**
     * Total portfolio value if we sold all holdings at current market rates.
     * Cash + TotalUnrealisedProfit + TotalUnleveredAbsoluteHoldingsCost
     * <seealso cref="Cash"/>
     * <seealso cref="TotalUnrealizedProfit"/>
     * <seealso cref="TotalUnleveredAbsoluteHoldingsCost"/>
     */
    public BigDecimal getTotalPortfolioValue() {
        // we can't include forex in this calculation since we would be double accounting with respect to the cash book
        BigDecimal totalHoldingsValueWithoutForex = 0;
        for( Security position : Securities.values() ) {
             if( position.getType() != SecurityType.Forex ) 
                 totalHoldingsValueWithoutForex = totalHoldingsValueWithoutForex.add( position.getHoldings().getHoldingsValue() );
        }

        return CashBook.setTotalValueInAccountCurrency( CashBook.getTotalValueInAccountCurrency().add( UnsettledCashBook.getTotalValueInAccountCurrency() ).add( totalHoldingsValueWithoutForex ) );
    }

    /**
     * Total fees paid during the algorithm operation across all securities in portfolio.
     */
    public BigDecimal getTotalFees() {
        return Securities.values().stream().map( position -> position.getHoldings().getTotalFees() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Sum of all gross profit across all securities in portfolio.
     */
    public BigDecimal getTotalProfit() {
        return Securities.values().stream().map( position -> position.getHoldings().getProfit() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Total sale volume since the start of algorithm operations.
    */
    public BigDecimal getTotalSaleVolume() {
        return Securities.values().stream().map( position -> position.getHoldings().getTotalSaleVolume() ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Gets the total margin used across all securities in the account's currency
    */
    public BigDecimal getTotalMarginUsed() {
        return Securities.values().stream().map( position -> position.getMarginModel().getMaintenanceMargin( position ) ).reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Gets the remaining margin on the account in the account's currency
    */
    public BigDecimal getMarginRemaining() {
        return getTotalPortfolioValue().subtract( UnsettledCashBook.getTotalValueInAccountCurrency() ).subtract( getTotalMarginUsed() );
    }

    /**
     * Gets or sets the <see cref="MarginCallModel"/> for the portfolio. This
     * is used to executed margin call orders.
     */
    public MarginCallModel getMarginCallModel() {
        return MarginCallModel;
    }

    public void setMarginCallModel( MarginCallModel marginCallModel ) {
        MarginCallModel = marginCallModel;
    }

    /**
     * Indexer for the PortfolioManager class to access the underlying security holdings objects.
     * @param symbol Symbol object indexer
     * @returns SecurityHolding class from the algorithm securities
     */
    public SecurityHolding get( Symbol symbol ) {
        return Securities.get( symbol ).getHoldings();
    }

    public SecurityHolding put( Symbol symbol, SecurityHolding value ) {
        Securities.get( symbol ).setHoldings( value ); 
    }

    /**
     * Indexer for the PortfolioManager class to access the underlying security holdings objects.
     * @param ticker string ticker symbol indexer
     * @returns SecurityHolding class from the algorithm securities
     */
    public SecurityHolding this[string ticker]
    {
        get { return Securities[ticker].Holdings; }
        set { Securities[ticker].Holdings = value; }
    }

    /**
     * Set the base currrency cash this algorithm is to manage.
     * @param cash Decimal cash value of portfolio
     */
    public void setCash( BigDecimal cash) {
        _baseCurrencyCash.SetAmount(cash);
    }

    /**
     * Set the cash for the specified symbol
     * @param symbol The cash symbol to set
     * @param cash Decimal cash value of portfolio
     * @param conversionRate The current conversion rate for the
     */
    public void setCash( String symbol, BigDecimal cash, BigDecimal conversionRate ) {
        Cash item;
        if( CashBook.TryGetValue(symbol, out item)) {
            item.setAmount( cash );
            item.ConversionRate = conversionRate;
        }
        else
            CashBook.Add(symbol, cash, conversionRate);
    }

     /**
      * Gets the margin available for trading a specific symbol in a specific direction.
      * @param symbol The symbol to compute margin remaining for
      * @returns The maximum order size that is currently executable in the specified direction
      */
     public BigDecimal getMarginRemaining( Symbol symbol ) {
         return getMarginRemaining( symbol, OrderDirection.Buy );
     }
     
    /**
     * Gets the margin available for trading a specific symbol in a specific direction.
     * @param symbol The symbol to compute margin remaining for
     * @param direction The order/trading direction
     * @returns The maximum order size that is currently executable in the specified direction
     */
    public BigDecimal getMarginRemaining( Symbol symbol, OrderDirection direction ) {
        final Security security = Securities.get( symbol );
        return security.getMarginModel().getMarginRemaining( this, security, direction );
    }

     /**
      * Gets the margin available for trading a specific symbol in a specific direction.
      * Alias for <see cref="GetMarginRemaining"/>
      * @param symbol The symbol to compute margin remaining for
      * @returns The maximum order size that is currently executable in the specified direction
      */
     public BigDecimal getBuyingPower( Symbol symbol ) {
         return getBuyingPower( symbol, OrderDirection.Buy );
     }
     
    /**
     * Gets the margin available for trading a specific symbol in a specific direction.
     * Alias for <see cref="GetMarginRemaining"/>
     * @param symbol The symbol to compute margin remaining for
     * @param direction The order/trading direction
     * @returns The maximum order size that is currently executable in the specified direction
     */
    public BigDecimal getBuyingPower( Symbol symbol, OrderDirection direction ) {
        return getMarginRemaining( symbol, direction );
    }

    /**
     * Calculate the new average price after processing a partial/complete order fill event. 
     * 
     *     For purchasing stocks from zero holdings, the new average price is the sale price.
     *     When simply partially reducing holdings the average price remains the same.
     *     When crossing zero holdings the average price becomes the trade price in the new side of zero.
     * 
     */
    public void processFill( OrderEvent fill ) {
        security = Securities[fill.Symbol];
        security.PortfolioModel.ProcessFill(this, security, fill);
    }

    /**
     * Scan the portfolio and the updated data for a potential margin call situation which may get the holdings below zero! 
     * If there is a margin call, liquidate the portfolio immediately before the portfolio gets sub zero.
     * @param issueMarginCallWarning Set to true if a warning should be issued to the algorithm
     * @returns True for a margin call on the holdings.
     */
    public List<SubmitOrderRequest> scanForMarginCall( out boolean issueMarginCallWarning ) {
        issueMarginCallWarning = false;

        totalMarginUsed = TotalMarginUsed;

        // don't issue a margin call if we're not using margin
        if( totalMarginUsed <= 0) {
            return new List<SubmitOrderRequest>();
        }

        // don't issue a margin call if we're under 1x implied leverage on the whole portfolio's holdings
        averageHoldingsLeverage = TotalAbsoluteHoldingsCost/totalMarginUsed;
        if( averageHoldingsLeverage <= 1.0m) {
            return new List<SubmitOrderRequest>();
        }

        marginRemaining = MarginRemaining;

        // issue a margin warning when we're down to 5% margin remaining
        totalPortfolioValue = TotalPortfolioValue;
        if( marginRemaining <= totalPortfolioValue*0.05m) {
            issueMarginCallWarning = true;
        }

        // if we still have margin remaining then there's no need for a margin call
        if( marginRemaining > 0) {
            return new List<SubmitOrderRequest>();
        }

        // generate a listing of margin call orders
        marginCallOrders = new List<SubmitOrderRequest>();

        // skip securities that have no price data or no holdings, we can't liquidate nothingness
        for( Security security : Securities.values().Where(x -> x.Holdings.Quantity != 0 && x.Price != 0)) {
            marginCallOrder = security.MarginModel.GenerateMarginCallOrder(security, totalPortfolioValue, totalMarginUsed);
            if( marginCallOrder != null && marginCallOrder.Quantity != 0) {
                marginCallOrders.Add(marginCallOrder);
            }
        }

        return marginCallOrders;
    }

    /**
     * Applies a dividend to the portfolio
     * @param dividend The dividend to be applied
     */
    public void applyDividend( Dividend dividend ) {
        security = Securities[dividend.Symbol];

        // only apply dividends when we're in raw mode or split adjusted mode
        mode = security.DataNormalizationMode;
        if( mode == DataNormalizationMode.Raw || mode == DataNormalizationMode.splitAdjusted) {
            // longs get benefits, shorts get clubbed on dividends
            total = security.Holdings.Quantity*dividend.Distribution;

            // assuming USD, we still need to add Currency to the security object
            _baseCurrencyCash.AddAmount(total);
        }
    }

    /**
     * Applies a split to the portfolio
     * @param split The split to be applied
     */
    public void applySplit(Split split) {
        security = Securities[split.Symbol];

        // only apply splits in raw data mode, 
        mode = security.DataNormalizationMode;
        if( mode != DataNormalizationMode.Raw) {
            return;
        }

        // we need to modify our holdings in lght of the split factor
        quantity = security.Holdings.Quantity/split.splitFactor;
        avgPrice = security.Holdings.AveragePrice*split.splitFactor;

        // we'll model this as a cash adjustment
        leftOver = quantity - (int) quantity;
        extraCash = leftOver*split.ReferencePrice;
        _baseCurrencyCash.AddAmount(extraCash);

        security.Holdings.SetHoldings(avgPrice, (int) quantity);

        // build a 'next' value to update the market prices in light of the split factor
        next = security.GetLastData();
        if( next == null ) {
            // sometimes we can get splits before we receive data which
            // will cause this to return null, in this case we can't possibly
            // have any holdings or price to set since we haven't received
            // data yet, so just do nothing
            return;
        }
        next.Value *= split.splitFactor;

        // make sure to modify open/high/low as well for tradebar data types
        tradeBar = next as TradeBar;
        if( tradeBar != null ) {
            tradeBar.Open *= split.splitFactor;
            tradeBar.High *= split.splitFactor;
            tradeBar.Low *= split.splitFactor;
        }
        
        // make sure to modify bid/ask as well for tradebar data types
        tick = next as Tick;
        if( tick != null ) {
            tick.AskPrice *= split.splitFactor;
            tick.BidPrice *= split.splitFactor;
        }

        security.SetMarketPrice(next);
    }

    /**
     * Record the transaction value and time in a list to later be processed for statistics creation.
     * 
     * Bit of a hack -- but using datetime as dictionary key is dangerous as you can process multiple orders within a second.
     * For the accounting / statistics generating purposes its not really critical to know the precise time, so just add a millisecond while there's an identical key.
     * 
     * @param time Time of order processed 
     * @param transactionProfitLoss Profit Loss.
     */
    public void addTransactionRecord( LocalDateTime time, BigDecimal transactionProfitLoss ) {
        clone = time;
        while( Transactions.TransactionRecord.containsKey(clone)) {
            clone = clone.AddMilliseconds(1);
        }
        
        Transactions.TransactionRecord.Add(clone, transactionProfitLoss);
    }

    /**
     * Retrieves a summary of the holdings for the specified symbol
     * @param symbol The symbol to get holdings for
     * @returns The holdings for the symbol or null if the symbol is invalid and/or not in the portfolio
     */
    Security getSecurity( Symbol symbol ) {
        Security security;
        if( Securities.TryGetValue(symbol, out security)) {
            return security;
        }
        return null;
    }

    /**
     * Adds an item to the list of unsettled cash amounts
    */
     * @param item The item to add
    public void addUnsettledCashAmount(UnsettledCashAmount item) {
        synchronized(_unsettledCashAmountsLocker) {
            _unsettledCashAmounts.Add(item);
        }
    }

    /**
     * Scan the portfolio to check if unsettled funds should be settled
    */
    public void ScanForCashSettlement(DateTime timeUtc) {
        synchronized(_unsettledCashAmountsLocker) {
            foreach (item in _unsettledCashAmounts.ToList()) {
                // check if settlement time has passed
                if( timeUtc >= item.SettlementTimeUtc) {
                    // remove item from unsettled funds list
                    _unsettledCashAmounts.Remove(item);

                    // update unsettled cashbook
                    UnsettledCashBook[item.Currency].AddAmount(-item.Amount);

                    // update settled cashbook
                    CashBook[item.Currency].AddAmount(item.Amount);
                }
            }
        }
    }
}
