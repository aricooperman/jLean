using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

package com.quantconnect.lean.Packets
{
    /**
     * Packet for history jobs
    */
    public class HistoryPacket : Packet
    {
        /**
         * The queue where the data should be sent
        */
        public String QueueName;

        /**
         * The individual requests to be processed
        */
        public List<HistoryRequest> Requests = new List<HistoryRequest>();

        /**
         * Initializes a new instance of the <see cref="HistoryPacket"/> class
        */
        public HistoryPacket()
            : base(PacketType.History) {
        }
    }

    /**
     * Specifies request parameters for a single historical request.
     * A HistoryPacket is made of multiple requests for data. These
     * are used to request data during live mode from a data server
    */
    public class HistoryRequest
    {
        /**
         * The start time to request data in UTC
        */
        public DateTime StartTimeUtc;

        /**
         * The end time to request data in UTC
        */
        public DateTime EndTimeUtc;

        /**
         * The symbol to request data for
        */
        public Symbol Symbol;

        /**
         * The symbol's security type
        */
        public SecurityType SecurityType;

        /**
         * The requested resolution
        */
        public Resolution Resolution;

        /**
         * The market the symbol belongs to
        */
        public String Market;
    }

    /**
     * Specifies various types of history results
    */
    public enum HistoryResultType
    {
        /**
         * The requested file data
        */
        File,

        /**
         * The request's status
        */
        Status,

        /**
         * The request is completed
        */
        Completed,

        /**
         * The request had an error
        */
        Error
    }

    /**
     * Provides a container for results from history requests. This contains
     * the file path relative to the /Data folder where the data can be written
    */
    public abstract class HistoryResult
    {
        /**
         * Gets the type of history result
        */
        public HistoryResultType Class { get; private set; }

        /**
         * Initializes a new instance of the <see cref="HistoryResult"/> class
        */
         * @param type The type of history result
        protected HistoryResult(HistoryResultType type) {
            Class = type;
        }
    }

    /**
     * Defines requested file data for a history request
    */
    public class FileHistoryResult : HistoryResult
    {
        /**
         * The relative file path where the data should be written
        */
        public String Filepath;

        /**
         * The file's contents, this is a zipped csv file
        */
        public byte[] File;

        /**
         * Default constructor for serializers
        */
        public FileHistoryResult()
            : base(HistoryResultType.File) {
        }

        /**
         * Initializes a new instance of the <see cref="HistoryResult"/> class
        */
         * @param filepath The relative file path where the file should be written, rooted in /Data, so for example ./forex/fxcm/daily/eurusd.zip
         * @param file The zipped csv file content in bytes
        public FileHistoryResult( String filepath, byte[] file)
            : this() {
            Filepath = filepath;
            File = file;
        }
    }

    /**
     * Specifies the completed message from a history result
    */
    public class CompletedHistoryResult : HistoryResult
    {
        /**
         * Initializes a new instance of <see cref="CompletedHistoryResult"/> class
        */
        public CompletedHistoryResult()
            : base(HistoryResultType.Completed) {
        }
    }

    /**
     * Specfies an error message in a history result
    */
    public class ErrorHistoryResult : HistoryResult
    {
        /**
         * Gets the error that was encountered
        */
        public String Message;

        /**
         * Default constructor for serializers
        */
        public ErrorHistoryResult()
            : base(HistoryResultType.Error) {
        }

        /**
         * Initializes a new instance of the <see cref="ErrorHistoryResult"/> class
        */
         * @param message The error message
        public ErrorHistoryResult( String message)
            : this() {
            Message = message;
        }
    }

    /**
     * Specifies the progress of a request
    */
    public class StatusHistoryResult : HistoryResult
    {
        /**
         * Gets the progress of the request
        */
        public int Progress;

        /**
         * Default constructor for serializers
        */
        public StatusHistoryResult()
            : base(HistoryResultType.Status) {
        }

        /**
         * Initializes a new instance of the <see cref="StatusHistoryResult"/> class
        */
         * @param progress The progress, from 0 to 100
        public StatusHistoryResult(int progress)
            : this() {
            Progress = progress;
        }
    }
}
