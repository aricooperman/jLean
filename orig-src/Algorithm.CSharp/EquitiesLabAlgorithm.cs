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
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using QuantConnect.Data.Market;
using QuantConnect.Orders;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
     * This algorithm demonstrates how you can get your symbol list each day from a remote server
     * using the WebClient
    */
    public class EquitiesLabAlgorithm : QCAlgorithm
    {
        public static final String EquitiesLabKey = @"ENTER YOUR EQUITIES LAB KEY HERE";
        public static final String EquitiesLabScreener = @"ENTRY YOUR EQUITIES LAB SCREEN KEY HERE";
        public static final String EquitiesLabUrlFormat = @"https://www.equitieslab.com/play/prod/RestControl/get?key=%1$s&screener=%2$s&date=%3$s&metadata=false";

        private DateTime tradedToday;
        private EquitiesLabResponse _todaysResponse;

        public @Override void Initialize() {
            UniverseSettings.Resolution = Resolution.Hour;

            SetStartDate(2015, 01, 05);
            SetEndDate(2015, 11, 09);

            SetCash(1000*1000);


            AddUniverse( "equities-lab-universe", date =>
            {
                using (client = new WebClient()) {
                    //2014-12-30
                    file = client.DownloadString(String.format(EquitiesLabUrlFormat, EquitiesLabKey, EquitiesLabScreener, date.toString( "yyyy-MM-dd")));
                    response = JsonConvert.DeserializeObject<EquitiesLabResponse>(file);
                    _todaysResponse = new EquitiesLabResponse();
                    _todaysResponse.Securities = response.Securities.Where(x -> ValidSymbols.Contains(x.Ticker)).ToList();
                    return _todaysResponse.Securities.Select(x -> x.Ticker);
                }
            });

            // cancell all orders at EOD
            Schedule.Event( "Cancel Open Orders").EveryDay().At(Duration.ofHours(16)).Run(() =>
            {
                foreach (ticket in Transactions.GetOrderTickets(x -> x.Status.IsOpen())) {
                    ticket.Cancel();
                }
            });
        }

        public void OnData(TradeBars slice) {
            if( tradedToday.Date != Time.Date) {
                // leave a small buffer of cash
                targetPercentage = 1m/(_todaysResponse.Securities.Count + 1);

                foreach (target in _todaysResponse.Securities.Where(x -> ValidSymbols.Contains(x.Ticker))) {
                    // rebalance portfolio to equal weights
                    SetHoldings(target.Ticker, targetPercentage);
                }

                tradedToday = Time.Date;
            }
            else
            {
                foreach (target in _todaysResponse.Securities.Where(x -> ValidSymbols.Contains(x.Ticker))) {
                    // set stop loss / profit orders
                    security = Securities[target.Ticker];
                    if( !security.Invested) continue;

                    if( security.Close < target.StopLoss || security.Close > target.StopGain) {
                        MarketOrder(target.Ticker, -security.Holdings.Quantity, true);
                    }
                }
            }
        }

        public @Override void OnOrderEvent(OrderEvent orderEvent) {
            if( orderEvent.Status.IsFill()) {
                // if we receive a fill cancel the other outstanding order
                Transactions.CancelOpenOrders(orderEvent.Symbol);
            }
        }

        class EquitiesLabResponse
        {
            @JsonProperty( "securities")]
            public List<EquitiesLabTarget> Securities = new List<EquitiesLabTarget>();
        }

        class EquitiesLabTarget
        {
            @JsonProperty( "ticker")]
            public String Ticker;
            @JsonProperty( "stop_loss")]
            public BigDecimal StopLoss;
            @JsonProperty( "stop_gain")]
            public BigDecimal StopGain;
        }

        public static final HashSet<String> ValidSymbols = new HashSet<String>
        {
            {"NP"},
            {"PKX"},
            {"RYN"},
            {"HCLP"},
            {"OLN"},
            {"MLM"},
            {"SCOK"},
            {"NEU"},
            {"SSLT"},
            {"MON"},
            {"HSC"},
            {"TX"},
            {"CLW"},
            {"ROCK"},
            {"FNV"},
            {"WOR"},
            {"HMY"},
            {"TAHO"},
            {"AAON"},
            {"STLD"},
            {"MAS"},
            {"TS"},
            {"GLT"},
            {"PGEM"},
            {"CHMT"},
            {"NRP"},
            {"BBL"},
            {"EMN"},
            {"SYT"},
            {"SNMX"},
            {"ARLP"},
            {"USCR"},
            {"DD"},
            {"DOW"},
            {"NOR"},
            {"DEL"},
            {"KOP"},
            {"AA"},
            {"OCIP"},
            {"FMSA"},
            {"USLM"},
            {"SCL"},
            {"GPRE"},
            {"SHW"},
            {"PPP"},
            {"JHX"},
            {"UFPI"},
            {"OMG"},
            {"GOLD"},
            {"X"},
            {"SLCA"},
            {"RGLD"},
            {"WY"},
            {"TREC"},
            {"TMST"},
            {"RPM"},
            {"CSTM"},
            {"CYT"},
            {"TNH"},
            {"AUY"},
            {"GSM"},
            {"WLKP"},
            {"AVD"},
            {"SCHN"},
            {"KALU"},
            {"USG"},
            {"GGB"},
            {"FMC"},
            {"HBM"},
            {"VHI"},
            {"ZEUS"},
            {"FBR"},
            {"MOS"},
            {"RIO"},
            {"GRA"},
            {"BIOA"},
            {"PAH"},
            {"IPHS"},
            {"GG"},
            {"AWI"},
            {"HUN"},
            {"BTU"},
            {"IIIN"},
            {"IPI"},
            {"PENX"},
            {"POT"},
            {"MTRN"},
            {"CENX"},
            {"APD"},
            {"MT"},
            {"ASPN"},
            {"RS"},
            {"GFF"},
            {"EXP"},
            {"BVN"},
            {"CF"},
            {"DDC"},
            {"SWC"},
            {"CCJ"},
            {"CDE"},
            {"RIOM"},
            {"LYB"},
            {"BCC"},
            {"RTK"},
            {"AG"},
            {"RNO"},
            {"FSM"},
            {"CHNR"},
            {"SHLM"},
            {"LXU"},
            {"UFS"},
            {"SCCO"},
            {"SZYM"},
            {"ZINC"},
            {"UAN"},
            {"FELP"},
            {"IBP"},
            {"TREX"},
            {"ABX"},
            {"VMC"},
            {"WLB"},
            {"RYAM"},
            {"AMRS"},
            {"OMN"},
            {"IOSP"},
            {"SXCP"},
            {"ECL"},
            {"BAK"},
            {"SLW"},
            {"TC"},
            {"AKS"},
            {"OC"},
            {"SQM"},
            {"CE"},
            {"PVG"},
            {"GFI"},
            {"SSRI"},
            {"TROX"},
            {"PX"},
            {"ASH"},
            {"YZC"},
            {"SA"},
            {"HL"},
            {"AXTA"},
            {"BECN"},
            {"MUX"},
            {"WDFC"},
            {"CBT"},
            {"SBGL"},
            {"LPX"},
            {"NTIC"},
            {"CGA"},
            {"OEC"},
            {"IFF"},
            {"VALE"},
            {"CLF"},
            {"CRH"},
            {"CMC"},
            {"DRD"},
            {"HW"},
            {"NCS"},
            {"PGTI"},
            {"NUE"},
            {"CMP"},
            {"MIL"},
            {"MDM"},
            {"CLD"},
            {"SID"},
            {"PPG"},
            {"DOOR"},
            {"AHGP"},
            {"BHP"},
            {"AGI"},
            {"PAAS"},
            {"NX"},
            {"NTK"},
            {"MTX"},
            {"KS"},
            {"ACH"},
            {"FCX"},
            {"SWM"},
            {"PCL"},
            {"SMG"},
            {"ALB"},
            {"SXC"},
            {"ODC"},
            {"MERC"},
            {"CSTE"},
            {"KGC"},
            {"NEM"},
            {"AXLL"},
            {"KRA"},
            {"AGU"},
            {"WPP"},
            {"CBPX"},
            {"AEM"},
            {"HNRG"},
            {"BXC"},
            {"RNF"},
            {"SIAL"},
            {"APOG"},
            {"USAP"},
            {"FUL"},
            {"MBII"},
            {"FF"},
            {"STCK"},
            {"FOE"},
            {"SXT"},
            {"BCPC"},
            {"VAL"},
            {"OCIR"},
            {"SYNL"},
            {"POL"},
            {"TGEN"},
            {"KWR"},
            {"ARG"},
            {"MEOH"},
            {"ZEP"},
            {"NWPX"},
            {"EXK"},
            {"PATK"},
            {"WLK"},
            {"ACI"},
            {"TRQ"},
            {"KMG"},
            {"HWKN"},
            {"RFP"},
            {"KRO"},
            {"MDU"},
            {"HNH"},
            {"BLDR"}
        };
    }
}
