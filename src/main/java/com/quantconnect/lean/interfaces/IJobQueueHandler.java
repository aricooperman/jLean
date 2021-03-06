﻿/*
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
//using QuantConnect.Packets;

package com.quantconnect.lean.interfaces;

import org.apache.commons.lang3.tuple.Pair;

import com.quantconnect.lean.packets.AlgorithmNodePacket;

/**
 * Task requester interface with cloud system
 */
public interface IJobQueueHandler {
    /**
     * Initialize the internal state
     */
    void initialize();

    /**
     * Request the next task to run through the engine:
     * @returns Algorithm job to process
     */
    Pair<String,AlgorithmNodePacket> nextJob( /*out*/ String algorithmPath );

    /**
     * Signal task complete
     * @param job Work to do.
     */
    void acknowledgeJob( AlgorithmNodePacket job );
}
