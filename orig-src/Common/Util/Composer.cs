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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO;
using System.Linq;
using System.Reflection;
using QuantConnect.Configuration;
using QuantConnect.Logging;

package com.quantconnect.lean.Util
{
    /**
    /// Provides methods for obtaining exported MEF instances
    */
    public class Composer
    {
        private static final String PluginDirectory = Config.Get( "plugin-directory");

        /**
        /// Gets the singleton instance
        */
        public static final Composer Instance = new Composer();

        /**
        /// Initializes a new instance of the <see cref="Composer"/> class. This type
        /// is a light wrapper on top of an MEF <see cref="CompositionContainer"/>
        */
        public Composer() {
            Reset();
        }

        private CompositionContainer _compositionContainer;
        private final object _exportedValuesLockObject = new object();
        private final Map<Type, IEnumerable> _exportedValues = new Map<Type, IEnumerable>();

        /**
        /// Gets the export matching the predicate
        */
         * @param predicate">Function used to pick which imported instance to return, if null the first instance is returned
        @returns The only export matching the specified predicate
        public T Single<T>(Func<T, bool> predicate) {
            if( predicate == null ) {
                throw new ArgumentNullException( "predicate");
            }

            return GetExportedValues<T>().Single(predicate);
        }

        /**
        /// Adds the specified instance to this instance to allow it to be recalled via GetExportedValueByTypeName
        */
        /// <typeparam name="T">The contract type</typeparam>
         * @param instance">The instance to add
        public void AddPart<T>(T instance) {
            lock (_exportedValuesLockObject) {
                IEnumerable values;
                if( _exportedValues.TryGetValue(typeof (T), out values)) {
                    ((IList<T>) values).Add(instance);
                }
                else
                {
                    values = new List<T> {instance};
                    _exportedValues[typeof (T)] = values;
                }
            }
        }

        /**
        /// Extension method to searches the composition container for an export that has a matching type name. This function
        /// will first try to match on Type.AssemblyQualifiedName, then Type.FullName, and finally on Type.Name
        /// 
        /// This method will not throw if multiple types are found matching the name, it will just return the first one it finds.
        */
        /// <typeparam name="T">The type of the export</typeparam>
         * @param typeName">The name of the type to find. This can be an assembly qualified name, a full name, or just the type's name
        @returns The export instance
        public T GetExportedValueByTypeName<T>( String typeName)
            where T : class
        {
            try
            {
                lock (_exportedValuesLockObject) {
                    T instance;
                    IEnumerable values;
                    type = typeof(T);
                    if( _exportedValues.TryGetValue(type, out values)) {
                        // if we've alread loaded this part, then just return the same one
                        instance = values.OfType<T>().FirstOrDefault(x -> x.GetType().MatchesTypeName(typeName));
                        if( instance != null ) {
                            return instance;
                        }
                    }

                    // we want to get the requested part without instantiating each one of that type
                    selectedPart = _compositionContainer.Catalog.Parts
                        .Select(x -> new { part = x, Type = ReflectionModelServices.GetPartType(x).Value })
                        .Where(x -> type.IsAssignableFrom(x.Type))
                        .Where(x -> x.Type.MatchesTypeName(typeName))
                        .Select(x -> x.part)
                        .FirstOrDefault();

                    if( selectedPart == null ) {
                        throw new ArgumentException(
                            "Unable to locate any exports matching the requested typeName: " + typeName, "typeName");
                    }

                    exportDefinition =
                        selectedPart.ExportDefinitions.First(
                            x -> x.ContractName == AttributedModelServices.GetContractName(type));
                    instance = (T)selectedPart.CreatePart().GetExportedValue(exportDefinition);

                    // cache the new value for next time
                    if( values == null ) {
                        values = new List<T> { instance };
                        _exportedValues[type] = values;
                    }
                    else
                    {
                        ((List<T>)values).Add(instance);
                    }

                    return instance;
                }
            } 
            catch (ReflectionTypeLoadException err) {
                foreach (exception in err.LoaderExceptions) {
                    Log.Error(exception);
                    Log.Error(exception.toString());
                }

                if( err.InnerException != null ) Log.Error(err.InnerException);

                throw;
            }
        }
        /**
        /// Gets all exports of type T
        */
        public IEnumerable<T> GetExportedValues<T>() {
            lock (_exportedValuesLockObject) {
                IEnumerable values;
                if( _exportedValues.TryGetValue(typeof (T), out values)) {
                    return values.OfType<T>();
                }

                values = _compositionContainer.GetExportedValues<T>().ToList();
                _exportedValues[typeof (T)] = values;
                return values.OfType<T>();
            }
        }

        /**
        /// Clears the cache of exported values, causing new instances to be created.
        */
        public void Reset() {
            lock(_exportedValuesLockObject) {
                // grab assemblies from current executing directory
                catalogs = new List<ComposablePartCatalog>
                {
                    new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.dll"),
                    new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.exe")
                };
                if( !string.IsNullOrWhiteSpace(PluginDirectory) && Directory.Exists(PluginDirectory) && new DirectoryInfo(PluginDirectory).FullName != AppDomain.CurrentDomain.BaseDirectory) {
                    catalogs.Add(new DirectoryCatalog(PluginDirectory, "*.dll"));
                }
                aggregate = new AggregateCatalog(catalogs);
                _compositionContainer = new CompositionContainer(aggregate);
                _exportedValues.Clear();
            }
        }
    }
}
