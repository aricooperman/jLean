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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using Newtonsoft.Json.Linq;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Securities;
using QuantConnect.Util;
using Log = QuantConnect.Logging.Log;

package com.quantconnect.lean.ToolBox.CoarseUniverseGenerator
{
    public static class Program
    {
        private static final String ExclusionsFile = "exclusions.txt";

        /**
         * This program generates the coarse files requires by lean for universe selection.
         * Universe selection is planned to happen in two stages, the first stage, the 'coarse'
         * stage serves to cull the set using coarse filters, such as price, market, and dollar volume.
         * Later we'll support full fundamental data such as ratios and financial statements, and these
         * would be run AFTER the initial coarse filter
         * 
         * The files are generated from LEAN formatted daily trade bar equity files
        */
         * @param args Unused argument
        public static void Main( String[] args) {
            // read out the configuration file
            JToken jtoken;
            config = JObject.Parse(File.ReadAllText( "CoarseUniverseGenerator/config.json"));

            ignoreMaplessSymbols = false;
            updateMode = false;
            updateTime = Duration.ZERO;
            if( config.TryGetValue( "update-mode", out jtoken)) {
                updateMode = jtoken.Value<bool>();
                if( config.TryGetValue( "update-time-of-day", out jtoken)) {
                    updateTime = TimeSpan.Parse(jtoken.Value<String>());
                }
            }

            dataDirectory = Globals.DataFolder;
            if( config.TryGetValue( "data-directory", out jtoken)) {
                dataDirectory = jtoken.Value<String>();
            }

            //Ignore symbols without a map file:
            // Typically these are nothing symbols (NASDAQ test symbols, or symbols listed for a few days who aren't actually ever traded).
            if( config.TryGetValue( "ignore-mapless", out jtoken)) {
                ignoreMaplessSymbols = jtoken.Value<bool>();
            }

            do
            {
                ProcessEquityDirectories(dataDirectory, ignoreMaplessSymbols);
            }
            while (WaitUntilTimeInUpdateMode(updateMode, updateTime));
        }

        /**
         * If we're in update mode, pause the thread until the next update time
        */
         * @param updateMode True for update mode, false for run-once
         * @param updateTime The time of day updates should be performed
        @returns True if in update mode, otherwise false
        private static boolean WaitUntilTimeInUpdateMode( boolean updateMode, Duration updateTime) {
            if( !updateMode) return false;

            now = DateTime.Now;
            timeUntilNextProcess = (now.Date.AddDays(1).Add(updateTime) - now);
            Thread.Sleep((int)timeUntilNextProcess.TotalMilliseconds);
            return true;
        }

        /**
         * Iterates over each equity directory and aggregates the data into the coarse file
        */
         * @param dataDirectory The Lean /Data directory
         * @param ignoreMaplessSymbols Ignore symbols without a QuantQuote map file.
        public static void ProcessEquityDirectories( String dataDirectory, boolean ignoreMaplessSymbols) {
            exclusions = ReadExclusionsFile(ExclusionsFile);

            equity = Path.Combine(dataDirectory, "equity");
            foreach (directory in Directory.EnumerateDirectories(equity)) {
                dailyFolder = Path.Combine(directory, "daily");
                mapFileFolder = Path.Combine(directory, "map_files");
                coarseFolder = Path.Combine(directory, "fundamental", "coarse");
                if( !Directory.Exists(coarseFolder)) {
                    Directory.CreateDirectory(coarseFolder);
                }

                lastProcessedDate = GetStartDate(coarseFolder);
                ProcessDailyFolder(dailyFolder, coarseFolder, MapFileResolver.Create(mapFileFolder), exclusions, ignoreMaplessSymbols, lastProcessedDate);
            }
        }

        /**
         * Iterates each daily file in the specified <paramref name="dailyFolder"/> and adds a line for each
         * day to the approriate coarse file
        */
         * @param dailyFolder The folder with daily data
         * @param coarseFolder The coarse output folder
         * @param mapFileResolver">
         * @param exclusions The symbols to be excluded from processing
         * @param ignoreMapless Ignore the symbols without a map file.
         * @param symbolResolver Function used to provide symbol resolution. Default resolution uses the zip file name to resolve
         * the symbol, specify null for this behavior.
        @returns A collection of the generated coarse files
        public static ICollection<String> ProcessDailyFolder( String dailyFolder, String coarseFolder, MapFileResolver mapFileResolver, HashSet<String> exclusions, boolean ignoreMapless, DateTime startDate, Func<String,String> symbolResolver = null ) {
            static final BigDecimal scaleFactor = 10000m;

            Log.Trace( "Processing: %1$s", dailyFolder);

            start = DateTime.UtcNow;

            // load map files into memory

            symbols = 0;
            maplessCount = 0;
            dates = new HashSet<DateTime>();

            // instead of opening/closing these constantly, open them once and dispose at the end (~3x speed improvement)
            writers = new Map<String, StreamWriter>();

            dailyFolderDirectoryInfo = new DirectoryInfo(dailyFolder).Parent;
            if( dailyFolderDirectoryInfo == null ) {
                throw new Exception( "Unable to resolve market for daily folder: " + dailyFolder);
            }
            market = dailyFolderDirectoryInfo.Name.toLowerCase();

            // open up each daily file to get the values and append to the daily coarse files
            foreach (file in Directory.EnumerateFiles(dailyFolder)) {
                try
                {
                    symbol = Path.GetFileNameWithoutExtension(file);
                    if( symbol == null ) {
                        Log.Trace( "CoarseGenerator.ProcessDailyFolder(): Unable to resolve symbol from file: %1$s", file);
                        continue;
                    }

                    if( symbolResolver != null ) {
                        symbol = symbolResolver(symbol);
                    }

                    symbol = symbol.toUpperCase();

                    if( exclusions.Contains(symbol)) {
                        Log.Trace( "Excluded symbol: %1$s", symbol);
                        continue;
                    }

                    ZipFile zip;
                    using (reader = Compression.Unzip(file, out zip)) {
                        // 30 period EMA constant
                        static final BigDecimal k = 2m / (30 + 1);

                        seeded = false;
                        runningAverageVolume = BigDecimal.ZERO;

                        checkedForMapFile = false;

                        symbols++;
                        String line;
                        while ((line = reader.ReadLine()) != null ) {
                            //20150625.csv
                            csv = line.split(',');
                            date = DateTime.ParseExact(csv[0], DateFormat.TwelveCharacter, CultureInfo.InvariantCulture);
                            
                            // spin past old data
                            if( date < startDate) continue;

                            if( ignoreMapless && !checkedForMapFile) {
                                checkedForMapFile = true;
                                if( !mapFileResolver.ResolveMapFile(symbol, date).Any()) {
                                    // if the resolved map file has zero entries then it's a mapless symbol
                                    maplessCount++;
                                    break;
                                }
                            }

                            close = decimal.Parse(csv[4])/scaleFactor;
                            volume = long.Parse(csv[5]);

                            // compute the current volume EMA for dollar volume calculations
                            runningAverageVolume = seeded
                                ? volume*k + runningAverageVolume*(1 - k)
                                : volume;

                            seeded = true;

                            dollarVolume = close * runningAverageVolume;

                            coarseFile = Path.Combine(coarseFolder, date.toString( "yyyyMMdd") + ".csv");
                            dates.Add(date);

                            // try to resolve a map file and if found, regen the sid
                            sid = SecurityIdentifier.GenerateEquity(SecurityIdentifier.DefaultDate, symbol, market);
                            mapFile = mapFileResolver.ResolveMapFile(symbol, date);
                            if( !mapFile.IsNullOrEmpty()) {
                                // if available, us the permtick in the coarse files, because of this, we need
                                // to update the coarse files each time new map files are added/permticks change
                                sid = SecurityIdentifier.GenerateEquity(mapFile.FirstDate, mapFile.OrderBy(x -> x.Date).First().MappedSymbol, market);
                            }
                            if( mapFile == null && ignoreMapless) {
                                // if we're ignoring mapless files then we should always be able to resolve this
                                Log.Error( String.format( "CoarseGenerator.ProcessDailyFolder(): Unable to resolve map file for %1$s as of %2$s", symbol, date.ToShortDateString()));
                                continue;
                            }

                            // sid,symbol,close,volume,dollar volume
                            coarseFileLine = sid + "," + symbol + "," + close + "," + volume + "," + Math.Truncate(dollarVolume);

                            StreamWriter writer;
                            if( !writers.TryGetValue(coarseFile, out writer)) {
                                writer = new StreamWriter(new FileStream(coarseFile, FileMode.Create, FileAccess.Write, FileShare.Write));
                                writers[coarseFile] = writer;
                            }
                            writer.WriteLine(coarseFileLine);
                        }
                    }

                    if( symbols%1000 == 0) {
                        Log.Trace( "CoarseGenerator.ProcessDailyFolder(): Completed processing %1$s symbols. Current elapsed: %2$s seconds", symbols, (DateTime.UtcNow - start).TotalSeconds.toString( "0.00"));
                    }
                }
                catch (Exception err) {
                    // log the error and continue with the process
                    Log.Error(err.toString());
                }
            }

            Log.Trace( "CoarseGenerator.ProcessDailyFolder(): Saving %1$s coarse files to disk", dates.Count);

            // dispose all the writers at the end of processing
            foreach (writer in writers) {
                writer.Value.Dispose();
            }

            stop = DateTime.UtcNow;

            Log.Trace( "CoarseGenerator.ProcessDailyFolder(): Processed %1$s symbols into %2$s coarse files in %3$s seconds", symbols, dates.Count, (stop - start).TotalSeconds.toString( "0.00"));
            Log.Trace( "CoarseGenerator.ProcessDailyFolder(): Excluded %1$s mapless symbols.", maplessCount);

            return writers.Keys;
        }

        /**
         * Reads the specified exclusions file into a new hash set.
         * Returns an empty set if the file does not exist
        */
        public static HashSet<String> ReadExclusionsFile( String exclusionsFile) {
            exclusions = new HashSet<String>();
            if( File.Exists(exclusionsFile)) {
                excludedSymbols = File.ReadLines(exclusionsFile).Select(x -> x.Trim()).Where(x -> !x.StartsWith( "#"));
                exclusions = new HashSet<String>(excludedSymbols, StringComparer.InvariantCultureIgnoreCase);
                Log.Trace( "CoarseGenerator.ReadExclusionsFile(): Loaded %1$s symbols into the exclusion set", exclusions.Count);
            }
            return exclusions;
        }

        /**
         * Resolves the start date that should be used in the <see cref="ProcessDailyFolder"/>. This will
         * be equal to the latest file date (20150101.csv) plus one day
        */
         * @param coarseDirectory The directory containing the coarse files
        @returns The last coarse file date plus one day if exists, else DateTime.MinValue
        public static DateTime GetStartDate( String coarseDirectory) {
            lastProcessedDate = (
                from coarseFile in Directory.EnumerateFiles(coarseDirectory)
                let date = TryParseCoarseFileDate(coarseFile)
                where date != null
                // we'll start on the following day
                select date.Value.AddDays(1)
                ).DefaultIfEmpty(DateTime.MinValue).Max();

            return lastProcessedDate;
        }

        private static DateTime? TryParseCoarseFileDate( String coarseFile) {
            try
            {
                dateString = Path.GetFileNameWithoutExtension(coarseFile);
                return DateTime.ParseExact(dateString, "yyyyMMdd", null );
            }
            catch
            {
                return null;
            }
        }
    }
}
