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
*/

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using QuantConnect.Commands;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;

package com.quantconnect.lean.Queues
{
    /**
    /// Represents a command queue handler that sources it's commands from
    /// a file on the local disk
    */
    public class FileCommandQueueHandler : ICommandQueueHandler
    {
        private final String _commandJsonFilePath;
        private final Queue<ICommand> _commands = new Queue<ICommand>();

        /**
        /// Initializes a new instance of the <see cref="FileCommandQueueHandler"/> class
        /// using the 'command-json-file' configuration value for the command json file
        */
        public FileCommandQueueHandler()
            : this(Config.Get( "command-json-file", "command.json")) {
        }

        /**
        /// Initializes a new instance of the <see cref="FileCommandQueueHandler"/> class
        */
         * @param commandJsonFilePath">The file path to the commands json file
        public FileCommandQueueHandler( String commandJsonFilePath) {
            _commandJsonFilePath = commandJsonFilePath;
        }

        /**
        /// Initializes this command queue for the specified job
        */
         * @param job">The job that defines what queue to bind to
         * @param algorithm">The algorithm instance
        public void Initialize(AlgorithmNodePacket job, IAlgorithm algorithm) {
        }

        /**
        /// Gets the next command in the queue
        */
        @returns The next command in the queue, if present, null if no commands present
        public IEnumerable<ICommand> GetCommands() {
            if( File.Exists(_commandJsonFilePath)) {
                // update the queue by reading the command file
                ReadCommandFile();
            }

            while (_commands.Count != 0) {
                yield return _commands.Dequeue();
            }
        }

        /**
        /// Reads the commnd file on disk and populates the queue with the commands
        */
        private void ReadCommandFile() {
            object deserialized;
            try
            {
                if( !File.Exists(_commandJsonFilePath)) return;
                contents = File.ReadAllText(_commandJsonFilePath);
                deserialized = JsonConvert.DeserializeObject(contents, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            }
            catch (Exception err) {
                Log.Error(err);
                deserialized = null;
            }

            // remove the file when we're done reading it
            File.Delete(_commandJsonFilePath);

            // try it as an enumerable
            enumerable = deserialized as IEnumerable<ICommand>;
            if( enumerable != null ) {
                foreach (command in enumerable) {
                    _commands.Enqueue(command);
                }
                return;
            }
            
            // try it as a single command
            item = deserialized as ICommand;
            if( item != null ) {
                _commands.Enqueue(item);
            }
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
        }
    }
}
