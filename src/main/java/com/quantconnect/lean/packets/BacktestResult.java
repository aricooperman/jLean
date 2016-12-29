package com.quantconnect.lean.packets;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.HashMap;
import java.util.Map;

import com.quantconnect.lean.charting.Chart;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.statistics.AlgorithmPerformance;

import it.unimi.dsi.fastutil.ints.Int2ObjectArrayMap;
import it.unimi.dsi.fastutil.ints.Int2ObjectMap;

/**
 *  Backtest results object class - result specific items from the packet.
 */
public class BacktestResult {

    /**
     * Chart updates in this backtest since the last backtest result packet was sent.
     */
    public final Map<String,Chart> charts;
    
    /**
     * Order updates since the last backtest result packet was sent.
     */
    public final Map<Integer,Order> orders;
    
    /**
     * Profit and loss results from closed trades.
     */
    public final Map<LocalDate,BigDecimal> profitLoss;

    /**
     * Statistics information for the backtest.
     * The statistics are only generated on the last result packet of the backtest.
     */
    public final Map<String,String> statistics;

    /**
     * The runtime / dynamic statistics generated while a backtest is running.
     */
    public final Map<String,String> runtimeStatistics;

    /**
     * Rolling window detailed statistics.
     */
    public final Map<String,AlgorithmPerformance> rollingWindow;
    
    /**
     * Rolling window detailed statistics.
     */
    public AlgorithmPerformance totalPerformance = null;

    /**
     * Default Constructor
     */
    public BacktestResult() {
        this( new HashMap<>(), new Int2ObjectArrayMap<>(), new HashMap<>(), new HashMap<>(), new HashMap<>(), new HashMap<>() );
    }

    public BacktestResult( Map<String,Chart> charts, Int2ObjectMap<Order> orders, Map<LocalDate,BigDecimal> profitLoss, Map<String,String> statistics, 
            Map<String,String> runtimeStatistics, Map<String,AlgorithmPerformance> rollingWindow ) {
        this( charts, orders, profitLoss, statistics, runtimeStatistics, rollingWindow, null );
    }
    
    /**
     * Constructor for the result class using dictionary objects.
     */
    public BacktestResult( Map<String,Chart> charts, Int2ObjectMap<Order> orders, Map<LocalDate,BigDecimal> profitLoss, Map<String,String> statistics, 
            Map<String,String> runtimeStatistics, Map<String,AlgorithmPerformance> rollingWindow, AlgorithmPerformance totalPerformance ) {
        this.charts = charts;
        this.orders = orders;
        this.profitLoss = profitLoss;
        this.statistics = statistics;
        this.runtimeStatistics = runtimeStatistics;
        this.rollingWindow = rollingWindow;
        this.totalPerformance = totalPerformance;
    }
}