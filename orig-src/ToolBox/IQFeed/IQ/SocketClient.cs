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
using System.Text;
using System.Net;
using System.Net.Sockets;

package com.quantconnect.lean.ToolBox.IQFeed
{

    public class TextLineEventArgs : EventArgs
    {
        public TextLineEventArgs( String textLine) {
            _textLine = textLine;
        }
        public String textLine { get { return _textLine; } } 
        #region private
        private String _textLine;
        #endregion
    }

    public class SocketClient
    {
        public event EventHandler<TextLineEventArgs> TextLineEvent;
        public SocketClient(IPEndPoint endPoint, int bufferSize) {
            _socket = null;
            _endPoint = endPoint;
            _callback = new AsyncCallback(_OnReceive);
            _buffer = new byte[bufferSize];
        }
        protected void DisconnectFromSocket(int flushSeconds = 1) {
            try
            {
                _socket.Close(flushSeconds);
            }
            catch (Exception ex) {
                throw new Exception( "Unable to close socket", ex);
            }

        }

        public void Send( String command) {
            szCommand = new byte[command.Length];
            szCommand = Encoding.ASCII.GetBytes(command);
            iBytesToSend = szCommand.Length;
            try
            {
                _socket.Send(szCommand, iBytesToSend, SocketFlags.None);
            }
            catch (Exception ex) {
                throw new Exception( "Error in send in socket", ex);
            }
        }
        protected void ConnectToSocketAndBeginReceive(Socket socket) {
            try
            {
                _socket = socket;
                _socket.Connect(_endPoint);
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, _callback, null );
            }
            catch (Exception ex) {
                throw new Exception( "Error in connecting to socket and starting receive: " + ex.Message + " " + _endPoint.Address +":"+ _endPoint.Port + " >>>> " + ex.StackTrace, ex);
            }

        }


        protected void OnTextLineEvent(TextLineEventArgs e) {
            if( TextLineEvent != null ) TextLineEvent(this, e);
        }

        #region private
 
        private void _OnReceive(IAsyncResult asyn) {
            iReceivedBytes = 0;
             try
            {
                iReceivedBytes = _socket.EndReceive(asyn);
            }
            catch (Exception ex) {
                ode = ex as ObjectDisposedException;
                if( ode == null ) {
                    throw new Exception( "Error in ending receive", ex);
                }
                return;
            }

            sData = Encoding.ASCII.GetString(_buffer, 0, iReceivedBytes);

            sData = _incompleteRecord + sData;
            _incompleteRecord = "";

            while (sData.Length > 0) {
                iNewLinePos = -1;
                iNewLinePos = sData.IndexOf( "\n");
                String sLine;
                if( iNewLinePos == -1) {
                    _incompleteRecord = sData;
                    sData = "";
                }
                else
                {
                    sLine = sData.Substring(0, iNewLinePos);
                    OnTextLineEvent(new TextLineEventArgs(sLine));
                    sData = sData.Substring(sLine.Length + 1);
                }
            }
            if( !_socket.Connected) { return; }
            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, _callback, null );
            }
            catch (Exception ex) {
                throw new Exception( "Error in beginning asynchronous receive", ex);
            }

        }
         
        private AsyncCallback _callback;
        private Socket _socket;
        private IPEndPoint _endPoint;
        private byte[] _buffer;
        private String _incompleteRecord;
        #endregion
    }
}
