package com.quantconnect.lean.packets;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.HashMap;
import java.util.Map;

import com.quantconnect.lean.charting.Chart;
import com.quantconnect.lean.orders.Order;

/**
 *  Backtest results object class - result specific items from the packet.
 */
public class BacktestResult {

    /**
     * Chart updates in this backtest since the last backtest result packet was sent.
     */
    public final Map<String,Chart> Charts = new HashMap<String,Chart>();
    
    /**
     * Order updates since the last backtest result packet was sent.
     */
    public final Map<Integer,Order> Orders = new HashMap<Integer,Order>();
    
    /**
     * Profit and loss results from closed trades.
     */
    public final Map<LocalDate,BigDecimal> ProfitLoss = new HashMap<LocalDate,BigDecimal>();

    /**
     * Statistics information for the backtest.
     * The statistics are only generated on the last result packet of the backtest.
     */
    public final Map<String,String> Statistics = new HashMap<String,String>();

    /**
     * The runtime / dynamic statistics generated while a backtest is running.
     */
    public final Map<String,String> RuntimeStatistics = new HashMap<String,String>();

    /**
     * Rolling window detailed statistics.
     */
    public final Map<String,AlgorithmPerformance> RollingWindow = new HashMap<String,AlgorithmPerformance>();
    
    /**
     * Rolling window detailed statistics.
     */
    public AlgorithmPerformance TotalPerformance = null;

    /**
     * Default Constructor
     */
    public BacktestResult() { }

    public BacktestResult( Map<String,Chart> charts, Int2ObjectMap<Order> orders, Map<DateTime,BigDecimal> profitLoss, Map<String,String> statistics, 
            Map<String,String> runtimeStatistics, Map<String,AlgorithmPerformance> rollingWindow ) {
        this( charts, orders, profitLoss, statistics, runtimeStatistics, rollingWindow, null );
    }
    
    /**
     * Constructor for the result class using dictionary objects.
     */
    public BacktestResult( Map<String,Chart> charts, Int2ObjectMap<Order> orders, Map<DateTime,BigDecimal> profitLoss, Map<String,String> statistics, 
            Map<String,String> runtimeStatistics, Map<String,AlgorithmPerformance> rollingWindow, AlgorithmPerformance totalPerformance ) {
        Charts = charts;
        Orders = orders;
        ProfitLoss = profitLoss;
        Statistics = statistics;
        RuntimeStatistics = runtimeStatistics;
        RollingWindow = rollingWindow;
        TotalPerformance = totalPerformance;
    }
}