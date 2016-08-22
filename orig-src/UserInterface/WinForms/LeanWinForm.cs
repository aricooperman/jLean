﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Messaging;
using QuantConnect.Packets;
//using Gecko;
//using Gecko.JQuery;

package com.quantconnect.lean.Views.WinForms
{
    public partial class LeanWinForm : Form
    {
        private readonly WebBrowser _monoBrowser;
        private readonly AlgorithmNodePacket _job;
        private readonly QueueLogHandler _logging;
        private readonly EventMessagingHandler _messaging;
        private readonly boolean _liveMode = false;
        //private GeckoWebBrowser _geckoBrowser;

        /// <summary>
        /// Create the UX.
        /// </summary>
        /// <param name="notificationHandler">Messaging system</param>
        /// <param name="job">Job to use for URL generation</param>
        public LeanWinForm(IMessagingHandler notificationHandler, AlgorithmNodePacket job) {
            InitializeComponent();

            //Form Setup:
            CenterToScreen();
            WindowState = FormWindowState.Maximized;
            Text = "QuantConnect Lean Algorithmic Trading Engine: v" + Globals.Version;

            //Save off the messaging event handler we need:
            _job = job;
            _liveMode = job is LiveNodePacket;
            _messaging = (EventMessagingHandler)notificationHandler;
            url = GetUrl(job, _liveMode);

            //GECKO WEB BROWSER: Create the browser control
            // https://www.nuget.org/packages/GeckoFX/
            // -> If you don't have IE.
            //_geckoBrowser = new GeckoWebBrowser { Dock = DockStyle.Fill, Name = "browser" };
            //_geckoBrowser.DOMContentLoaded += BrowserOnDomContentLoaded;
            //_geckoBrowser.Navigate(url);
            //splitPanel.Panel1.Controls.Add(_geckoBrowser);

            // MONO WEB BROWSER: Create the browser control
            // Default shipped with VS and Mono. Works OK in Windows, and compiles in linux.
            _monoBrowser = new WebBrowser() {Dock = DockStyle.Fill, Name = "Browser"};
            _monoBrowser.DocumentCompleted += MonoBrowserOnDocumentCompleted;
            _monoBrowser.Navigate(url);
            splitPanel.Panel1.Controls.Add(_monoBrowser);

            //Setup Event Handlers:
            _messaging.DebugEvent += MessagingOnDebugEvent;
            _messaging.LogEvent += MessagingOnLogEvent;
            _messaging.RuntimeErrorEvent += MessagingOnRuntimeErrorEvent;
            _messaging.HandledErrorEvent += MessagingOnHandledErrorEvent;
            _messaging.BacktestResultEvent += MessagingOnBacktestResultEvent;

            _logging = Log.LogHandler as QueueLogHandler;

            //Show warnings if the API token and UID aren't set.
            if( _job.UserId == 0) {
                MessageBox.Show( "Your user id is not set. Please check your config.json file 'job-user-id' property.", "LEAN Algorithmic Trading", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if( _job.Channel == "") {
                MessageBox.Show( "Your API token is not set. Please check your config.json file 'api-access-token' property.", "LEAN Algorithmic Trading", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Get the URL for the embedded charting
        /// </summary>
        /// <param name="job">Job packet for the URL</param>
        /// <param name="liveMode">Is this a live mode chart?</param>
        /// <param name="holdReady">Hold the ready signal to inject data</param>
        private static String GetUrl(AlgorithmNodePacket job, boolean liveMode = false, boolean holdReady = false) {
            url = "";
            hold = holdReady == false ? "0" : "1";
            embedPage = liveMode ? "embeddedLive" : "embedded";

            url = String.format(
                "https://www.quantconnect.com/terminal/%1$s?user=%2$s&token=%3$s&pid={3}&version={4}&holdReady={5}&bid={6}",
                embedPage, job.UserId, job.Channel, job.ProjectId, Globals.Version, hold, job.AlgorithmId);

            return url;
        }

        /// <summary>
        /// MONO BROWSER: Browser content has completely loaded.
        /// </summary>
        private void MonoBrowserOnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs webBrowserDocumentCompletedEventArgs) {
            _messaging.OnConsumerReadyEvent();
        }

        /// <summary>
        /// GECKO BROWSER: Browser content has completely loaded.
        /// </summary>
        //private void BrowserOnDomContentLoaded(object sender, DomEventArgs domEventArgs)
        //{
        //    _messaging.OnConsumerReadyEvent();
        //}

        /// <summary>
        /// Onload Form Initialization
        /// </summary>
        private void LeanWinForm_Load(object sender, EventArgs e) {
            if( OS.IsWindows && !WBEmulator.IsBrowserEmulationSet()) {
                WBEmulator.SetBrowserEmulationVersion();
            }
        }

        /// <summary>
        /// Update the status label at the bottom of the form
        /// </summary>
        private void timer_Tick(object sender, EventArgs e) {
            StatisticsToolStripStatusLabel.Text = string.Concat( "Performance: CPU: ", OS.CpuUsage.NextValue().toString( "0.0"), "%",
                                                                " Ram: ", OS.TotalPhysicalMemoryUsed, " Mb");

            if( _logging == null ) return;

            LogEntry log;
            while (_logging.Logs.TryDequeue(out log)) {
                switch (log.MessageType) {
                    case LogType.Debug:
                        LogTextBox.AppendText(log.toString(), Color.Black);
                        break;
                    default:
                    case LogType.Trace:
                        LogTextBox.AppendText(log.toString(), Color.Black);
                        break;
                    case LogType.Error:
                        LogTextBox.AppendText(log.toString(), Color.DarkRed);
                        break;
                }
            }
        }
         
        /// <summary>
        /// Backtest result packet
        /// </summary>
        /// <param name="packet"></param>
        private void MessagingOnBacktestResultEvent(BacktestResultPacket packet) {
            if( packet.Progress != 1) return;

            //Remove previous event handler:
            url = GetUrl(_job, _liveMode, true);

            //Generate JSON:
            jObj = new JObject();
            dateFormat = "yyyy-MM-dd HH:mm:ss";
            dynamic final = jObj;
            final.dtPeriodStart = packet.PeriodStart.toString(dateFormat);
            final.dtPeriodFinished = packet.PeriodFinish.AddDays(1).toString(dateFormat);
            dynamic resultData = new JObject();
            resultData.version = "3";
            resultData.results = JObject.FromObject(packet.Results);
            resultData.statistics = JObject.FromObject(packet.Results.Statistics);
            resultData.iTradeableDates = 1;
            resultData.ranking = null;
            final.oResultData = resultData;
            json = JsonConvert.SerializeObject(final);

            //GECKO RESULT SET:
            //_geckoBrowser.DOMContentLoaded += (sender, args) =>
            //{
            //    executor = new JQueryExecutor(_geckoBrowser.Window);
            //    executor.ExecuteJQuery( "window.jnBacktest = JSON.parse('" + json + "');");
            //    executor.ExecuteJQuery( "$.holdReady(false)");
            //};
            //_geckoBrowser.Navigate(url);

            //MONO WEB BROWSER RESULT SET:
            _monoBrowser.DocumentCompleted += (sender, args) =>
            {
                if( _monoBrowser.Document == null ) return;
                _monoBrowser.Document.InvokeScript( "eval", new object[] { "window.jnBacktest = JSON.parse('" + json + "');" });
                _monoBrowser.Document.InvokeScript( "eval", new object[] { "$.holdReady(false)" });
            };
            _monoBrowser.Navigate(url);

            foreach (pair in packet.Results.Statistics) {
                _logging.Trace( "STATISTICS:: " + pair.Key + " " + pair.Value);
            }
        }

        /// <summary>
        /// Display a handled error
        /// </summary>
        private void MessagingOnHandledErrorEvent(HandledErrorPacket packet) {
            hstack = (!string.IsNullOrEmpty(packet.StackTrace) ? (Environment.NewLine + " " + packet.StackTrace) : string.Empty);
            _logging.Error(packet.Message + hstack);
        }

        /// <summary>
        /// Display a runtime error
        /// </summary>
        private void MessagingOnRuntimeErrorEvent(RuntimeErrorPacket packet) {
            rstack = (!string.IsNullOrEmpty(packet.StackTrace) ? (Environment.NewLine + " " + packet.StackTrace) : string.Empty);
            _logging.Error(packet.Message + rstack);
        }

        /// <summary>
        /// Display a log packet
        /// </summary>
        private void MessagingOnLogEvent(LogPacket packet) {
            _logging.Trace(packet.Message);
        }

        /// <summary>
        /// Display a debug packet
        /// </summary>
        /// <param name="packet"></param>
        private void MessagingOnDebugEvent(DebugPacket packet) {
            _logging.Trace(packet.Message);
        }

        /// <summary>
        /// Closing the form exit the LEAN engine too.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeanWinForm_FormClosed(object sender, FormClosedEventArgs e) {
            Log.Trace( "LeanWinForm(): Form closed.");
            Environment.Exit(0);
        }
    }
}
