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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides a means of distributing output from enumerators from a dedicated separate thread
    */
    public class BaseDataExchange
    {
        private int _sleepInterval = 1;
        private volatile boolean _isStopping = false;
        private Func<Exception, bool> _isFatalError;

        private final String _name;
        private final object _enumeratorsWriteLock = new object();
        private final ConcurrentMap<Symbol, DataHandler> _dataHandlers;
        private ConcurrentMap<Symbol, EnumeratorHandler> _enumerators;

        /**
         * Gets or sets how long this thread will sleep when no data is available
        */
        public int SleepInterval
        {
            get { return _sleepInterval; }
            set { if( value > -1) _sleepInterval = value; }
        }

        /**
         * Gets a name for this exchange
        */
        public String Name
        {
            get { return _name; }
        }

        /**
         * Initializes a new instance of the <see cref="BaseDataExchange"/>
        */
         * @param name A name for this exchange
         * @param enumerators The enumerators to fanout
        public BaseDataExchange( String name) {
            _name = name;
            _isFatalError = x -> false;
            _dataHandlers = new ConcurrentMap<Symbol, DataHandler>();
            _enumerators = new ConcurrentMap<Symbol, EnumeratorHandler>();
        }

        /**
         * Adds the enumerator to this exchange. If it has already been added
         * then it will remain registered in the exchange only once
        */
         * @param handler The handler to use when this symbol's data is encountered
        public void AddEnumerator(EnumeratorHandler handler) {
            _enumerators[handler.Symbol] = handler;
        }

        /**
         * Adds the enumerator to this exchange. If it has already been added
         * then it will remain registered in the exchange only once
        */
         * @param symbol A unique symbol used to identify this enumerator
         * @param enumerator The enumerator to be added
         * @param shouldMoveNext Function used to determine if move next should be called on this
         * enumerator, defaults to always returning true
         * @param enumeratorFinished Delegate called when the enumerator move next returns false
        public void AddEnumerator(Symbol symbol, IEnumerator<BaseData> enumerator, Func<bool> shouldMoveNext = null, Action<EnumeratorHandler> enumeratorFinished = null ) {
            enumeratorHandler = new EnumeratorHandler(symbol, enumerator, shouldMoveNext);
            if( enumeratorFinished != null ) {
                enumeratorHandler.EnumeratorFinished += (sender, args) -> enumeratorFinished(args);
            }
            AddEnumerator(enumeratorHandler);
        }

        /**
         * Sets the specified function as the error handler. This function
         * returns true if it is a fatal error and queue consumption should
         * cease.
        */
         * @param isFatalError The error handling function to use when an
         * error is encountered during queue consumption. Returns true if queue
         * consumption should be stopped, returns false if queue consumption should
         * continue
        public void SetErrorHandler(Func<Exception, bool> isFatalError) {
            // default to false;
            _isFatalError = isFatalError ?? (x -> false);
        }

        /**
         * Sets the specified hander function to handle data for the handler's symbol
        */
         * @param handler The handler to use when this symbol's data is encountered
        @returns An identifier that can be used to remove this handler
        public void SetDataHandler(DataHandler handler) {
            _dataHandlers[handler.Symbol] = handler;
        }

        /**
         * Sets the specified hander function to handle data for the handler's symbol
        */
         * @param symbol The symbol whose data is to be handled
         * @param handler The handler to use when this symbol's data is encountered
        @returns An identifier that can be used to remove this handler
        public void SetDataHandler(Symbol symbol, Action<BaseData> handler) {
            dataHandler = new DataHandler(symbol);
            dataHandler.DataEmitted += (sender, args) -> handler(args);
            SetDataHandler(dataHandler);
        }

        /**
         * Removes the handler with the specified identifier
        */
         * @param symbol The symbol to remove handlers for
        public boolean RemoveDataHandler(Symbol symbol) {
            DataHandler handler;
            return _dataHandlers.TryRemove(symbol, out handler);
        }

        /**
         * Removes and returns enumerator handler with the specified symbol.
         * The removed handler is returned, null if not found
        */
        public EnumeratorHandler RemoveEnumerator(Symbol symbol) {
            EnumeratorHandler handler;
            if( _enumerators.TryRemove(symbol, out handler)) {
                handler.OnEnumeratorFinished();
                handler.Enumerator.Dispose();
            }
            return handler;
        }

        /**
         * Begins consumption of the wrapped <see cref="IDataQueueHandler"/> on
         * a separate thread
        */
         * @param token A cancellation token used to signal to stop
        public void Start(CancellationToken? token = null ) {
            Log.Trace( "BaseDataExchange(%1$s) Starting...", Name);
            _isStopping = false;
            ConsumeEnumerators(token ?? CancellationToken.None);
        }

        /**
         * Ends consumption of the wrapped <see cref="IDataQueueHandler"/>
        */
        public void Stop() {
            Log.Trace( "BaseDataExchange(%1$s) Stopping...", Name);
            _isStopping = true;
        }

        /** Entry point for queue consumption </summary>
         * @param token A cancellation token used to signal to stop
         *  This function only returns after <see cref="Stop"/> is called or the token is cancelled
        private void ConsumeEnumerators(CancellationToken token) {
            while (true) {
                if( _isStopping || token.IsCancellationRequested) {
                    _isStopping = true;
                    request = token.IsCancellationRequested ? "Cancellation requested" : "Stop requested";
                    Log.Trace( "BaseDataExchange(%1$s).ConsumeQueue(): %2$s.  Exiting...", Name, request);
                    return;
                }

                try
                {
                    // call move next each enumerator and invoke the appropriate handlers

                    handled = false;
                    foreach (kvp in _enumerators) {
                        enumeratorHandler = kvp.Value;
                        enumerator = enumeratorHandler.Enumerator;

                        // check to see if we should advance this enumerator
                        if( !enumeratorHandler.ShouldMoveNext()) continue;

                        if( !enumerator.MoveNext()) {
                            enumeratorHandler.OnEnumeratorFinished();
                            enumeratorHandler.Enumerator.Dispose();
                            _enumerators.TryRemove(enumeratorHandler.Symbol, out enumeratorHandler);
                            continue;
                        }

                        if( enumerator.Current == null ) continue;

                        // if the enumerator is configured to handle it, then do it, don't pass to data handlers
                        if( enumeratorHandler.HandlesData) {
                            handled = true;
                            enumeratorHandler.HandleData(enumerator.Current);
                            continue;
                        }

                        // invoke the correct handler
                        DataHandler dataHandler;
                        if( _dataHandlers.TryGetValue(enumerator.Current.Symbol, out dataHandler)) {
                            handled = true;
                            dataHandler.OnDataEmitted(enumerator.Current);
                        }
                    }

                    // if we didn't handle anything on this past iteration, take a nap
                    if( !handled && _sleepInterval != 0) {
                        Thread.Sleep(_sleepInterval);
                    }
                }
                catch (Exception err) {
                    Log.Error(err);
                    if( _isFatalError(err)) {
                        Log.Trace( "BaseDataExchange(%1$s).ConsumeQueue(): Fatal error encountered. Exiting...", Name);
                        return;
                    }
                }
            }
        }

        /**
         * Handler used to handle data emitted from enumerators
        */
        public class DataHandler
        {
            /**
             * Event fired when MoveNext returns true and Current is non-null
            */
            public event EventHandler<BaseData> DataEmitted;

            /**
             * The symbol this handler handles
            */
            public final Symbol Symbol;

            /**
             * Initializes a new instance of the <see cref="DataHandler"/> class
            */
             * @param symbol The symbol whose data is to be handled
            public DataHandler(Symbol symbol) {
                Symbol = symbol;
            }

            /**
             * Event invocator for the <see cref="DataEmitted"/> event
            */
             * @param data The data being emitted
            public void OnDataEmitted(BaseData data) {
                handler = DataEmitted;
                if( handler != null ) handler(this, data);
            }
        }

        /**
         * Handler used to manage a single enumerator's move next/end of stream behavior
        */
        public class EnumeratorHandler
        {
            private final Func<bool> _shouldMoveNext;
            private final Action<BaseData> _handleData;

            /**
             * Event fired when MoveNext returns false
            */
            public event EventHandler<EnumeratorHandler> EnumeratorFinished;

            /**
             * A unique symbol used to identify this enumerator
            */
            public final Symbol Symbol;

            /**
             * The enumerator this handler handles
            */
            public final IEnumerator<BaseData> Enumerator;

            /**
             * Determines whether or not this handler is to be used for handling the
             * data emitted. This is useful when enumerators are not for a single symbol,
             * such is the case with universe subscriptions
            */
            public final boolean HandlesData;

            /**
             * Initializes a new instance of the <see cref="EnumeratorHandler"/> class
            */
             * @param symbol The symbol to identify this enumerator
             * @param enumerator The enumeator this handler handles
             * @param shouldMoveNext Predicate function used to determine if we should call move next
             * on the symbol's enumerator
             * @param handleData Handler for data if HandlesData=true
            public EnumeratorHandler(Symbol symbol, IEnumerator<BaseData> enumerator, Func<bool> shouldMoveNext = null, Action<BaseData> handleData = null ) {
                Symbol = symbol;
                Enumerator = enumerator;
                HandlesData = handleData != null;

                _handleData = handleData ?? (data -> { });
                _shouldMoveNext = shouldMoveNext ?? (() -> true);
            }

            /**
             * Initializes a new instance of the <see cref="EnumeratorHandler"/> class
            */
             * @param symbol The symbol to identify this enumerator
             * @param enumerator The enumeator this handler handles
             * @param handlesData True if this handler will handle the data, false otherwise
            protected EnumeratorHandler(Symbol symbol, IEnumerator<BaseData> enumerator, boolean handlesData) {
                Symbol = symbol;
                HandlesData = handlesData;
                Enumerator = enumerator;

                _handleData = data -> { };
                _shouldMoveNext = () -> true;
            }

            /**
             * Event invocator for the <see cref="EnumeratorFinished"/> event
            */
            public void OnEnumeratorFinished() {
                handler = EnumeratorFinished;
                if( handler != null ) handler(this, this);
            }

            /**
             * Returns true if this enumerator should move next
            */
            public boolean ShouldMoveNext() {
                return _shouldMoveNext();
            }

            /**
             * Handles the specified data.
            */
             * @param data The data to be handled
            public void HandleData(BaseData data) {
                _handleData(data);
            }
        }
    }
}