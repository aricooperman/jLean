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
using System.Threading.Tasks;
using QuantConnect.Logging;

package com.quantconnect.lean.Util
{
    /**
     * Controller type used to schedule <see cref="IParallelRunnerWorkItem"/> instances
     * to run on dedicated runner threads
    */
    public class ParallelRunnerController : IDisposable
    {
        private Thread _processQueueThread;

        private final int _threadCount;
        private final object _sync = new object();
        private final ManualResetEvent _waitHandle;
        private final ParallelRunnerWorker[] _workers;
        private final BlockingCollection<IParallelRunnerWorkItem> _holdQueue;
        private final BlockingCollection<IParallelRunnerWorkItem> _processQueue;

        /**
         * Gets a wait handle that can be used to wait for this controller
         * to finish all scheduled work
        */
        public WaitHandle WaitHandle
        {
            get { return _waitHandle; }
        }

        /**
         * Initializes a new instance of the <see cref="ParallelRunnerController"/> class
        */
         * @param threadCount The number of dedicated threads to spin up
        public ParallelRunnerController(int threadCount) {
            _threadCount = threadCount;
            _waitHandle = new ManualResetEvent(false);
            _workers = new ParallelRunnerWorker[threadCount];
            _holdQueue = new BlockingCollection<IParallelRunnerWorkItem>();
            _processQueue = new BlockingCollection<IParallelRunnerWorkItem>();
        }

        /**
         * Schedules the specified work item to run
        */
         * @param workItem The work item to schedule
        public void Schedule(IParallelRunnerWorkItem workItem) {
            if( workItem.IsReady) _processQueue.Add(workItem);
            else _holdQueue.Add(workItem);
        }

        /**
         * Starts this instance of <see cref="ParallelRunnerController"/>.
         * This method is indempotent
        */
         * @param token The cancellation token
        public void Start(CancellationToken token) {
            WaitHandle[] waitHandles;
            synchronized(_sync) {
                if( _workers[0] != null ) return;
                for (int i = 0; i < _threadCount; i++) {
                    worker = new ParallelRunnerWorker(this, _processQueue);
                    worker.Start(token);
                    _workers[i] = worker;
                }

                waitHandles = _workers.Select(x -> x.WaitHandle).ToArray();
            }

            Task.Run(() =>
            {
                WaitHandle.WaitAll(waitHandles);
                _waitHandle.Set();

                foreach (worker in _workers) {
                    worker.Dispose();
                }

            }, CancellationToken.None);

            _processQueueThread = new Thread(() -> ProcessHoldQueue(token));
            _processQueueThread.Start();
        }

        /**
         * Processes the internal hold queue checking to see if work
         * items are ready to run
        */
         * @param token The cancellation token
        private void ProcessHoldQueue(CancellationToken token) {
            try
            {
                foreach (workItem in _holdQueue.GetConsumingEnumerable(token)) {
                    if( workItem.IsReady) {
                        _processQueue.Add(workItem, token);
                    }
                    else
                    {
                        _holdQueue.Add(workItem, token);
                    }
                }
            }
            catch (OperationCanceledException err) {
                if( !token.IsCancellationRequested) {
                    Log.Error(err);
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            synchronized(_sync) {
                if( _holdQueue != null ) _holdQueue.Dispose();
                if( _processQueue != null ) _processQueue.Dispose();
                if( _processQueueThread != null && _processQueueThread.IsAlive) _processQueueThread.Abort();

                foreach (worker in _workers) {
                    worker.Dispose();
                }

                if( _waitHandle != null ) {
                    _waitHandle.Set();
                    _waitHandle.Dispose();
                }
            }
        }
    }
}