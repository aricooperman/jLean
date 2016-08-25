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
 *
*/

using System;
using System.Collections.Concurrent;

package com.quantconnect.lean.ToolBox.IQFeed
{

    public class LookupTableMarketEventArgs : LookupEventArgs
    {
        public LookupTableMarketEventArgs( String line) :
            base(null, LookupType.REQ_TAB_MKT, LookupSequence.MessageDetail) {
            fields = line.split(',');
            _code = fields[0];
            _shortName = fields[1];
            _longName = fields[2];
         }
        public String Code { get { return _code; } }
        public String ShortName { get { return _shortName; } }
        public String LongName { get { return _longName; } }

        #region private
        private String _code;
        private String _shortName;
        private String _longName;
        #endregion
    }
    public class LookupTableMarketCenterEventArgs : LookupEventArgs
    {
        public LookupTableMarketCenterEventArgs( String line) :
            base(null, LookupType.REQ_TAB_MKC, LookupSequence.MessageDetail) {
            fields = line.split(',');
           _code = fields[0];
            _marketEquityId = fields[1].split(' ');
            _marketOptionId = fields[2].split(' ');
        }
        public String Code { get { return _code; } }
        public string[] MarketEquityId { get { return _marketEquityId; } }
        public string[] MarketOptionId { get { return _marketOptionId; } }

        #region private

        private String _code;
        private string[] _marketEquityId;
        private string[] _marketOptionId;
        #endregion
    }
    public class LookupTableNaicEventArgs : LookupEventArgs
    {
        public LookupTableNaicEventArgs( String line) :
            base(null, LookupType.REQ_TAB_NAC, LookupSequence.MessageDetail) {
            fields = line.split(',');
            _code = fields[0];
            _description = fields[1];
        }
        public String Code { get { return _code; } }
        public String Description { get { return _description; } }
 
        #region private
        private String _code;
        private String _description;
        #endregion
    }
    public class LookupTableSecurityTypeEventArgs : LookupEventArgs
    {
        public LookupTableSecurityTypeEventArgs( String line) :
            base(null, LookupType.REQ_TAB_SEC, LookupSequence.MessageDetail) {
            fields = line.split(',');
            _code = fields[0];
            _shortName = fields[1];
            _longName = fields[2];
        }
        public String Code { get { return _code; } }
        public String ShortName { get { return _shortName; } }
        public String LongName { get { return _longName; } }

        #region private
        private String _code;
        private String _shortName;
        private String _longName;
        #endregion
    }
    public class LookupTableSicEventArgs : LookupEventArgs
    {
        public LookupTableSicEventArgs( String line) :
            base(null, LookupType.REQ_TAB_SIC, LookupSequence.MessageDetail) {
            fields = line.split(',');
            _code = fields[0];
            _description = fields[1];
        }
        public String Code { get { return _code; } }
        public String Description { get { return _description; } }

        #region private
        private String _code;
        private String _description;
        #endregion
    }

    public class IQLookupTableClient : SocketClient
    {
        // Delegates for event
        public event EventHandler<LookupEventArgs> LookupEvent;


        public IQLookupTableClient(int bufferSize) : base(IQSocket.GetEndPoint(PortType.Lookup), bufferSize) {
            _que = new ConcurrentQueue<LookupType>();
        }
        public void Connect() {
            ConnectToSocketAndBeginReceive(IQSocket.GetSocket());
        }
        public void Disconnect(int flushSeconds = 2) {
            DisconnectFromSocket(flushSeconds);
        }
        public void SetClientName( String name) {
            Send( "S,SET CLIENT NAME," + name + "\r\n");
        }

        public void RequestMarkets() {
            Send( "SLM\r\n");
            _enQue(LookupType.REQ_TAB_MKT);
        }
        public void RequestMarketCenters() {
            Send( "SMC\r\n");
            _enQue(LookupType.REQ_TAB_MKC);
        }
        public void RequestNaic() {
            Send( "SNC\r\n");
            _enQue(LookupType.REQ_TAB_NAC);
         }
        public void RequestSecurityTypes() {
            Send( "SST\r\n");
            _enQue(LookupType.REQ_TAB_SEC);
        }
        public void RequestSic() {
            Send( "SSC\r\n");
            _enQue(LookupType.REQ_TAB_SIC);
        }

        protected @Override void OnTextLineEvent(TextLineEventArgs e) {
            if( e.textLine.StartsWith( "!ENDMSG!")) {
                lut = _peekQue();
                OnLookupEvent(new LookupEventArgs(null, lut, LookupSequence.MessageEnd));
                _deQue();
                return;
            }

            lute = _peekQue();
 
            switch (lute) {
                case LookupType.REQ_TAB_MKC:
                    OnLookupEvent(new LookupTableMarketCenterEventArgs(e.textLine));
                    return;
                case LookupType.REQ_TAB_MKT:
                    OnLookupEvent(new LookupTableMarketEventArgs(e.textLine));
                    return;
                case LookupType.REQ_TAB_NAC:
                    OnLookupEvent(new LookupTableNaicEventArgs(e.textLine));
                    return;
                case LookupType.REQ_TAB_SEC:
                    OnLookupEvent(new LookupTableSecurityTypeEventArgs(e.textLine));
                    return;
                case LookupType.REQ_TAB_SIC:
                    OnLookupEvent(new LookupTableSicEventArgs(e.textLine));
                    return;
            }

            throw new Exception( "(Lookup Table) NOT HANDLED:" + e.textLine);
        }
        protected void OnLookupEvent(LookupEventArgs e) {
            if( LookupEvent != null ) LookupEvent(this, e);
        }

 
        #region private

        void _deQue() {
            LookupType lut;
            if( !_que.TryDequeue(out lut)) {
                throw new Exception( "Out of sequence - Table receive");
            }
            if( _que.Count > 0) {
                OnLookupEvent(new LookupEventArgs(null, _peekQue(), LookupSequence.MessageStart));
            }
            return;
        }
        LookupType _peekQue() {
            LookupType lut;
            if( !_que.TryPeek(out lut)) {
                throw new Exception( "Out of sequence - Table receive");
            }
            return lut;
        }
        void _enQue(LookupType lut) {
            _que.Enqueue(lut);
            if( _que.Count == 1) {
                OnLookupEvent(new LookupEventArgs(null, lut, LookupSequence.MessageStart));
            }
        }


        private ConcurrentQueue<LookupType> _que;
        #endregion
    }
}
