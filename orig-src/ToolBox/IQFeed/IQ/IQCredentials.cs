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

namespace QuantConnect.ToolBox.IQFeed
{
    public class IQCredentials
    {
        public IQCredentials( String loginId = "", String password = "", boolean autoConnect = false, boolean saveCredentials = true)
        {
            _loginId = loginId;
            _password = password;
            _autoConnect = autoConnect;
            _saveCredentials = saveCredentials;
        }
        public String LoginId { get { return _loginId; } set { _loginId = value; } }
        public String Password { get { return _password; } set { _password = value; } }
        public boolean AutoConnect { get { return _autoConnect; } set { _autoConnect = value; } }
        public boolean SaveCredentials { get { return _saveCredentials; } set { _saveCredentials = value; } }

        #region private
        private String _loginId;
        private String _password;
        private boolean _autoConnect;
        private boolean _saveCredentials;
        #endregion
    }

}
