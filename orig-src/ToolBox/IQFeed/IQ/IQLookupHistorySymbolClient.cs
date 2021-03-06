﻿/*
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

using System;
using System.Globalization;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.IQFeed
{
    // Historical stock data lookup events
    public class LookupTickEventArgs : LookupEventArgs
    {
        public LookupTickEventArgs( String requestId, String line) :
            base(requestId, LookupType.REQ_HST_TCK, LookupSequence.MessageDetail) {
            fields = line.split(',');
            if( fields.Length < 11) {
                Log.Error( "LookupIntervalEventArgs.ctor(): " + line);
                return;
            }
            if( !DateTime.TryParseExact(fields[1], "yyyy-MM-dd HH:mm:ss", _enUS, DateTimeStyles.None, out _dateTimeStamp)) _dateTimeStamp = DateTime.MinValue;
            if( !double.TryParse(fields[2], out _last)) _last = 0;
            if( !int.TryParse(fields[3], out _lastSize)) _lastSize = 0;
            if( !int.TryParse(fields[4], out _totalVolume)) _totalVolume = 0;
            if( !double.TryParse(fields[5], out _bid)) _bid = 0;
            if( !double.TryParse(fields[6], out _ask)) _ask = 0;
            if( !int.TryParse(fields[7], out _tickId)) _tickId = 0;
            if( !char.TryParse(fields[10], out _basis)) _basis = ' ';
        }
        public DateTime DateTimeStamp { get { return _dateTimeStamp; } }
        public double Last { get { return _last; } }
        public int LastSize { get { return _lastSize; } }
        public int TotalVolume { get { return _totalVolume; } }
        public double Bid { get { return _bid; } }
        public double Ask { get { return _ask; } }
        public int TickId { get { return _tickId; } }
        public char Basis { get { return _basis; } } 

        #region private
        private DateTime _dateTimeStamp;
        private double _last;
        private int _lastSize;
        private int _totalVolume;
        private double _bid;
        private double _ask;
        private int _tickId;
        private char _basis;
        private CultureInfo _enUS = new CultureInfo( "en-US");
        #endregion
    }
    public class LookupIntervalEventArgs : LookupEventArgs
    {
        public LookupIntervalEventArgs( String requestId, String line) :
            base(requestId, LookupType.REQ_HST_INT, LookupSequence.MessageDetail) {
            fields = line.split(',');
            if( fields.Length < 8) {
                Log.Error( "LookupIntervalEventArgs.ctor(): " + line);
                return;
            }
            if( !DateTime.TryParseExact(fields[1], "yyyy-MM-dd HH:mm:ss", _enUS, DateTimeStyles.None, out _dateTimeStamp)) _dateTimeStamp = DateTime.MinValue;
            if( !double.TryParse(fields[2], out _high)) _high = 0;
            if( !double.TryParse(fields[3], out _low)) _low = 0;
            if( !double.TryParse(fields[4], out _open)) _open = 0;
            if( !double.TryParse(fields[5], out _close)) _close = 0;
            if( !int.TryParse(fields[6], out _totalVolume)) _totalVolume = 0;
            if( !int.TryParse(fields[7], out _periodVolume)) _periodVolume = 0;
        }
        public DateTime DateTimeStamp { get { return _dateTimeStamp; } }
        public double High { get { return _high; } }
        public double Low { get { return _low; } }
        public double Open { get { return _open; } }
        public double Close { get { return _close; } }
        public int TotalVolume { get { return _totalVolume; } }
        public int PeriodVolume { get { return _periodVolume; } }

        #region private
        private DateTime _dateTimeStamp;
        private double _high;
        private double _low;
        private double _open;
        private double _close;
        private int _totalVolume;
        private int _periodVolume;
        private CultureInfo _enUS = new CultureInfo( "en-US");
        #endregion
    }
    public class LookupDayWeekMonthEventArgs : LookupEventArgs
    {
        public LookupDayWeekMonthEventArgs( String requestId, String line) :
            base(requestId, LookupType.REQ_HST_DWM, LookupSequence.MessageDetail) {
            fields = line.split(',');
            if( fields.Length < 8) {
                Log.Error( "LookupIntervalEventArgs.ctor(): " + line);
                return;
            }
            if( !DateTime.TryParseExact(fields[1], "yyyy-MM-dd HH:mm:ss", _enUS, DateTimeStyles.None, out _dateTimeStamp)) _dateTimeStamp = DateTime.MinValue;
            if( !double.TryParse(fields[2], out _high)) _high = 0;
            if( !double.TryParse(fields[3], out _low)) _low = 0;
            if( !double.TryParse(fields[4], out _open)) _open = 0;
            if( !double.TryParse(fields[5], out _close)) _close = 0;
            if( !int.TryParse(fields[6], out _periodVolume)) _periodVolume = 0;
            if( !int.TryParse(fields[7], out _openInterest)) _openInterest = 0;
        }
        public DateTime DateTimeStamp { get { return _dateTimeStamp; } }
        public double High { get { return _high; } }
        public double Low { get { return _low; } }
        public double Open { get { return _open; } }
        public double Close { get { return _close; } }
        public int PeriodVolume { get { return _periodVolume; } }
        public int OpenInterest { get { return _openInterest; } }

        #region private
        private DateTime _dateTimeStamp;
        private double _high;
        private double _low;
        private double _open;
        private double _close;
        private int _periodVolume;
        private int _openInterest;
        private CultureInfo _enUS = new CultureInfo( "en-US");
        #endregion
    }

    // Symbol search lookup events
    public class LookupSymbolEventArgs : LookupEventArgs
    {
        public LookupSymbolEventArgs( String requestId, String line) :
            base(requestId, LookupType.REQ_SYM_SYM, LookupSequence.MessageDetail) {
            fields = line.split(',');
            if( fields.Length < 5) throw new Exception( "Error in Symbol parameter provided");
            _symbol = fields[1];
            _marketId = fields[2];
            _securityId = fields[3];
            _description = "";
            for (i = 4; i < fields.Length; i++) _description += fields[i];
        }
        public String Symbol { get { return _symbol; } }
        public String MarketId { get { return _marketId; } }
        public String SecurityId { get { return _securityId; } }
        public String Description { get { return _description; } }

        #region private
        private String _symbol;
        private String _marketId;
        private String _securityId;
        private String _description;
        #endregion
    }
    public class LookupSicSymbolEventArgs : LookupEventArgs
    {
        public LookupSicSymbolEventArgs( String requestId, String line) :
            base(requestId, LookupType.REQ_SYM_SIC, LookupSequence.MessageDetail) {
            fields = line.split(',');
            if( fields.Length < 6) throw new Exception( "Error in SIC parameter provided");

            _sic = fields[1];
            _symbol = fields[2];
            _marketId = fields[3];
            _securityId = fields[4];
            _description = "";
            for (i = 5; i < fields.Length; i++) _description += fields[i];
        }

        public String Sic { get { return _sic; } } 
        public String Symbol { get { return _symbol; } }
        public String MarketId { get { return _marketId; } }
        public String SecurityId { get { return _securityId; } }
        public String Description { get { return _description; } }

        #region private
        private String _sic;
        private String _symbol;
        private String _marketId;
        private String _securityId;
        private String _description;
        #endregion
    }
    public class LookupNaicSymbolEventArgs : LookupEventArgs
    {
        public LookupNaicSymbolEventArgs( String requestId, String line) :
            base(requestId, LookupType.REQ_SYM_NAC, LookupSequence.MessageDetail) {
            fields = line.split(',');
            if( fields.Length < 6) throw new Exception( "Error in NAIC parameter provided");

            _naic = fields[1];
            _symbol = fields[2];
            _marketId = fields[3];
            _securityId = fields[4];
            _description = "";
            for (i = 5; i < fields.Length; i++) _description += fields[i];
        }
        public String Naic { get { return _naic; } } 
        public String Symbol { get { return _symbol; } }
        public String MarketId { get { return _marketId; } }
        public String SecurityId { get { return _securityId; } }
        public String Description { get { return _description; } }

        #region private
        private String _naic;
        private String _symbol;
        private String _marketId;
        private String _securityId;
        private String _description;
        #endregion
    }

    public class IQLookupHistorySymbolClient : SocketClient 
    {
        // Delegates for event
        public event EventHandler<LookupEventArgs> LookupEvent;

        // Constructor
        public IQLookupHistorySymbolClient(int bufferSize)
            : base(IQSocket.GetEndPoint(PortType.Lookup), bufferSize) {
            _histDataPointsPerSend = 500;
            _timeMarketOpen = new Time(09, 30, 00);
            _timeMarketClose = new Time(16, 00, 00);
            _lastRequestNumber = -1;
            _histMaxDataPoints = 5000;
         }

        // Command Requests
        public void Connect() {
            ConnectToSocketAndBeginReceive(IQSocket.GetSocket());
        }
        public void Disconnect(int flushSeconds = 2) {
            DisconnectFromSocket(flushSeconds);
        }
        public void SetClientName( String name) {
            Send( "S,SET CLIENT NAME," + name + "\r\n");
        }

        // Historical Data Requests
        public int RequestTickData( String symbol, int dataPoints, boolean oldToNew) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_TCK.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "HTX,%1$s,%2$s,%3$s,%4$s,%5$s\r\n", symbol, dataPoints.toString( "0000000"), oldToNew ? "1" : "0",
                reqNo, _histDataPointsPerSend.toString( "0000000"));
            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_TCK, LookupSequence.MessageStart));
            return _lastRequestNumber;
        }
        public int RequestTickData( String symbol, int days, boolean oldToNew, Time timeStartInDay = null, Time timeEndInDay = null ) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_TCK.toString() + _lastRequestNumber.toString( "0000000");
            if( timeStartInDay == null ) timeStartInDay = _timeMarketOpen;
            if( timeEndInDay == null ) timeEndInDay = _timeMarketClose;

            reqString = String.format( "HTD,%1$s,%2$s,%3$s,%4$s,%5$s,%6$s,%7$s,{7}\r\n", symbol, days.toString( "0000000"), _histMaxDataPoints.toString( "0000000"),
                timeStartInDay.IQFeedFormat, timeEndInDay.IQFeedFormat, oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));
            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_TCK, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestTickData( String symbol, DateTime start, DateTime? end, boolean oldToNew, Time timeStartInDay = null, Time timeEndInDay = null ) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_TCK.toString() + _lastRequestNumber.toString( "0000000");
            //if( timeStartInDay == null ) timeStartInDay = _timeMarketOpen;
            //if( timeEndInDay == null ) timeEndInDay = _timeMarketClose;

            reqString = String.format( "HTT,%1$s,%2$s,%3$s,%4$s,%5$s,%6$s,%7$s,{7},{8}\r\n", symbol, start.toString( "yyyyMMdd HHmmss"),
                end.HasValue ? end.Value.toString( "yyyyMMdd HHmmss") : "", _histMaxDataPoints.toString( "0000000"),
                timeStartInDay == null ? "" : timeStartInDay.IQFeedFormat, timeEndInDay == null ? "" : timeEndInDay.IQFeedFormat, oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));
            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_TCK, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestIntervalData( String symbol, Interval interval, int dataPoints, boolean oldToNew) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_INT.toString() + _lastRequestNumber.toString( "0000000");
 
            reqString = String.format( "HIX,%1$s,%2$s,%3$s,%4$s,%5$s,%6$s\r\n", symbol, interval.Seconds.toString( "0000000"),
                dataPoints.toString( "0000000"), oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));
            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_INT, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestIntervalData( String symbol, Interval interval, int days, boolean oldToNew, Time timeStartInDay = null, Time timeEndInDay = null ) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_INT.toString() + _lastRequestNumber.toString( "0000000");
            if( timeStartInDay == null ) timeStartInDay = _timeMarketOpen;
            if( timeEndInDay == null ) timeEndInDay = _timeMarketClose;

            reqString = String.format( "HID,%1$s,%2$s,%3$s,%4$s,%5$s,%6$s,%7$s,{7},{8}\r\n", symbol, interval.Seconds.toString( "0000000"),
                days.toString( "0000000"), _histMaxDataPoints.toString( "0000000"), timeStartInDay.IQFeedFormat, timeEndInDay.IQFeedFormat,
                oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));
                 
            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_INT, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestIntervalData( String symbol, Interval interval, DateTime start, DateTime? end, boolean oldToNew, Time timeStartInDay = null, Time timeEndInDay = null ) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_INT.toString() + _lastRequestNumber.toString( "0000000");
            //if( timeStartInDay == null ) timeStartInDay = _timeMarketOpen;
            //if( timeEndInDay == null ) timeEndInDay = _timeMarketClose;

            reqString = String.format( "HIT,%1$s,%2$s,%3$s,%4$s,%5$s,%6$s,%7$s,{7},{8},{9}\r\n", symbol, interval.Seconds.toString( "0000000"),
                start.toString( "yyyyMMdd HHmmss"), end.HasValue ? end.Value.toString( "yyyyMMdd HHmmss") : "",
                "", timeStartInDay == null ? "" : timeStartInDay.IQFeedFormat, timeEndInDay == null ? "" : timeEndInDay.IQFeedFormat,  oldToNew ? "1" : "0",
                 reqNo, _histDataPointsPerSend.toString( "0000000"));

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_INT, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestDailyData( String symbol, int dataPoints, boolean oldToNew) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_DWM.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "HDX,%1$s,%2$s,%3$s,%4$s,%5$s\r\n", symbol, dataPoints.toString( "0000000"),
                 oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_DWM, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestDailyData( String symbol, DateTime start, DateTime? end, boolean oldToNew) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_DWM.toString() + _lastRequestNumber.toString( "0000000");
 
            reqString = String.format( "HDT,%1$s,%2$s,%3$s,%4$s,%5$s,%6$s,%7$s\r\n", symbol, 
                start.toString( "yyyyMMdd"), end.HasValue ? end.Value.toString( "yyyyMMdd") : "",
                  _histMaxDataPoints.toString( "0000000"), oldToNew ? "1" : "0",
                 reqNo, _histDataPointsPerSend.toString( "0000000"));

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_DWM, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestWeeklyData( String symbol, int dataPoints, boolean oldToNew) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_DWM.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "HWX,%1$s,%2$s,%3$s,%4$s,%5$s\r\n", symbol, dataPoints.toString( "0000000"),
                 oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_DWM, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestMonthlyData( String symbol, int dataPoints, boolean oldToNew) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_HST_DWM.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "HMX,%1$s,%2$s,%3$s,%4$s,%5$s\r\n", symbol, dataPoints.toString( "0000000"),
                 oldToNew ? "1" : "0", reqNo, _histDataPointsPerSend.toString( "0000000"));

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_HST_DWM, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }

        // Search Symbols by filter
        public enum SearchField { Symbol, Description }
        public enum FilterType { Market, SecurityType }
        public int RequestSymbols(SearchField searchField, String searchText, FilterType filterType, string[] filterValue) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_SYM_SYM.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "SBF,%1$s,%2$s,%3$s,%4$s,%5$s\r\n", (searchField == SearchField.Symbol) ? "s" : "d",
                searchText, (filterType == FilterType.Market) ? "e" : "t",  String.Join( " ", filterValue), reqNo);

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_SYM_SYM, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestSymbolBySic( String searchText) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_SYM_SIC.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "SBS,%1$s,%2$s\r\n", searchText, reqNo);

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_SYM_SIC, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }
        public int RequestSymbolByNaic( String searchText) {
            _lastRequestNumber++;
            reqNo = LookupType.REQ_SYM_NAC.toString() + _lastRequestNumber.toString( "0000000");

            reqString = String.format( "SBN,%1$s,%2$s\r\n", searchText, reqNo);

            Send(reqString);
            OnLookupEvent(new LookupEventArgs(reqNo, LookupType.REQ_SYM_NAC, LookupSequence.MessageStart));

            return _lastRequestNumber;
        }


        // Events
        protected @Override void OnTextLineEvent(TextLineEventArgs e) {
            if( e.textLine.StartsWith(LookupType.REQ_HST_TCK.toString())) {
                reqId = e.textLine.Substring(0, e.textLine.IndexOf(','));
                if( e.textLine.StartsWith(reqId + ",!ENDMSG!")) {
                    OnLookupEvent(new LookupEventArgs(reqId, LookupType.REQ_HST_TCK, LookupSequence.MessageEnd));
                    return;
                }

                OnLookupEvent(new LookupTickEventArgs(reqId, e.textLine));
                return;
            }

            if( e.textLine.StartsWith(LookupType.REQ_HST_INT.toString())) {
                reqId = e.textLine.Substring(0, e.textLine.IndexOf(','));
                if( e.textLine.StartsWith(reqId + ",!ENDMSG!")) {
                    OnLookupEvent(new LookupEventArgs(reqId, LookupType.REQ_HST_INT, LookupSequence.MessageEnd));
                    return;
                }

                OnLookupEvent(new LookupIntervalEventArgs(reqId, e.textLine));
                return;
            }

            if( e.textLine.StartsWith(LookupType.REQ_HST_DWM.toString())) {
                reqId = e.textLine.Substring(0, e.textLine.IndexOf(','));
                if( e.textLine.StartsWith(reqId + ",!ENDMSG!")) {
                    OnLookupEvent(new LookupEventArgs(reqId, LookupType.REQ_HST_DWM, LookupSequence.MessageEnd));
                    return;
                }

                OnLookupEvent(new LookupDayWeekMonthEventArgs(reqId, e.textLine));
                return;
            }

            if( e.textLine.StartsWith(LookupType.REQ_SYM_SYM.toString())) {
                reqId = e.textLine.Substring(0, e.textLine.IndexOf(','));
                if( e.textLine.StartsWith(reqId + ",!ENDMSG!")) {
                    OnLookupEvent(new LookupEventArgs(reqId, LookupType.REQ_SYM_SYM, LookupSequence.MessageEnd));
                    return;
                }
                if( e.textLine.StartsWith(reqId + ",E")) { return; }

                OnLookupEvent(new LookupSymbolEventArgs(reqId, e.textLine));
                return;
            }

            if( e.textLine.StartsWith(LookupType.REQ_SYM_NAC.toString())) {
                reqId = e.textLine.Substring(0, e.textLine.IndexOf(','));
                if( e.textLine.StartsWith(reqId + ",!ENDMSG!")) {
                    OnLookupEvent(new LookupEventArgs(reqId, LookupType.REQ_SYM_NAC, LookupSequence.MessageEnd));
                    return;
                }
                if( e.textLine.StartsWith(reqId + ",E")) { return; }


                OnLookupEvent(new LookupNaicSymbolEventArgs(reqId, e.textLine));
                return;
            }

            if( e.textLine.StartsWith(LookupType.REQ_SYM_SIC.toString())) {
                reqId = e.textLine.Substring(0, e.textLine.IndexOf(','));
                if( e.textLine.StartsWith(reqId + ",!ENDMSG!")) {
                    OnLookupEvent(new LookupEventArgs(reqId, LookupType.REQ_SYM_SIC, LookupSequence.MessageEnd));
                    return;
                }
                if( e.textLine.StartsWith(reqId + ",E")) { return; }

                OnLookupEvent(new LookupSicSymbolEventArgs(reqId, e.textLine));
                return;
            }

            throw new Exception( "(Lookup) NOT HANDLED:" + e.textLine);
        }
        protected void OnLookupEvent(LookupEventArgs e) {
            if( LookupEvent != null ) LookupEvent(this, e);
        }

        #region private
        private int _histDataPointsPerSend;
        private int _histMaxDataPoints;
        private Time _timeMarketOpen;
        private Time _timeMarketClose;
        private int _lastRequestNumber;
        #endregion

    }
}
