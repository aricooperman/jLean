package com.quantconnect.lean;

/// Specifies where a subscription's data comes from
public enum SubscriptionTransportMedium {
    /// The subscription's data comes from disk
    LocalFile,

    /// The subscription's data is downloaded from a remote source
    RemoteFile,

    /// The subscription's data comes from a rest call that is polled and returns a single line/data point of information
    Rest
}