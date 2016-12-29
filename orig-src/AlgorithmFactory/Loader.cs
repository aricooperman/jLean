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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ImpromptuInterface;
using IronPython.Hosting;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;

package com.quantconnect.lean.AlgorithmFactory 
{
    /**
     * Loader creates and manages the memory and exception space of the algorithm, ensuring if it explodes the Lean Engine is intact.
    */
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Loader : MarshalByRefObject
    {
        // Defines the maximum amount of time we will allow for instantiating an instance of IAlgorithm
        private final Duration _loaderTimeLimit;

        // Language of the loader class.
        private final Language _language;

        // Location of the IronPython standard library
        private final String _ironPythonLibrary = Config.Get( "ironpython-location", "../ironpython/Lib");

        // Defines how we resolve a list of type names into a single type name to be instantiated
        private final Func<List<String>,String> _multipleTypeNameResolverFunction;

        /**
         * Memory space of the user algorithm
        */
        public AppDomain appDomain;

        /**
         * The algorithm's interface type that we'll be trying to load
        */
        private static final Class AlgorithmInterfaceType = typeof (IAlgorithm);

        /**
         * The full type name of QCAlgorithm, this is so we don't pick him up when querying for types
        */
        private static final String AlgorithmBaseTypeFullName = "QuantConnect.Algorithm.QCAlgorithm";

        /**
         * Creates a new loader with a 10 second maximum load time that forces exactly one derived type to be found
        */
        public Loader()
            : this(Language.CSharp, Duration.ofSeconds(10), names -> names.SingleOrDefault()) {
        }

        /**
         * Creates a new loader with the specified configuration
        */
         * @param language Which language are we trying to load
         * @param loaderTimeLimit">
         * Used to limit how long it takes to create a new instance
         * 
         * @param multipleTypeNameResolverFunction">
         * Used to resolve multiple type names found in assembly to a single type name, if null, defaults to names -> names.SingleOrDefault()
         * 
         * When we search an assembly for derived types of IAlgorithm, sometimes the assembly will contain multiple matching types. This is the case
         * for the QuantConnect.Algorithm assembly in this solution.  In order to pick the correct type, consumers must specify how to pick the type,
         * that's what this function does, it picks the correct type from the list of types found within the assembly.
         * 
        public Loader(Language language, Duration loaderTimeLimit, Func<List<String>,String> multipleTypeNameResolverFunction) {
            _language = language;

            if( multipleTypeNameResolverFunction == null ) {
                throw new NullPointerException( "multipleTypeNameResolverFunction");
            }

            _loaderTimeLimit = loaderTimeLimit;
            _multipleTypeNameResolverFunction = multipleTypeNameResolverFunction;
        }


        /**
         * Creates a new instance of the specified class in the library, safely.
        */
         * @param assemblyPath Location of the DLL
         * @param algorithmInstance Output algorithm instance
         * @param errorMessage Output error message on failure
        @returns Bool true on successfully loading the class.        
        public boolean TryCreateAlgorithmInstance( String assemblyPath, out IAlgorithm algorithmInstance, out String errorMessage) {
            //Default initialisation of Assembly.
            algorithmInstance = null;
            errorMessage = "";

            //First most basic check:
            if( !File.Exists(assemblyPath)) {
                return false;
            }

            switch (_language) {
                case Language.Python:
                    TryCreatePythonAlgorithm(assemblyPath, out algorithmInstance, out errorMessage);
                    break;

                default:
                    TryCreateILAlgorithm(assemblyPath, out algorithmInstance, out errorMessage);
                    break;
            }

            //Successful load.
            return algorithmInstance != null;
        }


        /**
         * Create a new instance of a python algorithm
        */
         * @param assemblyPath">
         * @param algorithmInstance">
         * @param errorMessage">
        @returns 
        private boolean TryCreatePythonAlgorithm( String assemblyPath, out IAlgorithm algorithmInstance, out String errorMessage) {
            success = false;
            algorithmInstance = null;
            errorMessage = "";

            try
            {
                //Create the python engine
                engine = Python.CreateEngine();
                paths = engine.GetSearchPaths();
                paths.Add(_ironPythonLibrary);
                engine.SetSearchPaths(paths);

                //Load the dll - built with clr.Compiler()
                Log.Trace( "Loader.TryCreatePythonAlgorithm(): Loading python assembly: " + assemblyPath);
                library = Assembly.LoadFile(Path.GetFullPath(assemblyPath));
                engine.Runtime.LoadAssembly(library);

                //Import the python dll: requires a main.py file to serve as starting point for the algorithm.
                items = new List<KeyValuePair<String, dynamic>>();
                try
                {
                    Log.Trace( "Loader.TryCreatePythonAlgorithm(): Importing python module...");
                    algorithmName = Config.Get( "algorithm-type-name");
                    scope = engine.Runtime.ImportModule( String.IsNullOrEmpty(algorithmName) ? "main" : algorithmName);
                    items = (List<KeyValuePair<String, dynamic>>)scope.GetItems();
                }
                catch (Exception err) {
                    Log.Error(err);
                    errorMessage = err.Message + " - could not locate 'main' module. Please make sure you have a main.py file in your project.";
                    return false;
                }

                //Loop through the types in the dll, see if we can find a "QCAlgorithm" base class
                Log.Trace( "Loader.TryCreatePythonAlgorithm(): Finding QCAlgorithm...");
                dynamic dynamicAlgorithm = null;
                foreach (item in items) {
                    try
                    {
                        String baseName = item.Value.__bases__.toString().toString();
                        if( baseName.Contains( "QCAlgorithm")) {
                            dynamicAlgorithm = item.Value;
                        }
                    }
                    catch (Exception) { 
                        //Suppress the error messages
                    }
                }

                //If we haven't found it yet
                if( dynamicAlgorithm == null ) {
                    errorMessage = "Could not find QCAlgorithm class in your project";
                    return false;
                }

                //Cast DLR object to an IAlgorithm instance with Impromptu
                Log.Trace( "Loader.TryCreatePythonAlgorithm(): Creating IAlgorithm instance...");
                dynamic instance = engine.Operations.CreateInstance(dynamicAlgorithm);
                algorithmInstance = Impromptu.ActLike<IAlgorithm>(instance);
                success = true;
            }
            catch (Exception err) {
                Log.Error(err);
            }

            return success && (algorithmInstance != null );
        }


        /**
         * Create a generic IL algorithm 
        */
         * @param assemblyPath">
         * @param algorithmInstance">
         * @param errorMessage">
        @returns 
        private boolean TryCreateILAlgorithm( String assemblyPath, out IAlgorithm algorithmInstance, out String errorMessage) {
            errorMessage = "";
            algorithmInstance = null;

            try
            {
                byte[] debugInformationBytes = null;
                
                // if the assembly is located in the base directory then don't bother loading the pdbs
                // manually, they'll be loaded automatically by the .NET runtime.
                if( new FileInfo(assemblyPath).DirectoryName == AppDomain.CurrentDomain.BaseDirectory) {
                    // see if the pdb exists
                    mdbFilename = assemblyPath + ".mdb";
                    pdbFilename = assemblyPath.Substring(0, assemblyPath.Length - 4) + ".pdb";
                    if( File.Exists(pdbFilename)) {
                        debugInformationBytes = File.ReadAllBytes(pdbFilename);
                    }
                    // see if the mdb exists
                    if( File.Exists(mdbFilename)) {
                        debugInformationBytes = File.ReadAllBytes(mdbFilename);
                    }
                }

                //Load the assembly:
                Assembly assembly;
                if( debugInformationBytes == null ) {
                    Log.Trace( "Loader.TryCreateILAlgorithm(): Loading only the algorithm assembly");
                    assembly = Assembly.LoadFrom(assemblyPath);
                }
                else
                {
                    Log.Trace( "Loader.TryCreateILAlgorithm(): Loading debug information with algorithm");
                    assemblyBytes = File.ReadAllBytes(assemblyPath);
                    assembly = Assembly.Load(assemblyBytes, debugInformationBytes);
                }
                if( assembly == null ) {
                    errorMessage = "Assembly is null.";
                    Log.Error( "Loader.TryCreateILAlgorithm(): Assembly is null");
                    return false;
                }

                //Get the list of extention classes in the library: 
                types = GetExtendedTypeNames(assembly);
                Log.Debug( "Loader.TryCreateILAlgorithm(): Assembly types: " + String.join( ",", types));

                //No extensions, nothing to load.
                if( types.Count == 0) {
                    errorMessage = "Algorithm type was not found.";
                    Log.Error( "Loader.TryCreateILAlgorithm(): Types array empty, no algorithm type found.");
                    return false;
                }

                if( types.Count > 1) {
                    // reshuffle type[0] to the resolved typename
                    types[0] = _multipleTypeNameResolverFunction.Invoke(types);

                    if(  String.IsNullOrEmpty(types[0])) {
                        errorMessage = "Please verify algorithm type name matches the algorithm name in the configuration file. Unable to resolve multiple algorithm types to a single type.";
                        Log.Error( "Loader.TryCreateILAlgorithm(): Failed resolving multiple algorithm types to a single type.");
                        return false;
                    }
                }
                //Load the assembly into this AppDomain:
                algorithmInstance = (IAlgorithm)assembly.CreateInstance(types[0], true);

                if( algorithmInstance != null ) {
                    Log.Trace( "Loader.TryCreateILAlgorithm(): Loaded " + algorithmInstance.GetType().Name);
                }

            }
            catch (ReflectionTypeLoadException err) {
                Log.Error(err);
                Log.Error( "Loader.TryCreateILAlgorithm(1): " + err.LoaderExceptions[0]);
                if( err.InnerException != null ) errorMessage = err.InnerException.Message;
            }
            catch (Exception err) {
                Log.Error(err);
                if( err.InnerException != null ) errorMessage = err.InnerException.Message;
            }

            return true;
        }

        /**
         * Get a list of all the matching type names in this DLL assembly:
        */
         * @param assembly Assembly dll we're loading.
        @returns String list of types available.
        public static List<String> GetExtendedTypeNames(Assembly assembly) {
            types = new List<String>();
            try
            {
                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e) {
                    assemblyTypes = e.Types;
                }

                if( assemblyTypes != null && assemblyTypes.Length > 0) {
                    types = (from t in assemblyTypes
                             where t.IsClass                                    // require class
                             where !t.IsAbstract                                // require concrete impl
                             where AlgorithmInterfaceType.IsAssignableFrom(t)   // require derived from IAlgorithm
                             where t.FullName != AlgorithmBaseTypeFullName      // require not equal to QuantConnect.QCAlgorithm
                             where t.GetConstructor(Type.EmptyTypes) != null    // require default ctor
                             select t.FullName).ToList();
                }
                else
                {
                    Log.Error( "API.GetExtendedTypeNames(): No types found in assembly.");
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }

            return types;
        }

        /**
         * Creates a new instance of the class in the library, safely.
        */
         * @param assemblyPath Location of the DLL
         * @param algorithmInstance Output algorithm instance
         * @param errorMessage Output error message on failure
        @returns bool success     
        public boolean TryCreateAlgorithmInstanceWithIsolator( String assemblyPath, out IAlgorithm algorithmInstance, out String errorMessage) {
            IAlgorithm instance = null;
            error = string.Empty;

            success = false;
            isolator = new Isolator();
            complete = isolator.ExecuteWithTimeLimit(_loaderTimeLimit, () =>
            {
                success = TryCreateAlgorithmInstance(assemblyPath, out instance, out error);
            });

            algorithmInstance = instance;
            errorMessage = error;

            // if the isolator stopped us early add that to our error message
            if( !complete) {
                errorMessage = "Failed to create algorithm instance within 10 seconds. Try re-building algorithm. " + error;
            }

            return complete && success && algorithmInstance != null;
        }


        /**
         * Unload this factory's appDomain.
        */
         * Not used in lean engine. Running the library in an app domain is 10x slower.
         * <seealso cref="CreateAppDomain"/>
        public void Unload() {
            if( appDomain != null ) {
                AppDomain.Unload(appDomain);
                appDomain = null;
            }
        }

    } // End Algorithm Factory Class

} // End QC Namespace.
