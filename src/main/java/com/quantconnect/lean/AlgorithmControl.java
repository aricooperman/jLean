package com.quantconnect.lean;

/// Wrapper for algorithm status enum to include the charting subscription.
public class AlgorithmControl {
    /// Default initializer for algorithm control class.
    public AlgorithmControl() {
        // default to true, API can @Override
        hasSubscribers = true;
        status = AlgorithmStatus.Running;
        chartSubscription = "Strategy Equity";
    }

    /// Current run status of the algorithm id.
    public AlgorithmStatus status;

    /// Currently requested chart.
    public String chartSubscription;

    /// True if there's subscribers on the channel
    public boolean hasSubscribers;
}