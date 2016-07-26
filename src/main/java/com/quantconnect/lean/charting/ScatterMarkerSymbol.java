package com.quantconnect.lean.charting;

/**
 *  Shape or symbol for the marker in a scatter plot
 */
//    [JsonConverter(typeof(StringEnumConverter))]
public enum ScatterMarkerSymbol {
    /// Circle symbol
    None( "none" ),
    /// Circle symbol
    Circle( "circle" ),
    /// Square symbol
    Square( "square" ),
    /// Diamond symbol
    Diamond( "diamond" ),
    /// Triangle symbol
    Triangle( "triangle" ),
    /// Triangle-down symbol
    TriangleDown( "triangle-down" );
    
    private final String desc;

    ScatterMarkerSymbol( String desc ) {
        this.desc = desc;
    }
    
    public String toString() {
        return desc;
    }
}