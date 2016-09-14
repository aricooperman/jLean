package com.quantconnect.lean;

/// Specifies the type of <see cref="QuantConnect.Data.Market.Delisting"/> data
public enum DelistingType {
    /// Specifies a warning of an imminent delisting
    Warning/* = 0*/,

    /// Specifies the symbol has been delisted
    Delisted/* = 1*/
}