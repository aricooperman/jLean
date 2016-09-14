package com.quantconnect.lean;

/// Defines the different channel status values
public class ChannelStatus {
    /// The channel is empty
    public static final String Vacated = "channel_vacated";
    /// The channel has subscribers
    public static final String Occupied = "channel_occupied";
}