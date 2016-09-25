package com.quantconnect.lean.statistics;

/**
 * The method used to group order fills into trades
 */
public enum FillGroupingMethod {
    
    /**
     * A Trade is defined by a fill that establishes or increases a position and an offsetting fill that reduces the position size.
     */
    FillToFill,

    /**
     * A Trade is defined by a sequence of fills, from a flat position to a non-zero position which may increase or decrease in quantity, and back to a flat position.
     */
    FlatToFlat,

    /**
     * A Trade is defined by a sequence of fills, from a flat position to a non-zero position and an offsetting fill that reduces the position size.
     */
    FlatToReduced,
}