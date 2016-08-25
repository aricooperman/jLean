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
using System.Text;
using Ionic.Zip;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Util;

package com.quantconnect.lean.ToolBox
{
    /**
     * Data writer for saving an IEnumerable of BaseData into the LEAN data directory.
    */
    public class LeanDataWriter
    {
        private final Symbol _symbol;
        private final String _market;
        private final String _dataDirectory;
        private final TickType _dataType;
        private final Resolution _resolution;
        private final SecurityType _securityType;

        /**
         * Create a new lean data writer to this base data directory.
        */
         * @param symbol Symbol string
         * @param dataDirectory Base data directory
         * @param resolution Resolution of the desired output data
         * @param dataType Write the data to trade files
        public LeanDataWriter(Resolution resolution, Symbol symbol, String dataDirectory, TickType dataType = TickType.Trade) {
            _securityType = symbol.ID.SecurityType;
            _dataDirectory = dataDirectory;
            _resolution = resolution;
            _symbol = symbol;
            _market = symbol.ID.Market.toLowerCase();
            _dataType = dataType;

            // All fx data is quote data.
            if( _securityType == SecurityType.Forex || _securityType == SecurityType.Cfd) {
                _dataType = TickType.Quote;
            }

            // Can only process Fx and equity for now
            if( _securityType != SecurityType.Equity && _securityType != SecurityType.Forex && _securityType != SecurityType.Cfd) {
                throw new Exception( "Sorry this security type is not yet supported by the LEAN data writer: " + _securityType);
            }
        }

        /**
         * Given the constructor parameters, write out the data in LEAN format.
        */
         * @param source IEnumerable source of the data: sorted from oldest to newest.
        public void Write(IEnumerable<BaseData> source) {
            switch (_resolution) {
                case Resolution.Daily:
                case Resolution.Hour:
                    WriteDailyOrHour(source);
                    break;

                case Resolution.Minute:
                case Resolution.Second:
                case Resolution.Tick:
                    WriteMinuteOrSecondOrTick(source);
                    break;
            }
        }

        /**
         * Write out the data in LEAN format (minute, second or tick resolutions)
        */
         * @param source IEnumerable source of the data: sorted from oldest to newest.
         * This function overwrites existing data files
        private void WriteMinuteOrSecondOrTick(IEnumerable<BaseData> source) {
            sb = new StringBuilder();
            lastTime = new DateTime();


            // Loop through all the data and write to file as we go
            foreach (data in source) {
                // Ensure the data is sorted
                if( data.Time < lastTime) throw new Exception( "The data must be pre-sorted from oldest to newest");

                // Based on the security type and resolution, write the data to the zip file
                if( lastTime != DateTime.MinValue && data.Time.Date > lastTime.Date) {
                    // Write and clear the file contents
                    outputFile = GetZipOutputFileName(_dataDirectory, lastTime);
                    WriteFile(outputFile, sb.toString(), lastTime);
                    sb.Clear();
                }

                lastTime = data.Time;

                // Build the line and append it to the file
                sb.Append(LeanData.GenerateLine(data, _securityType, _resolution) + Environment.NewLine);
            }

            // Write the last file
            if( sb.Length > 0) {
                outputFile = GetZipOutputFileName(_dataDirectory, lastTime);
                WriteFile(outputFile, sb.toString(), lastTime);
            }
        }

        /**
         * Write out the data in LEAN format (daily or hour resolutions)
        */
         * @param source IEnumerable source of the data: sorted from oldest to newest.
         * This function performs a merge (insert/append/overwrite) with the existing Lean zip file
        private void WriteDailyOrHour(IEnumerable<BaseData> source) {
            sb = new StringBuilder();
            lastTime = new DateTime();

            // Determine file path
            outputFile = GetZipOutputFileName(_dataDirectory, lastTime);

            // Load new data rows into a SortedDictionary for easy merge/update
            newRows = new SortedMap<DateTime,String>(source.ToDictionary(x -> x.Time, x -> LeanData.GenerateLine(x, _securityType, _resolution)));
            SortedMap<DateTime,String> rows;

            if( File.Exists(outputFile)) {
                // If file exists, we load existing data and perform merge
                rows = LoadHourlyOrDailyFile(outputFile);
                foreach (kvp in newRows) {
                    rows[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                // No existing file, just use the new data
                rows = newRows;
            }

            // Loop through the SortedDictionary and write to file contents
            foreach (kvp in rows) {
                // Build the line and append it to the file
                sb.Append(kvp.Value + Environment.NewLine);
            }

            // Write the file contents
            if( sb.Length > 0) {
                WriteFile(outputFile, sb.toString(), lastTime);
            }
        }

        /**
         * Loads an existing hourly or daily Lean zip file into a SortedDictionary
        */
        private static SortedMap<DateTime,String> LoadHourlyOrDailyFile( String fileName) {
            rows = new SortedMap<DateTime,String>();

            using (zip = ZipFile.Read(fileName)) {
                using (stream = new MemoryStream()) {
                    zip[0].Extract(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    using (reader = new StreamReader(stream)) {
                        String line;
                        while ((line = reader.ReadLine()) != null ) {
                            time = DateTime.ParseExact(line.Substring(0, DateFormat.TwelveCharacter.Length), DateFormat.TwelveCharacter, CultureInfo.InvariantCulture);
                            rows.Add(time, line);
                        }
                    }
                }
            }

            return rows;
        }

        /**
         * Write this file to disk
        */
        private void WriteFile( String fileName, String data, DateTime time) {
            data = data.TrimEnd();
            if( File.Exists(fileName)) {
                File.Delete(fileName);
                Log.Trace( "LeanDataWriter.Write(): Existing deleted: " + fileName);
            }
            // Create the directory if it doesnt exist
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            // Write out this data String to a zip file
            Compression.Zip(data, fileName, LeanData.GenerateZipEntryName(_symbol.Value, _securityType, time, _resolution, _dataType));
            Log.Trace( "LeanDataWriter.Write(): Created: " + fileName);
        }

        /**
         * Get the output zip file
        */
         * @param baseDirectory Base output directory for the zip file
         * @param time Date/time for the data we're writing
        @returns The full path to the output zip file
        private String GetZipOutputFileName( String baseDirectory, DateTime time) {
            return LeanData.GenerateZipFilePath(baseDirectory, _symbol.Value, _securityType, _market, time, _resolution);
        }

    }
}
