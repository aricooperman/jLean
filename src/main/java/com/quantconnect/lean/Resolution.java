package com.quantconnect.lean;

/// Resolution of data requested.
/// <remarks>Always sort the enum from the smallest to largest resolution</remarks>
public enum Resolution {
    /// Tick Resolution (1)
    Tick,
    /// Second Resolution (2)
    Second,
    /// Minute Resolution (3)
    Minute,
    /// Hour Resolution (4)
    Hour,
    /// Daily Resolution (5)
    Daily
}