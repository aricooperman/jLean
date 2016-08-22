using System;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Brokerages.InteractiveBrokers;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Brokerages.InteractiveBrokers
{
    [TestFixture, Ignore( "These tests require the IBController and IB TraderWorkstation to be installed.")]
    public class InteractiveBrokersForexOrderTests : BrokerageTests
    {
        // set to true to disable launch of gateway from tests
        private static final boolean _manualGatewayControl = false;
        private static boolean _gatewayLaunched;
     
        [TestFixtureSetUp]
        public void InitializeBrokerage() {
        }

        [TestFixtureTearDown]
        public void DisposeBrokerage() {
            InteractiveBrokersGatewayRunner.Stop();
        }

        protected @Override Symbol Symbol
        {
            get { return Symbols.USDJPY; }
        }

        protected @Override SecurityType SecurityType
        {
            get { return SecurityType.Forex; }
        }

        protected @Override BigDecimal HighPrice
        {
            get { return 10000m; }
        }

        protected @Override BigDecimal LowPrice
        {
            get { return 0.01m; }
        }

        protected @Override BigDecimal GetAskPrice(Symbol symbol) {
            throw new NotImplementedException();
        }

        protected @Override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider) {
            if( !_manualGatewayControl && !_gatewayLaunched) {
                _gatewayLaunched = true;
                InteractiveBrokersGatewayRunner.Start(Config.Get( "ib-controller-dir"),
                    Config.Get( "ib-tws-dir"),
                    Config.Get( "ib-user-name"),
                    Config.Get( "ib-password"),
                    Config.GetBool( "ib-use-tws")
                    );
            }
            return new InteractiveBrokersBrokerage(orderProvider, securityProvider);
        }

        protected @Override void DisposeBrokerage(IBrokerage brokerage) {
            if( !_manualGatewayControl && brokerage != null ) {
                brokerage.Disconnect();
            }
        }
    }
}
