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

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using QuantConnect.Logging;

package com.quantconnect.lean.configuration;

import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.fasterxml.jackson.core.type.TypeReference;
import com.quantconnect.lean.Global;

import javaslang.Lazy;

/**
 * Configuration class loads the required external setup variables to launch the Lean engine.
 */
public class Config {
    
    private static final String ENVIRONMENT_CONFIG_NAME = "environment";
    private static final String CONFIG_FILE_NAME = "config.json";
    private static final Logger LOG = LoggerFactory.getLogger( Config.class );
    private static final Lazy<Map<String,Object>> settings = Lazy.of( () -> {
        
        // initialize settings inside a lazy for free thread-safe, one-time initialization
        
        final URL configFileUrl = Config.class.getResource( CONFIG_FILE_NAME );
        if( configFileUrl != null ) {
            try {
//                final Path configPath = Paths.get( configFileUrl.toURI() );
//                if( Files.exists( configPath ) ) {
//                    return new JSONObject( Files.lines( configPath ).collect( Collectors.joining() ) );
                return Global.OBJECT_MAPPER.readValue( configFileUrl, new TypeReference<Map<String,Object>>() { } );
//                }
            }
            catch( Exception e ) {
                LOG.warn( "Unable to find config file", e );
            }
        }

        final Map<String,Object> configObject = new HashMap<>();
        configObject.put( "algorithm-type-name", "BasicTemplateAlgorithm" ); //TODO
//                {"live-mode", false},
//                {"data-folder", "../../../Data/"},
//                {"messaging-handler", "QuantConnect.Messaging.Messaging"},
//                {"queue-handler", "QuantConnect.Queues.Queues"},
//                {"api-handler", "QuantConnect.Api.Api"},
//                {"setup-handler", "QuantConnect.Lean.Engine.Setup.ConsoleSetupHandler"},
//                {"result-handler", "QuantConnect.Lean.Engine.Results.BacktestingResultHandler"},
//                {"data-feed-handler", "QuantConnect.Lean.Engine.DataFeeds.FileSystemDataFeed"},
//                {"real-time-handler", "QuantConnect.Lean.Engine.RealTime.BacktestingRealTimeHandler"},
//                {"transaction-handler", "QuantConnect.Lean.Engine.TransactionHandlers.BacktestingTransactionHandler"}

        return configObject;
    } );

    /**
     * Gets the currently selected environment. If sub-environments are defined,
     * they'll be returned as {env1}.{env2}
     * @returns The fully qualified currently selected environment
     */
    @SuppressWarnings( "unchecked")
    public static String getEnvironment() {
        final List<String> environments = new ArrayList<String>();

        Map<String,Object> currentEnvironment = settings.get(); //TODO better type checks
        String currentEnv = (String)currentEnvironment.get( ENVIRONMENT_CONFIG_NAME );
        while( currentEnvironment != null && currentEnv != null ) {
            environments.add( currentEnv );
            final Map<String,Object> moreEnvironments = (Map<String,Object>)currentEnvironment.get( "environments" );
            if( moreEnvironments == null )
                break;

            currentEnvironment = (Map<String,Object>)moreEnvironments.get( currentEnv );
            currentEnv = (String)currentEnvironment.get( ENVIRONMENT_CONFIG_NAME );
        }
        
        return environments.stream().collect( Collectors.joining( "." ) );
    }
    
    /**
     * Get the matching config setting from the file searching for this key.
     * @param key String key value we're seaching for in the config file.
     * @param defaultValue
     * @returns String value of the configuration setting or empty String if nothing found.
     */
    public static String get( String key ) {
        return get( key, "" );
    }
    
    public static String get( String key, String defaultValue ) {
        // special case environment requests
        if( key.equals( ENVIRONMENT_CONFIG_NAME ) ) 
            return getEnvironment();

        final Map<String,Object> token = getToken( settings.get(), key );
        if( token == null ) {
            LOG.trace( "Config.Get(): Configuration key not found. Key: {} - Using default value: {}", key, defaultValue );
            return defaultValue;
        }
        
        return token.toString();
    }

    /**
     * Gets the underlying JToken for the specified key
     */
    public static Map<String,Object> getToken( String key ) {
        return getToken( settings.get(), key );
    }

    /**
     * Sets a configuration value. This is really only used to help testing. The key heye can be
     * specified as {environment}.key to set a value on a specific environment
     * @param key The key to be set
     * @param value The new value
     */
    @SuppressWarnings( "unchecked")
    public static void set( String key, String value ) {
        Map<String,Object> environment = settings.get();
        while( key.contains( "." ) ) {
            String envName = key.substring( 0, key.indexOf( "." ) );
            key = key.substring( key.indexOf( "." ) + 1 );
            Map<String,Object> environments = (Map<String,Object>)environment.get( "environments" );
            if( environments == null )
                environment.put( "environments", environments = new HashMap<String,Object>() );
            environment = (Map<String,Object>)environments.get( envName );
        }
        
        environment.put( key, value );
    }

    /**
     * Get a boolean value configuration setting by a configuration key.
     * @param key String value of the configuration key.
     * @param defaultValue The default value to use if not found in configuration
     * @returns Boolean value of the config setting.
     */
    public static boolean getBoolean( String key ) {
        return getBoolean( key, false );
    }
    
    public static boolean getBoolean( String key, boolean defaultValue ) {
        return getValue( key, defaultValue );
    }

    /**
     * Get the int value of a config string.
     * @param key Search key from the config file
     * @param defaultValue The default value to use if not found in configuration
     * @returns Int value of the config setting.
     */
    public static int getInt( String key ) {
        return getInt( key, 0 );
    }
    
    public static int getInt( String key, int defaultValue ) {
        return getValue( key, defaultValue );
    }

    /**
     * Get the double value of a config string.
     * @param key Search key from the config file
     * @param defaultValue The default value to use if not found in configuration
     * @returns Double value of the config setting.
     */
    public static double getDouble( String key ) {
        return getDouble( key, 0.0 );
    }
    
    public static double getDouble( String key, double defaultValue ) {
        return getValue( key, defaultValue );
    }

    /**
     * Gets a value from configuration and converts it to the requested type, assigning a default if
     * the configuration is null or empty
     * <typeparam name="T The requested type</typeparam>
     * @param key Search key from the config file
     * @param defaultValue The default value to use if not found in configuration
     * @returns Converted value of the config setting.
     */
    public static <T> T getValue( String key ) {
        return getValue( key, null );
    }
    
    public static <T> T getValue( String key, T defaultValue ) {
        return defaultValue; //TODO
//        // special case environment requests
//        if( key.equals( "environment" ) && typeof (T) == typeof ( String)) return (T) (object) GetEnvironment();
//
//        token = GetToken(Settings.Value, key);
//        if( token == null )
//        {
//            Log.Trace( String.format( "Config.GetValue(): %1$s - Using default value: %2$s", key, defaultValue));
//            return defaultValue;
//        }
//
//        type = typeof(T);
//        String value;
//        try
//        {
//            value = token.Value<String>();
//        }
//        catch (Exception err)
//        {
//            value = token.toString();
//        }
//
//        if( type.IsEnum)
//        {
//            return (T) Enum.Parse(type, value);
//        }
//
//        if( typeof(IConvertible).IsAssignableFrom(type))
//        {
//            return (T) Convert.ChangeType(value, type);
//        }
//
//        // try and find a static parse method
//        try
//        {
//            parse = type.GetMethod( "Parse", new[]{typeof( String)});
//            if( parse != null )
//            {
//                result = parse.Invoke(null, new object[] {value});
//                return (T) result;
//            }
//        }
//        catch (Exception err)
//        {
//            Log.Trace( "Config.GetValue<%1$s>(%2$s,%3$s): Failed to parse: %4$s. Using default value.", typeof (T).Name, key, defaultValue, value);
//            Log.Error(err);
//            return defaultValue;
//        }
//
//        try
//        {
//            return JsonConvert.DeserializeObject<T>(value);
//        }
//        catch (Exception err)
//        {
//            Log.Trace( "Config.GetValue<%1$s>(%2$s,%3$s): Failed to JSON deserialize: %4$s. Using default value.", typeof(T).Name, key, defaultValue, value);
//            Log.Error(err);
//            return defaultValue;
//        }
    }

    /**
     * Tries to find the specified key and parse it as a T, using
     * default(T) if unable to locate the key or unable to parse it
     * <typeparam name="T The desired output type</typeparam>
     * @param key The configuration key
     * @param value The output value
     * @returns True on successful parse, false when output value is default(T)
     */
    public static <T> /*boolean*/ T tryGetValue( String key ) {
        return tryGetValue( key, null );
    }

    /**
     * Tries to find the specified key and parse it as a T, using
     * defaultValue if unable to locate the key or unable to parse it
     * <typeparam name="T The desired output type</typeparam>
     * @param key The configuration key
     * @param defaultValue The default value to use on key not found or unsuccessful parse
     * @param value The output value
     * @returns True on successful parse, false when output value is defaultValue
     */
    public static <T> /*boolean*/ T tryGetValue( String key, T defaultValue ) {
//        try {
//            value = 
        return getValue( key, defaultValue );
//            return true;
//        }
//        catch
//        {
//            value = defaultValue;
//            return false;
//        }
    }

    /**
     * Write the contents of the serialized configuration back to the disk.
     */
    public static void write() {
//        if( !settings.get()..IsValueCreated ) return;
//        serialized = JsonConvert.SerializeObject(Settings.Value, Formatting.Indented);
//        File.WriteAllText( "config.json", serialized);
    }

    /**
     * Flattens the jobject with respect to the selected environment and then
     * removes the 'environments' node
     * @param @OverrideEnvironment The environment to use
     * @returns The flattened JObject
     */
    public static Map<String,Object> flatten( String overrideEnvironment ) {
        return flatten( settings.get(), overrideEnvironment );
    }

    /**
     * Flattens the jobject with respect to the selected environment and then
     * removes the 'environments' node
     * @param config The configuration represented as a JObject
     * @param @OverrideEnvironment The environment to use
     * @returns The flattened JObject
     */
    public static Map<String,Object> flatten( Map<String,Object> config, String overrideEnvironment ) {
        Map<String,Object> clone = null;
//        clone = (Map<String,Object>)config.DeepClone();
//
//        // remove the environment declaration
//        environmentProperty = clone.Property( "environment");
//        if( environmentProperty != null ) environmentProperty.Remove();
//
//        if( !StringUtils.isEmpty(@OverrideEnvironment))
//        {
//            environmentSections = @OverrideEnvironment.split('.');
//
//            for (int i = 0; i < environmentSections.Length; i++)
//            {
//                env = String.join( ".environments.", environmentSections.Where((x, j) -> j <= i));
//
//                environments = config["environments"];
//                if( !(environments is JObject)) continue;
//
//                settings = ((JObject) environments).SelectToken(env);
//                if( settings == null ) continue;
//
//                // copy values for the selected environment to the root
//                foreach (token in settings)
//                {
//                    path = Path.GetExtension(token.Path);
//                    dot = path.IndexOf( ".", StringComparison.InvariantCulture);
//                    if( dot != -1) path = path.Substring(dot + 1);
//
//                    // remove if already exists on clone
//                    jProperty = clone.Property(path);
//                    if( jProperty != null ) jProperty.Remove();
//
//                    value = (token is JProperty ? ((JProperty) token).Value : token).toString();
//                    clone.Add(path, value);
//                }
//            }
//        }
//
//        // remove all environments
//        environmentsProperty = clone.Property( "environments");
//        if( environmentsProperty != null ) environmentsProperty.Remove();

        return clone ;
    }

    @SuppressWarnings( "unchecked")
    private static Map<String,Object> getToken( Map<String,Object> settings, String key ) {
        return getToken( settings, key, (Map<String,Object>)settings.get( key ) );
    }

    @SuppressWarnings( "unchecked")
    private static Map<String,Object> getToken( Map<String,Object> settings, String key, Map<String,Object> current ) {
        final String environmentSetting = (String)settings.get( ENVIRONMENT_CONFIG_NAME );
        if( StringUtils.isNotBlank( environmentSetting ) ) {
            final Map<String,Object> environment = (Map<String,Object>)settings.get( "environments." + environmentSetting );
            if( environment != null ) {
                final Map<String,Object> setting = (Map<String,Object>)environment.get( key );
                if( setting != null )
                    current = setting;
                    // allows nesting of environments, live.tradier, live.interactive, ect...
                return getToken( environment, key, current );
            }
        }
        
        if( current == null )
            return (Map<String,Object>)settings.get( key );

        return current;
    }
}
