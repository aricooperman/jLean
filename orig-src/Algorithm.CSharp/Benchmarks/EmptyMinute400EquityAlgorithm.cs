using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;

package com.quantconnect.lean.Algorithm.CSharp.Benchmarks
{
    public class EmptyMinute400EquityAlgorithm : QCAlgorithm
    {
        public @Override void Initialize() {
            SetStartDate(2015, 09, 28);
            SetEndDate(2015, 11, 13);
            foreach (symbol in Symbols.Equity.All.Take(400)) {
                AddSecurity(SecurityType.Equity, symbol);
            }
        }

        public @Override void OnData(Slice slice) {
        }
    }
}