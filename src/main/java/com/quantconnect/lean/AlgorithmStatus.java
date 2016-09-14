package com.quantconnect.lean;

/// States of a live deployment.
public enum AlgorithmStatus {
    /// Error compiling algorithm at start
    DeployError,    //1
    /// Waiting for a server
    InQueue,        //2
    /// Running algorithm
    Running,        //3
    /// Stopped algorithm or exited with runtime errors
    Stopped,        //4
    /// Liquidated algorithm
    Liquidated,     //5
    /// Algorithm has been deleted
    Deleted,        //6
    /// Algorithm completed running
    Completed,      //7
    /// Runtime Error Stoped Algorithm
    RuntimeError,    //8
    /// Error in the algorithm id (not used).
    Invalid,
    /// The algorithm is logging into the brokerage
    LoggingIn,
    /// The algorithm is initializing
    Initializing,
    /// History status update
    History
}