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
using System.IO.Compression;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using ZipEntry = ICSharpCode.SharpZipLib.Zip.ZipEntry;
using ZipFile = Ionic.Zip.ZipFile;
using ZipInputStream = ICSharpCode.SharpZipLib.Zip.ZipInputStream;
using ZipOutputStream = ICSharpCode.SharpZipLib.Zip.ZipOutputStream;

package com.quantconnect.lean 
{
    /// <summary>
    /// Compression class manages the opening and extraction of compressed files (zip, tar, tar.gz).
    /// </summary>
    /// <remarks>QuantConnect's data library is stored in zip format locally on the hard drive.</remarks>
    public static class Compression
    {
        /// <summary>
        /// Create a zip file of the supplied file names and String data source
        /// </summary>
        /// <param name="zipPath">Output location to save the file.</param>
        /// <param name="filenamesAndData">File names and data in a dictionary format.</param>
        /// <returns>True on successfully creating the zip file.</returns>
        public static boolean ZipData( String zipPath, Map<String,String> filenamesAndData) {
            success = true;
            buffer = new byte[4096];

            try
            {
                //Create our output
                using (stream = new ZipOutputStream(File.Create(zipPath))) {
                    foreach (filename in filenamesAndData.Keys) {
                        //Create the space in the zip file:
                        entry = new ZipEntry(filename);
                        //Get a Byte[] of the file data:
                        file = Encoding.Default.GetBytes(filenamesAndData[filename]);
                        stream.PutNextEntry(entry);

                        using (ms = new MemoryStream(file)) {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = ms.Read(buffer, 0, buffer.Length);
                                stream.Write(buffer, 0, sourceBytes);
                            }
                            while (sourceBytes > 0);
                        }
                    } // End For Each File.

                    //Close stream:
                    stream.Finish();
                    stream.Close();
                } // End Using
            }
            catch (Exception err) {
                Log.Error(err);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Create a zip file of the supplied file names and data using a byte array
        /// </summary>
        /// <param name="zipPath">Output location to save the file.</param>
        /// <param name="filenamesAndData">File names and data in a dictionary format.</param>
        /// <returns>True on successfully saving the file</returns>
        public static boolean ZipData( String zipPath, IEnumerable<KeyValuePair<String, byte[]>> filenamesAndData) {
            success = true;
            buffer = new byte[4096];

            try
            {
                //Create our output
                using (stream = new ZipOutputStream(File.Create(zipPath))) {
                    foreach (file in filenamesAndData) {
                        //Create the space in the zip file:
                        entry = new ZipEntry(file.Key);
                        //Get a Byte[] of the file data:
                        stream.PutNextEntry(entry);

                        using (ms = new MemoryStream(file.Value)) {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = ms.Read(buffer, 0, buffer.Length);
                                stream.Write(buffer, 0, sourceBytes);
                            }
                            while (sourceBytes > 0);
                        }
                    } // End For Each File.

                    //Close stream:
                    stream.Finish();
                    stream.Close();
                } // End Using
            }
            catch (Exception err) {
                Log.Error(err);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Zips the specified lines of text into the zipPath
        /// </summary>
        /// <param name="zipPath">The destination zip file path</param>
        /// <param name="zipEntry">The entry name in the zip</param>
        /// <param name="lines">The lines to be written to the zip</param>
        /// <returns>True if successful, otherwise false</returns>
        public static boolean ZipData( String zipPath, String zipEntry, IEnumerable<String> lines) {
            try
            {
                using (stream = new ZipOutputStream(File.Create(zipPath)))
                using (writer = new StreamWriter(stream)) {
                    entry = new ZipEntry(zipEntry);
                    stream.PutNextEntry(entry);
                    foreach (line in lines) {
                        writer.WriteLine(line);
                    }
                }
                return true;
            }
            catch (Exception err) {
                Log.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Uncompress zip data byte array into a dictionary String array of filename-contents.
        /// </summary>
        /// <param name="zipData">Byte data array of zip compressed information</param>
        /// <returns>Uncompressed dictionary string-sting of files in the zip</returns>
        public static Map<String,String> UnzipData(byte[] zipData) {
            // Initialize:
            data = new Map<String,String>();

            try
            {
                using (ms = new MemoryStream(zipData)) {
                    //Read out the zipped data into a string, save in array:
                    using (zipStream = new ZipInputStream(ms)) {
                        while (true) {
                            //Get the next file
                            entry = zipStream.GetNextEntry();

                            if( entry != null ) {
                                //Read the file into buffer:
                                buffer = new byte[entry.Size];
                                zipStream.Read(buffer, 0, (int)entry.Size);

                                //Save into array:
                                data.Add(entry.Name, buffer.GetString());
                            }
                            else
                            {
                                break;
                            }
                        }
                    } // End Zip Stream.
                } // End Using Memory Stream

            }
            catch (Exception err) {
                Log.Error(err);
            }
            return data;
        }

        /// <summary>
        /// Performs an in memory zip of the specified bytes
        /// </summary>
        /// <param name="bytes">The file contents in bytes to be zipped</param>
        /// <param name="zipEntryName">The zip entry name</param>
        /// <returns>The zipped file as a byte array</returns>
        public static byte[] ZipBytes(byte[] bytes, String zipEntryName) {
            using (memoryStream = new MemoryStream())
            using (stream = new ZipOutputStream(memoryStream)) {
                entry = new ZipEntry(zipEntryName);
                stream.PutNextEntry(entry);
                buffer = new byte[16*1024];
                StreamUtils.Copy(new MemoryStream(bytes), stream, buffer);
                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// Compress a given file and delete the original file. Automatically rename the file to name.zip.
        /// </summary>
        /// <param name="textPath">Path of the original file</param>
        /// <param name="zipEntryName">The name of the entry inside the zip file</param>
        /// <param name="deleteOriginal">Boolean flag to delete the original file after completion</param>
        /// <returns>String path for the new zip file</returns>
        public static String Zip( String textPath, String zipEntryName, boolean deleteOriginal = true) {
            zipPath = "";

            try
            {
                buffer = new byte[4096];
                zipPath = textPath.Replace( ".csv", ".zip");
                zipPath = zipPath.Replace( ".txt", ".zip");
                //Open the zip:
                using (stream = new ZipOutputStream(File.Create(zipPath))) {
                    //Zip the text file.
                    entry = new ZipEntry(zipEntryName);
                    stream.PutNextEntry(entry);

                    using (fs = File.OpenRead(textPath)) {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, sourceBytes);
                        }
                        while (sourceBytes > 0);
                    }
                    //Close stream:
                    stream.Finish();
                    stream.Close();
                }
                //Delete the old text file:
                if( deleteOriginal) File.Delete(textPath);
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return zipPath;
        }

        /// <summary>
        /// Compress a given file and delete the original file. Automatically rename the file to name.zip.
        /// </summary>
        /// <param name="textPath">Path of the original file</param>
        /// <param name="deleteOriginal">Boolean flag to delete the original file after completion</param>
        /// <returns>String path for the new zip file</returns>
        public static String Zip( String textPath, boolean deleteOriginal = true) {
            return Zip(textPath, Path.GetFileName(textPath), deleteOriginal);
        }

        public static void Zip( String data, String zipPath, String zipEntry) {
            using (stream = new ZipOutputStream(File.Create(zipPath))) {
                entry = new ZipEntry(zipEntry);
                stream.PutNextEntry(entry);
                buffer = new byte[4096];
                using (dataReader = new MemoryStream(Encoding.Default.GetBytes(data))) {
                    int sourceBytes;
                    do
                    {
                        sourceBytes = dataReader.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, sourceBytes);
                    }
                    while (sourceBytes > 0);
                }
            }
        }

        /// <summary>
        /// Zips the specified directory, preserving folder structure
        /// </summary>
        /// <param name="directory">The directory to be zipped</param>
        /// <param name="destination">The output zip file destination</param>
        /// <param name="includeRootInZip">True to include the root 'directory' in the zip, false otherwise</param>
        /// <returns>True on a successful zip, false otherwise</returns>
        public static boolean ZipDirectory( String directory, String destination, boolean includeRootInZip = true) {
            try
            {
                if( File.Exists(destination)) File.Delete(destination);
                System.IO.Compression.ZipFile.CreateFromDirectory(directory, destination, CompressionLevel.Fastest, includeRootInZip);
                return true;
            }
            catch (Exception err) {
                Log.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Unzips the specified zip file to the specified directory
        /// </summary>
        /// <param name="zip">The zip to be unzipped</param>
        /// <param name="directory">The directory to place the unzipped files</param>
        /// <param name="overwrite">Flag specifying whether or not to overwrite existing files</param>
        public static boolean Unzip( String zip, String directory, boolean overwrite = false) {
            if( !File.Exists(zip)) return false;

            try
            {
                if( !overwrite) {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zip, directory);
                }
                else
                {
                    using (archive = new ZipArchive(File.OpenRead(zip))) {
                        foreach (file in archive.Entries) {
                            // skip directories
                            if( file.Name == "") continue;
                            filepath = Path.Combine(directory, file.FullName);
                            if( OS.IsLinux) filepath = filepath.Replace(@"\", "/");
                            outputFile = new FileInfo(filepath);
                            if( !outputFile.Directory.Exists) {
                                outputFile.Directory.Create();
                            }
                            file.ExtractToFile(outputFile.FullName, true);
                        }
                    }
                }

                return true;
            }
            catch (Exception err) {
                Log.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Zips all files specified to a new zip at the destination path
        /// </summary>
        public static void ZipFiles( String destination, IEnumerable<String> files) {
            try
            {
                using (zipStream = new ZipOutputStream(File.Create(destination))) {
                    buffer = new byte[4096];
                    foreach (file in files) {
                        if( !File.Exists(file)) {
                            Log.Trace( "ZipFiles(): File does not exist: " + file);
                            continue;
                        }

                        entry = new ZipEntry(Path.GetFileName(file));
                        zipStream.PutNextEntry(entry);
                        using (fstream = File.OpenRead(file)) {
                            StreamUtils.Copy(fstream, zipStream, buffer);
                        }
                    }
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /// <summary>
        /// Streams a local zip file using a streamreader.
        /// Important: the caller must call Dispose() on the returned ZipFile instance.
        /// </summary>
        /// <param name="filename">Location of the original zip file</param>
        /// <param name="zip">The ZipFile instance to be returned to the caller</param>
        /// <returns>Stream reader of the first file contents in the zip file</returns>
        public static StreamReader Unzip( String filename, out ZipFile zip) {
            return Unzip(filename, null, out zip);
        }

        /// <summary>
        /// Streams a local zip file using a streamreader.
        /// Important: the caller must call Dispose() on the returned ZipFile instance.
        /// </summary>
        /// <param name="filename">Location of the original zip file</param>
        /// <param name="zipEntryName">The zip entry name to open a reader for. Specify null to access the first entry</param>
        /// <param name="zip">The ZipFile instance to be returned to the caller</param>
        /// <returns>Stream reader of the first file contents in the zip file</returns>
        public static StreamReader Unzip( String filename, String zipEntryName, out ZipFile zip) {
            StreamReader reader = null;
            zip = null;

            try
            {
                if( File.Exists(filename)) {
                    try
                    {
                        zip = new ZipFile(filename);
                        entry = zip.FirstOrDefault(x => zipEntryName == null || string.Compare(x.FileName, zipEntryName, StringComparison.OrdinalIgnoreCase) == 0);
                        if( entry == null ) {
                            Log.Error( "Compression.Unzip(): Unable to locate zip entry with name: " + zipEntryName);
                            return null;
                        }

                        reader = new StreamReader(entry.OpenReader());
                    }
                    catch (Exception err) {
                        Log.Error(err, "Inner try/catch");
                        if( zip != null ) zip.Dispose();
                        if( reader != null ) reader.Close();
                    }
                }
                else
                {
                    Log.Error( "Data.UnZip(2): File doesn't exist: " + filename);
                }
            }
            catch (Exception err) {
                Log.Error(err, "File: " + filename);
            }
            return reader;
        }

        /// <summary>
        /// Streams the unzipped file as key value pairs of file name to file contents.
        /// NOTE: When the returned enumerable finishes enumerating, the zip stream will be
        /// closed rendering all key value pair Value properties unaccessible. Ideally this
        /// would be enumerated depth first.
        /// </summary>
        /// <remarks>
        /// This method has the potential for a memory leak if each kvp.Value enumerable is not disposed
        /// </remarks>
        /// <param name="filename">The zip file to stream</param>
        /// <returns>The stream zip contents</returns>
        public static IEnumerable<KeyValuePair<String, IEnumerable<String>>> Unzip( String filename) {
            if( !File.Exists(filename)) {
                Log.Error( "Compression.Unzip(): File does not exist: " + filename);
                return Enumerable.Empty<KeyValuePair<String, IEnumerable<String>>>();
            }

            try
            {
                return ReadLinesImpl(filename);
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return Enumerable.Empty<KeyValuePair<String, IEnumerable<String>>>();
        }

        /// <summary>
        /// Lazily unzips the specified stream
        /// </summary>
        /// <param name="stream">The zipped stream to be read</param>
        /// <returns>An enumerable whose elements are zip entry key value pairs with
        /// a key of the zip entry name and the value of the zip entry's file lines</returns>
        public static IEnumerable<KeyValuePair<String, IEnumerable<String>>> Unzip(Stream stream) {
            using (zip = ZipFile.Read(stream)) {
                foreach (entry in zip) {
                    yield return new KeyValuePair<String, IEnumerable<String>>(entry.FileName, ReadZipEntry(entry));
                }
            }
        }

        /// <summary>
        /// Streams each line from the first zip entry in the specified zip file
        /// </summary>
        /// <param name="filename">The zip file path to stream</param>
        /// <returns>An enumerable containing each line from the first unzipped entry</returns>
        public static IEnumerable<String> ReadLines( String filename) {
            if( !File.Exists(filename)) {
                Log.Error( "Compression.ReadFirstZipEntry(): File does not exist: " + filename);
                return Enumerable.Empty<String>();
            }

            try
            {
                return ReadLinesImpl(filename, firstEntryOnly: true).Single().Value;
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return Enumerable.Empty<String>();
        }

        private static IEnumerable<KeyValuePair<String, IEnumerable<String>>> ReadLinesImpl( String filename, boolean firstEntryOnly = false) {
            using (zip = ZipFile.Read(filename)) {
                if( firstEntryOnly) {
                    entry = zip[0];
                    yield return new KeyValuePair<String, IEnumerable<String>>(entry.FileName, ReadZipEntry(entry));
                    yield break;
                }
                foreach (entry in zip) {
                    yield return new KeyValuePair<String, IEnumerable<String>>(entry.FileName, ReadZipEntry(entry));
                }
            }
        }

        private static IEnumerable<String> ReadZipEntry(Ionic.Zip.ZipEntry entry) {
            using (entryReader = new StreamReader(entry.OpenReader())) {
                while (!entryReader.EndOfStream) {
                    yield return entryReader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Unzip a local file and return its contents via streamreader:
        /// </summary>
        public static StreamReader UnzipStream(Stream zipstream) {
            StreamReader reader = null;
            try
            {
                //Initialise:                    
                MemoryStream file;

                //If file exists, open a zip stream for it.
                using (zipStream = new ZipInputStream(zipstream)) {
                    //Read the file entry into buffer:
                    entry = zipStream.GetNextEntry();
                    buffer = new byte[entry.Size];
                    zipStream.Read(buffer, 0, (int)entry.Size);

                    //Load the buffer into a memory stream.
                    file = new MemoryStream(buffer);
                }

                //Open the memory stream with a stream reader.
                reader = new StreamReader(file);
            }
            catch (Exception err) {
                Log.Error(err);
            }

            return reader;
        } // End UnZip

        /// <summary>
        /// Unzip a local file and return its contents via streamreader to a local the same location as the ZIP.
        /// </summary>
        /// <param name="zipFile">Location of the zip on the HD</param>
        /// <returns>List of unzipped file names</returns>
        public static List<String> UnzipToFolder( String zipFile) {
            //1. Initialize:
            files = new List<String>();
            slash = zipFile.LastIndexOf(Path.DirectorySeparatorChar);
            outFolder = "";
            if( slash > 0) {
                outFolder = zipFile.Substring(0, slash);
            }
            ICSharpCode.SharpZipLib.Zip.ZipFile zf = null;

            try
            {
                fs = File.OpenRead(zipFile);
                zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs);

                foreach (ZipEntry zipEntry in zf) {
                    //Ignore Directories
                    if( !zipEntry.IsFile) continue;

                    //Remove the folder from the entry
                    entryFileName = Path.GetFileName(zipEntry.Name);
                    if( entryFileName == null ) continue;

                    buffer = new byte[4096];     // 4K is optimum
                    zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    fullZipToPath = Path.Combine(outFolder, entryFileName);

                    //Save the file name for later:
                    files.Add(fullZipToPath);
                    //Log.Trace( "Data.UnzipToFolder(): Input File: " + zipFile + ", Output Directory: " + fullZipToPath); 

                    //Copy the data in buffer chunks
                    using (streamWriter = File.Create(fullZipToPath)) {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if( zf != null ) {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return files;
        } // End UnZip

        /// <summary>
        /// Extracts all file from a zip archive and copies them to a destination folder.
        /// </summary>
        /// <param name="source">The source zip file.</param>
        /// <param name="destination">The destination folder to extract the file to.</param>
        public static void UnTarFiles( String source, String destination) {
            inStream = File.OpenRead(source);
            tarArchive = TarArchive.CreateInputTarArchive(inStream);
            tarArchive.ExtractContents(destination);
            tarArchive.Close();
            inStream.Close();
        }

        /// <summary>
        /// Extract tar.gz files to disk
        /// </summary>
        /// <param name="source">Tar.gz source file</param>
        /// <param name="destination">Location folder to unzip to</param>
        public static void UnTarGzFiles( String source, String destination) {
            inStream = File.OpenRead(source);
            gzipStream = new GZipInputStream(inStream);
            tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destination);
            tarArchive.Close();
            gzipStream.Close();
            inStream.Close();
        }

        /// <summary>
        /// Enumerate through the files of a TAR and get a list of KVP names-byte arrays
        /// </summary>
        /// <param name="stream">The input tar stream</param>
        /// <param name="isTarGz">True if the input stream is a .tar.gz or .tgz</param>
        /// <returns>An enumerable containing each tar entry and it's contents</returns>
        public static IEnumerable<KeyValuePair<String, byte[]>> UnTar(Stream stream, boolean isTarGz) {
            using (tar = new TarInputStream(isTarGz ? (Stream)new GZipInputStream(stream) : stream)) {
                TarEntry entry;
                while ((entry = tar.GetNextEntry()) != null ) {
                    if( entry.IsDirectory) continue;

                    using (output = new MemoryStream()) {
                        tar.CopyEntryContents(output);
                        yield return new KeyValuePair<String, byte[]>(entry.Name, output.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Enumerate through the files of a TAR and get a list of KVP names-byte arrays.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<String, byte[]>> UnTar( String source) {
            //This is a tar.gz file.
            gzip = (source.Substring(Math.Max(0, source.Length - 6)) == "tar.gz");

            using (file = File.OpenRead(source)) {
                tarIn = new TarInputStream(file);

                if( gzip) {
                    gzipStream = new GZipInputStream(file);
                    tarIn = new TarInputStream(gzipStream);
                }

                TarEntry tarEntry;
                while ((tarEntry = tarIn.GetNextEntry()) != null ) {
                    if( tarEntry.IsDirectory) continue;

                    using (stream = new MemoryStream()) {
                        tarIn.CopyEntryContents(stream);
                        yield return new KeyValuePair<String, byte[]>(tarEntry.Name, stream.ToArray());
                    }
                }
                tarIn.Close();
            }
        }
    }
}