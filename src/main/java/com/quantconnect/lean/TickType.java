package com.quantconnect.lean;

/// Types of tick data - trades or quote ticks.
/// <remarks>QuantConnect currently only has trade tick data but can handle quote tick data with the same data structures.</remarks>
public enum TickType {
    /// Trade type tick object.
    Trade,
    /// Quote type tick object.
    Quote
}