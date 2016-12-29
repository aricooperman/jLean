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
 *
*/

//using System.ComponentModel.Composition;

package com.quantconnect.lean.lean.engine;

import java.io.Closeable;

import com.quantconnect.lean.configuration.Config;
import com.quantconnect.lean.lean.engine.datafeeds.IDataFeed;
import com.quantconnect.lean.util.Composer;

/**
 * Provides a container for the algorithm specific handlers
*/
public class LeanEngineAlgorithmHandlers implements Closeable {
    private final IDataFeed _dataFeed;
    private final ISetupHandler _setup;
    private final IResultHandler _results;
    private final IRealTimeHandler _realTime;
    private final ITransactionHandler _transactions;
    private final IHistoryProvider _historyProvider;
    private final ICommandQueueHandler _commandQueue;
    private final IMapFileProvider _mapFileProvider;
    private final IFactorFileProvider _factorFileProvider;

    /**
     * Gets the result handler used to communicate results from the algorithm
    */
    public IResultHandler Results
    {
        get { return _results; }
    }

    /**
     * Gets the setup handler used to initialize the algorithm state
    */
    public ISetupHandler Setup
    {
        get { return _setup; }
    }

    /**
     * Gets the data feed handler used to provide data to the algorithm
    */
    public IDataFeed DataFeed
    {
        get { return _dataFeed; }
    }

    /**
     * Gets the transaction handler used to process orders from the algorithm
    */
    public ITransactionHandler Transactions
    {
        get { return _transactions; }
    }

    /**
     * Gets the real time handler used to process real time events
    */
    public IRealTimeHandler RealTime
    {
        get { return _realTime; }
    }

    /**
     * Gets the history provider used to process historical data requests within the algorithm
    */
    public IHistoryProvider HistoryProvider
    {
        get { return _historyProvider; }
    }

    /**
     * Gets the command queue responsible for receiving external commands for the algorithm
    */
    public ICommandQueueHandler CommandQueue
    {
        get { return _commandQueue; }
    }

    /**
     * Gets the map file provider used as a map file source for the data feed
    */
    public IMapFileProvider MapFileProvider
    {
        get { return _mapFileProvider; }
    }

    /**
     * Gets the map file provider used as a map file source for the data feed
    */
    public IFactorFileProvider FactorFileProvider
    {
        get { return _factorFileProvider; }
    }

    /**
     * Initializes a new instance of the <see cref="LeanEngineAlgorithmHandlers"/> class from the specified handlers
     * @param results The result handler for communicating results from the algorithm
     * @param setup The setup handler used to initialize algorithm state
     * @param dataFeed The data feed handler used to pump data to the algorithm
     * @param transactions The transaction handler used to process orders from the algorithm
     * @param realTime The real time handler used to process real time events
     * @param historyProvider The history provider used to process historical data requests
     * @param commandQueue The command queue handler used to receive external commands for the algorithm
     * @param mapFileProvider The map file provider used to retrieve map files for the data feed
     */
    public LeanEngineAlgorithmHandlers(
            final IResultHandler results,
            final ISetupHandler setup,
            final IDataFeed dataFeed,
            final ITransactionHandler transactions,
            final IRealTimeHandler realTime,
            final IHistoryProvider historyProvider,
            final ICommandQueueHandler commandQueue,
            final IMapFileProvider mapFileProvider,
            final IFactorFileProvider factorFileProvider
            ) {
        
        if( results == null ) {
            throw new NullPointerException( "results");
        }
        if( setup == null ) {
            throw new NullPointerException( "setup");
        }
        if( dataFeed == null ) {
            throw new NullPointerException( "dataFeed");
        }
        if( transactions == null ) {
            throw new NullPointerException( "transactions");
        }
        if( realTime == null ) {
            throw new NullPointerException( "realTime");
        }
        if( historyProvider == null ) {
            throw new NullPointerException( "realTime");
        }
        if( commandQueue == null ) {
            throw new NullPointerException( "commandQueue");
        }
        if( mapFileProvider == null ) {
            throw new NullPointerException( "mapFileProvider");
        }
        if( factorFileProvider == null ) {
            throw new NullPointerException( "factorFileProvider");
        }
        
        _results = results;
        _setup = setup;
        _dataFeed = dataFeed;
        _transactions = transactions;
        _realTime = realTime;
        _historyProvider = historyProvider;
        _commandQueue = commandQueue;
        _mapFileProvider = mapFileProvider;
        _factorFileProvider = factorFileProvider;
    }
    
    /**
     * Creates a new instance of the <see cref="LeanEngineAlgorithmHandlers"/> class from the specified composer using type names from configuration
     * @param composer The composer instance to obtain implementations from
     * @returns A fully hydrates <see cref="LeanEngineSystemHandlers"/> instance.
     * @throws Exception Throws a CompositionException during failure to load
     */
    public static LeanEngineAlgorithmHandlers fromConfiguration( final Composer composer ) {
        final String setupHandlerTypeName = Config.get( "setup-handler", "ConsoleSetupHandler");
        final String transactionHandlerTypeName = Config.get( "transaction-handler", "BacktestingTransactionHandler");
        final String realTimeHandlerTypeName = Config.get( "real-time-handler", "BacktestingRealTimeHandler");
        final String dataFeedHandlerTypeName = Config.get( "data-feed-handler", "FileSystemDataFeed");
        final String resultHandlerTypeName = Config.get( "result-handler", "BacktestingResultHandler");
        final String historyProviderTypeName = Config.get( "history-provider", "SubscriptionDataReaderHistoryProvider");
        final String commandQueueHandlerTypeName = Config.get( "command-queue-handler", "EmptyCommandQueueHandler");
        final String mapFileProviderTypeName = Config.get( "map-file-provider", "LocalDiskMapFileProvider");
        final String factorFileProviderTypeName = Config.get( "factor-file-provider", "LocalDiskFactorFileProvider");

        return new LeanEngineAlgorithmHandlers(
            composer.<IResultHandler>getExportedValueByTypeName( resultHandlerTypeName ),
            composer.<ISetupHandler>getExportedValueByTypeName(setupHandlerTypeName),
            composer.<IDataFeed>getExportedValueByTypeName(dataFeedHandlerTypeName),
            composer.<ITransactionHandler>getExportedValueByTypeName(transactionHandlerTypeName),
            composer.<IRealTimeHandler>getExportedValueByTypeName(realTimeHandlerTypeName),
            composer.<IHistoryProvider>getExportedValueByTypeName(historyProviderTypeName),
            composer.<ICommandQueueHandler>getExportedValueByTypeName(commandQueueHandlerTypeName),
            composer.<IMapFileProvider>getExportedValueByTypeName(mapFileProviderTypeName),
            composer.<IFactorFileProvider>getExportedValueByTypeName(factorFileProviderTypeName)
            );
    }

    /**
     * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
     */
    @Override
    public void close() {
        Setup.Dispose();
        CommandQueue.Dispose();
    }
}
