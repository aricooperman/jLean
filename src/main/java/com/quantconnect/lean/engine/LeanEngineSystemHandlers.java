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

package com.quantconnect.lean.engine;

import java.io.Closeable;

import com.quantconnect.lean.configuration.Config;
import com.quantconnect.lean.interfaces.IApi;

//using System;
//using System.ComponentModel.Composition;
//using QuantConnect.Configuration;
//using QuantConnect.Interfaces;
//using QuantConnect.Util;


/// Provides a container for the system level handlers
public class LeanEngineSystemHandlers implements Closeable {
    private /*final*/ IApi _api;
    private /*final*/ IMessagingHandler _notify;
    private /*final*/ IJobQueueHandler _jobQueue;

    /**
    /// Gets the api instance used for communicating algorithm limits, status, and storing of log data
    */
    public IApi getApi() {
        return _api;
    }

    /**
    /// Gets the messaging handler instance used for communicating various packets to listeners, including
    /// debug/log messages, email/sms/web messages, as well as results and run time errors
    */
    public IMessagingHandler getNotify() {
        return _notify;
    }

    /**
    /// Gets the job queue responsible for acquiring and acknowledging an algorithm job
    */
    public IJobQueueHandler getJobQueue() {
        return _jobQueue;
    }

    /**
    /// Initializes a new instance of the <see cref="LeanEngineSystemHandlers"/> class with the specified handles
    */
     * @param jobQueue">The job queue used to acquire algorithm jobs
     * @param api">The api instance used for communicating limits and status
     * @param notify">The messaging handler user for passing messages from the algorithm to listeners
    public LeanEngineSystemHandlers(IJobQueueHandler jobQueue, IApi api, IMessagingHandler notify) {
        if( jobQueue == null ) {
            throw new ArgumentNullException( "jobQueue");
        }
        if( api == null ) {
            throw new ArgumentNullException( "api");
        }
        if( notify == null ) {
            throw new ArgumentNullException( "notify");
        }
        _api = api;
        _jobQueue = jobQueue;
        _notify = notify;
    }

    /**
    /// Creates a new instance of the <see cref="LeanEngineSystemHandlers"/> class from the specified composer using type names from configuration
    */
     * @param composer">The composer instance to obtain implementations from
    @returns A fully hydrates <see cref="LeanEngineSystemHandlers"/> instance.
    /// <exception cref="CompositionException">Throws a CompositionException during failure to load</exception>
    public static LeanEngineSystemHandlers FromConfiguration(Composer composer) {
        return new LeanEngineSystemHandlers(
            composer.GetExportedValueByTypeName<IJobQueueHandler>(Config.Get( "job-queue-handler")),
            composer.GetExportedValueByTypeName<IApi>(Config.Get( "api-handler")),
            composer.GetExportedValueByTypeName<IMessagingHandler>(Config.Get( "messaging-handler"))
            );
    }

    /// Initializes the Api, Messaging, and JobQueue components
    public void initialize() {
        Api.Initialize(Config.GetInt( "job-user-id", 0), Config.Get( "api-access-token", ""));
        Notify.Initialize();
        JobQueue.Initialize();
    }

    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    public void close() {
        Api.Dispose();
    }
}

/*
using System;
using System.ComponentModel.Composition;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine
{
    /**
    /// Provides a container for the system level handlers
    */
    public class LeanEngineSystemHandlers : IDisposable
    {
        private final IApi _api;
        private final IMessagingHandler _notify;
        private final IJobQueueHandler _jobQueue;

        /**
        /// Gets the api instance used for communicating algorithm limits, status, and storing of log data
        */
        public IApi Api
        {
            get { return _api; }
        }

        /**
        /// Gets the messaging handler instance used for communicating various packets to listeners, including
        /// debug/log messages, email/sms/web messages, as well as results and run time errors
        */
        public IMessagingHandler Notify
        {
            get { return _notify; }
        }

        /**
        /// Gets the job queue responsible for acquiring and acknowledging an algorithm job
        */
        public IJobQueueHandler JobQueue
        {
            get { return _jobQueue; }
        }

        /**
        /// Initializes a new instance of the <see cref="LeanEngineSystemHandlers"/> class with the specified handles
        */
         * @param jobQueue">The job queue used to acquire algorithm jobs
         * @param api">The api instance used for communicating limits and status
         * @param notify">The messaging handler user for passing messages from the algorithm to listeners
        public LeanEngineSystemHandlers(IJobQueueHandler jobQueue, IApi api, IMessagingHandler notify) {
            if( jobQueue == null ) {
                throw new ArgumentNullException( "jobQueue");
            }
            if( api == null ) {
                throw new ArgumentNullException( "api");
            }
            if( notify == null ) {
                throw new ArgumentNullException( "notify");
            }
            _api = api;
            _jobQueue = jobQueue;
            _notify = notify;
        }

        /**
        /// Creates a new instance of the <see cref="LeanEngineSystemHandlers"/> class from the specified composer using type names from configuration
        */
         * @param composer">The composer instance to obtain implementations from
        @returns A fully hydrates <see cref="LeanEngineSystemHandlers"/> instance.
        /// <exception cref="CompositionException">Throws a CompositionException during failure to load</exception>
        public static LeanEngineSystemHandlers FromConfiguration(Composer composer) {
            return new LeanEngineSystemHandlers(
                composer.GetExportedValueByTypeName<IJobQueueHandler>(Config.Get( "job-queue-handler")),
                composer.GetExportedValueByTypeName<IApi>(Config.Get( "api-handler")),
                composer.GetExportedValueByTypeName<IMessagingHandler>(Config.Get( "messaging-handler"))
                );
        }

        /**
        /// Initializes the Api, Messaging, and JobQueue components
        */
        public void Initialize() {
            Api.Initialize(Config.GetInt( "job-user-id", 0), Config.Get( "api-access-token", ""));
            Notify.Initialize();
            JobQueue.Initialize();
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
            Api.Dispose();
        }
    }
}
*/