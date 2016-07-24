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

package com.quantconnect.lean.interfaces;

//using System.ComponentModel.Composition;
//using QuantConnect.Notifications;
//using QuantConnect.Packets;

/// Messaging System Plugin Interface. 
/// Provides a common messaging pattern between desktop and cloud implementations of QuantConnect.
public interface IMessagingHandler {

    /// Gets or sets whether this messaging handler has any current subscribers.
    /// When set to false, messages won't be sent.
    boolean hasSubscribers();
    
    void setSubscribers( boolean send );

    /// Initialize the Messaging System Plugin. 
    void initialize();

    /// Set the user communication channel
    /// <param name="job"></param>
    void setAuthentication( AlgorithmNodePacket job );

    /// Send any message with a base type of Packet.
    /// <param name="packet">Packet of data to send via the messaging system plugin</param>
    void send( Packet packet );

    /// Send any notification with a base type of Notification.
    /// <param name="notification">The notification to be sent.</param>
    void sendNotification( Notification notification );
}

/*

using System.ComponentModel.Composition;
using QuantConnect.Notifications;
using QuantConnect.Packets;

namespace QuantConnect.Interfaces
{
    /// <summary>
    /// Messaging System Plugin Interface. 
    /// Provides a common messaging pattern between desktop and cloud implementations of QuantConnect.
    /// </summary>
    [InheritedExport(typeof(IMessagingHandler))]
    public interface IMessagingHandler
    {
        /// <summary>
        /// Gets or sets whether this messaging handler has any current subscribers.
        /// When set to false, messages won't be sent.
        /// </summary>
        boolean HasSubscribers { get; set; }

        /// <summary>
        /// Initialize the Messaging System Plugin. 
        /// </summary>
        void Initialize();

        /// <summary>
        /// Set the user communication channel
        /// </summary>
        /// <param name="job"></param>
        void SetAuthentication(AlgorithmNodePacket job);

        /// <summary>
        /// Send any message with a base type of Packet.
        /// </summary>
        /// <param name="packet">Packet of data to send via the messaging system plugin</param>
        void Send(Packet packet);

        /// <summary>
        /// Send any notification with a base type of Notification.
        /// </summary>
        /// <param name="notification">The notification to be sent.</param>
        void SendNotification(Notification notification);
    }
}
*/