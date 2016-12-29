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
import java.io.IOException;

import com.quantconnect.lean.configuration.Config;
import com.quantconnect.lean.interfaces.IApi;
import com.quantconnect.lean.interfaces.IJobQueueHandler;
import com.quantconnect.lean.interfaces.IMessagingHandler;
import com.quantconnect.lean.util.Composer;

//using System.ComponentModel.Composition;

/**
 * Provides a container for the system level handlers
 */
public class LeanEngineSystemHandlers implements Closeable {
    private final IApi api;
    private final IMessagingHandler notify;
    private final IJobQueueHandler jobQueue;

    /**
     * Gets the api instance used for communicating algorithm limits, status, and storing of log data
     */
    public IApi getApi() {
        return api;
    }

    /**
     * Gets the messaging handler instance used for communicating various packets to listeners, including
     * debug/log messages, email/sms/web messages, as well as results and run time errors
     */
    public IMessagingHandler getNotify() {
        return notify;
    }

    /**
     * Gets the job queue responsible for acquiring and acknowledging an algorithm job
     */
    public IJobQueueHandler getJobQueue() {
        return jobQueue;
    }

    /**
     * Initializes a new instance of the <see cref="LeanEngineSystemHandlers"/> class with the specified handles
     * @param jobQueue The job queue used to acquire algorithm jobs
     * @param api The api instance used for communicating limits and status
     * @param notify The messaging handler user for passing messages from the algorithm to listeners
     */
    public LeanEngineSystemHandlers( final IJobQueueHandler jobQueue, final IApi api, final IMessagingHandler notify ) {
        if( jobQueue == null )
            throw new NullPointerException( "jobQueue");
        
        if( api == null )
            throw new NullPointerException( "api");
        
        if( notify == null )
            throw new NullPointerException( "notify");
        
        this.api = api;
        this.jobQueue = jobQueue;
        this.notify = notify;
    }

    /**
     * Creates a new instance of the <see cref="LeanEngineSystemHandlers"/> class from the specified composer using type names from configuration
     * @param composer The composer instance to obtain implementations from
     * @returns A fully hydrates <see cref="LeanEngineSystemHandlers"/> instance.
     * @throws CompositionException Throws a CompositionException during failure to load
    */
    public static LeanEngineSystemHandlers fromConfiguration( final Composer composer ) {
        return new LeanEngineSystemHandlers(
                composer.<IJobQueueHandler>getExportedValueByTypeName( Config.get( "job-queue-handler" ) ),
                composer.<IApi>getExportedValueByTypeName( Config.get( "api-handler" ) ),
                composer.<IMessagingHandler>getExportedValueByTypeName( Config.get( "messaging-handler") )
                );
    }

    /**
     * Initializes the Api, Messaging, and JobQueue components
     */
    public void initialize() {
        api.initialize( Config.getInt( "job-user-id", 0 ), Config.get( "api-access-token", "" ) );
        notify.initialize();
        jobQueue.initialize();
    }

    /**
     * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
     */
    @Override
    public void close() {
        try {
            api.close();
        }
        catch( final IOException e ) { }
    }
}