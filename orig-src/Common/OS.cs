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
using System.Diagnostics;
using System.IO;

package com.quantconnect.lean 
{
    /**
     * Operating systems class for managing anything that is operation system specific.
    */
     * Good design should remove the need for this function. Over time it should disappear.
    public static class OS 
    {
        private static PerformanceCounter _ramTotalCounter;
        private static PerformanceCounter _ramAvailableBytes;
        private static PerformanceCounter _cpuUsageCounter;

        /**
         * Total Physical Ram on the Machine:
        */
        private static PerformanceCounter RamTotalCounter 
        {
            get 
            {
                if( _ramTotalCounter == null ) {
                    if( IsLinux) {
                        _ramTotalCounter = new PerformanceCounter ( "Mono Memory", "Total Physical Memory"); 
                    } 
                    else 
                    {
                        _ramTotalCounter = new PerformanceCounter( "Memory", "Available Bytes");
                    }
                }
                return _ramTotalCounter;
            }
        }

        /**
         * Memory free on the machine available for use:
        */
        public static PerformanceCounter RamAvailableBytes 
        {
            get 
            {
                if( _ramAvailableBytes == null ) {
                    if( IsLinux) { 
                        _ramAvailableBytes = new PerformanceCounter( "Mono Memory", "Allocated Objects");
                    } 
                    else 
                    {
                        _ramAvailableBytes = new PerformanceCounter( "Memory", "Available Bytes");
                    }
                }
                return _ramAvailableBytes;
            }
        }

        /**
         * Total CPU usage as a percentage
        */
        public static PerformanceCounter CpuUsage
        {
            get
            {
                if( _cpuUsageCounter == null ) {
                    _cpuUsageCounter = new PerformanceCounter( "Processor", "% Processor Time", "_Total");
                }
                return _cpuUsageCounter;
            }
        }

        /**
         * Global Flag :: Operating System
        */
        public static boolean IsLinux 
        {
            get
            {
                p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        /**
         * Global Flag :: Operating System
        */
        public static boolean IsWindows
        {
            get 
            {
                return !IsLinux;
            }
        }


        /**
         * Character Separating directories in this OS:
        */
        public static String PathSeparation 
        {
            get
            {
                return Path.DirectorySeparatorChar.toString();
            }
        }

        /**
         * Get the drive space remaining on windows and linux in MB
        */
        public static long DriveSpaceRemaining 
        { 
            get 
            {
                d = GetDrive();
                return d.AvailableFreeSpace / (1024 * 1024);
            }
        }

        /**
         * Get the drive space remaining on windows and linux in MB
        */
        public static long DriveSpaceUsed
        {
            get
            {
                d = GetDrive();
                return (d.TotalSize - d.AvailableFreeSpace) / (1024 * 1024);
            }
        }


        /**
         * Total space on the drive
        */
        public static long DriveTotalSpace
        {
            get
            {
                d = GetDrive();
                return d.TotalSize / (1024 * 1024);
            }
        }

        /**
         * Get the drive.
        */
        @returns 
        private static DriveInfo GetDrive() {
            drives = DriveInfo.GetDrives();
            return drives[0];
        }

        /**
         * Get the RAM remaining on the machine:
        */
        public static long ApplicationMemoryUsed 
        {
            get
            {
                proc = Process.GetCurrentProcess();
                return (proc.PrivateMemorySize64 / (1024*1024));
            }
        }

        /**
         * Get the RAM remaining on the machine:
        */
        public static long TotalPhysicalMemory {
            get {
                return (long)(RamTotalCounter.NextValue() / (1024*1024));
            }
        }

        /**
         * Get the RAM used on the machine:
        */
        public static long TotalPhysicalMemoryUsed
        {
            get 
            {
                return GC.GetTotalMemory(false) / (1024*1024);
            }
        }

        /**
         * Gets the RAM remaining on the machine
        */
        private static long FreePhysicalMemory
        {
            get { return TotalPhysicalMemory - TotalPhysicalMemoryUsed; }
        }

        /**
         * Gets the statistics of the machine, including CPU% and RAM
        */
        public static Map<String,String> GetServerStatistics() {
            return new Map<String,String>
            {
                {"CPU Usage",            CpuUsage.NextValue().toString( "0.0") + "%"},
                {"Used RAM (MB)",        TotalPhysicalMemoryUsed.toString()},
                {"Total RAM (MB)",        TotalPhysicalMemory.toString()},
                {"Used Disk Space (MB)", DriveSpaceUsed.toString() },
                {"Total Disk Space (MB)", DriveTotalSpace.toString() },
                {"LEAN Version", "v" + Globals.Version}
            };
        }
    } // End OS Class
} // End QC Namespace
