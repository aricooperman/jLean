package com.quantconnect.lean;

/// Market data style: is the market data a summary (OHLC style) bar, or is it a time-price value.
public enum MarketDataType {
    /// Base market data type
    Base,
    /// TradeBar market data type (OHLC summary bar)
    TradeBar,
    /// Tick market data type (price-time pair)
    Tick,
    /// Data associated with an instrument
    Auxiliary,
    /// QuoteBar market data type [Bid(OHLC), Ask(OHLC) and Mid(OHLC) summary bar]
    QuoteBar,
    /// Option chain data
    OptionChain
}