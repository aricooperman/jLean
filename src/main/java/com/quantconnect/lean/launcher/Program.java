/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// using System;
// using System.ComponentModel.Composition;
// using System.Threading;
// using System.Windows.Forms;
// using QuantConnect.Configuration;
// using QuantConnect.Interfaces;
// using QuantConnect.Lean.Engine;
// using QuantConnect.Logging;
// using QuantConnect.Packets;
// using QuantConnect.Util;

package com.quantconnect.lean.launcher;

import java.time.LocalTime;

import org.apache.commons.lang3.tuple.Pair;
import org.apache.commons.math3.analysis.function.Log;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.Globals;
import com.quantconnect.lean.configuration.Config;
import com.quantconnect.lean.engine.LeanEngineSystemHandlers;
import com.quantconnect.lean.lean.engine.LeanEngineAlgorithmHandlers;
import com.quantconnect.lean.packets.AlgorithmNodePacket;
import com.quantconnect.lean.packets.LiveNodePacket;
import com.quantconnect.lean.util.Composer;

public class Program {

    private static final String _collapseMessage = "Unhandled exception breaking past controls and causing collapse of algorithm node. This is likely a memory leak of an external dependency or the underlying OS terminating the LEAN engine.";

    private static final Logger LOG = LoggerFactory.getLogger( Program.class );

    public static void main( final String[] args ) {
        //Initialize:
        final String mode = "RELEASE";
//            #if DEBUG //TODO add arg option handling
//                mode = "DEBUG";
//            #endif

        final String environment = Config.get( "environment" );
        final boolean liveMode = Config.getBoolean( "live-mode" );
//        LOG.debuggingEnabled = Config.GetBool( "debug-mode");
//        Log.LogHandler = Composer.Instance.GetExportedValueByTypeName<ILogHandler>(Config.Get( "log-handler", "CompositeLogHandler"));
   
            //Name thread for the profiler:
        Thread.currentThread().setName( "Algorithm Analysis Thread" );
        LOG.trace( "Engine.Main(): LEAN ALGORITHMIC TRADING ENGINE v" + Globals.getVersion() + " Mode: " + mode );
        LOG.trace( "Engine.Main(): Started " + LocalTime.now().toString() );
        final Runtime runtime = Runtime.getRuntime();
        runtime.gc();
        LOG.trace( "Engine.Main(): Memory " + runtime.freeMemory() + " free  " + runtime.totalMemory() + " Mb-Used  " + runtime.maxMemory() + " Mb-Total"); //TODO

        //Import external libraries specific to physical server location (cloud/local)
        LeanEngineSystemHandlers leanEngineSystemHandlers;
        try {
            leanEngineSystemHandlers = LeanEngineSystemHandlers.fromConfiguration( Composer.INSTANCE );
        }
        catch( final Exception compositionException ) {
            LOG.error( "Engine.Main(): Failed to load library: " + compositionException );
            throw compositionException;
        }

        //Setup packeting, queue and controls system: These don't do much locally.
        leanEngineSystemHandlers.initialize();

        //-> Pull job from QuantConnect job queue, or, pull local build:
        String assemblyPath = null;
        final Pair<String,AlgorithmNodePacket> nextJob = leanEngineSystemHandlers.getJobQueue().nextJob( assemblyPath );
        assemblyPath = nextJob.getLeft();
        final AlgorithmNodePacket job = nextJob.getRight();

        if( job == null )
            throw new NullPointerException( "Engine.Main(): Job was null." );
        
        LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
        try {
            leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.fromConfiguration( Composer.INSTANCE );
        }
        catch( final Exception compositionException ) {
            LOG.error( "Engine.Main(): Failed to load library: ", compositionException );
            throw compositionException;
        }

//            if( environment.EndsWith( "-desktop"))
//            {
//                Application.EnableVisualStyles();
//                messagingHandler = leanEngineSystemHandlers.Notify;
//                thread = new Thread(() -> LaunchUX(messagingHandler, job));
//                thread.SetApartmentState(ApartmentState.STA);
//                thread.Start();
//            }

        // log the job endpoints
        LOG.trace( "JOB HANDLERS: ");
        LOG.trace( "         DataFeed:     " + leanEngineAlgorithmHandlers.DataFeed.GetType().FullName);
        LOG.trace( "         Setup:        " + leanEngineAlgorithmHandlers.Setup.GetType().FullName);
        LOG.trace( "         RealTime:     " + leanEngineAlgorithmHandlers.RealTime.GetType().FullName);
        LOG.trace( "         Results:      " + leanEngineAlgorithmHandlers.Results.GetType().FullName);
        LOG.trace( "         Transactions: " + leanEngineAlgorithmHandlers.Transactions.GetType().FullName);
        LOG.trace( "         History:      " + leanEngineAlgorithmHandlers.HistoryProvider.GetType().FullName);
        LOG.trace( "         Commands:     " + leanEngineAlgorithmHandlers.CommandQueue.GetType().FullName);
        if( job instanceof LiveNodePacket )
            LOG.trace( "         Brokerage:    " + ((LiveNodePacket)job).Brokerage );

        // if the job version doesn't match this instance version then we can't process it
        // we also don't want to reprocess redelivered jobs
        if( VersionHelper.IsNotEqualVersion(job.Version) || job.Redelivered) {
            Log.Error( "Engine.Run(): Job Version: " + job.Version + "  Deployed Version: " + Globals.Version + " Redelivered: " + job.Redelivered);
            //Tiny chance there was an uncontrolled collapse of a server, resulting in an old user task circulating.
            //In this event kill the old algorithm and leave a message so the user can later review.
            leanEngineSystemHandlers.Api.SetAlgorithmStatus(job.AlgorithmId, AlgorithmStatus.RuntimeError, _collapseMessage);
            leanEngineSystemHandlers.Notify.SetAuthentication(job);
            leanEngineSystemHandlers.Notify.Send(new RuntimeErrorPacket(job.AlgorithmId, _collapseMessage));
            leanEngineSystemHandlers.JobQueue.AcknowledgeJob(job);
            return;
        }

        try {
            engine = new Engine( leanEngineSystemHandlers, leanEngineAlgorithmHandlers, liveMode );
            engine.run( job, assemblyPath );
        }
        finally {
            //Delete the message from the job queue:
            leanEngineSystemHandlers.getJobQueue().acknowledgeJob( job );
            LOG.trace( "Engine.Main(): Packet removed from queue: " + job.getAlgorithmId() );

            // clean up resources
            leanEngineSystemHandlers.close();
            leanEngineAlgorithmHandlers.close();
        }
    }

    //    /**
    //     * Form launcher method for thread.
    //    */
    //    static void LaunchUX( IMessagingHandler messaging, AlgorithmNodePacket job ) {
    //            //Launch the UX
    //            //form = Composer.Instance.GetExportedValueByTypeName<Form>( "desktop-ux-classname");
    //            form = new Views.WinForms.LeanWinForm(messaging, job);
    //            Application.Run(form);
    //        }
    //    }
}