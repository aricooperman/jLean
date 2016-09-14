package com.quantconnect.lean;

/// Specifies how data is normalized before being sent into an algorithm
public enum DataNormalizationMode {
    /// The raw price with dividends added to cash book
    Raw,
    /// The adjusted prices with splits and dividendends factored in
    Adjusted,
    /// The adjusted prices with only splits factored in, dividends paid out to the cash book
    SplitAdjusted,
    /// The split adjusted price plus dividends
    TotalReturn
}