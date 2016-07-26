package com.quantconnect.lean.charting;

import java.awt.Color;
import java.math.BigDecimal;
import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.List;

import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.fasterxml.jackson.databind.annotation.JsonSerialize;
import com.quantconnect.lean.util.ColorJsonConverters.ColorJsonDeserializer;
import com.quantconnect.lean.util.ColorJsonConverters.ColorJsonSerializer;

/**
 *  Chart Series Object - Series data and properties for a chart:
 */
//    [JsonObject]
public class Series {
    
//    private final Logger log = LoggerFactory.getLogger( getClass() );

    /// Name of the Series:
    public String name = "";

    /// Axis for the chart series.
    public String unit = "$";

    /// Index/position of the series on the chart.
    public int index = 0;

    /// <summary>
    ///  Values for the series plot:
    /// These values are assumed to be in ascending time order (first points earliest, last points latest)
    /// </summary>
    public List<ChartPoint> values = new ArrayList<ChartPoint>();

    /// <summary>
    /// Chart type for the series:
    /// </summary>
    public SeriesType seriesType = SeriesType.Line;

    /// Color the series 
    @JsonSerialize( using = ColorJsonSerializer.class, as = String.class ) //[JsonConverter(typeof(ColorJsonConverter))]
    @JsonDeserialize( using = ColorJsonDeserializer.class, as = Color.class )
    public Color color = null; //Color.Empty;

    /// <summary>
    /// Shape or symbol for the marker in a scatter plot
    /// </summary>
    public ScatterMarkerSymbol scatterMarkerSymbol = ScatterMarkerSymbol.None;

    /// Get the index of the last fetch update request to only retrieve the "delta" of the previous request.
    private int updatePosition;

    /// Default constructor for chart series
    public Series() { }

    /// Constructor method for Chart Series
    /// <param name="name">Name of the chart series</param>
    public Series( String name ) {
        this.name = name;
//        this.seriesType = SeriesType.Line;
//        this.unit = "$";
//        this.index = 0;
//        this.color = null;
//        this.scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /// Foundational constructor on the series class
    /// <param name="name">Name of the series</param>
    /// <param name="type">Type of the series</param>
    public Series( String name, SeriesType type ) {
        this( name );
        seriesType = type;
//        index = 0;
//        unit = "$";
//        color = null;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /// Foundational constructor on the series class
    /// <param name="name">Name of the series</param>
    /// <param name="type">Type of the series</param>
    /// <param name="index">Index position on the chart of the series</param>
    public Series( String name, SeriesType type, int index ) {
        this( name, type );
        this.index = index;
//        unit = "$";
//        color = null;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /// Foundational constructor on the series class
    /// <param name="name">Name of the series</param>
    /// <param name="type">Type of the series</param>
    /// <param name="index">Index position on the chart of the series</param>
    /// <param name="unit">Unit for the series axis</param>
    public Series( String name, SeriesType type, int index, String unit ) {
        this( name, type, index );
        this.unit = unit;
//        color = null;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /// Constructor method for Chart Series
    /// <param name="name">Name of the chart series</param>
    /// <param name="type">Type of the chart series</param>
    /// <param name="unit">Unit of the serier</param>
    public Series( String name, SeriesType type, String unit ) {
        this( name, type );
        this.unit = unit;
//        Values = new ArrayList<ChartPoint>();
//        index = 0;
//        color = Color.Empty;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /// Constructor method for Chart Series
    /// <param name="name">Name of the chart series</param>
    /// <param name="type">Type of the chart series</param>
    /// <param name="unit">Unit of the serier</param>
    /// <param name="color">Color of the series</param>
    public Series( String name, SeriesType type, String unit, Color color ) {
        this( name, type, unit );
        this.color = color;
//        Values = new ArrayList<ChartPoint>();
//        index = 0;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /// Constructor method for Chart Series
    /// <param name="name">Name of the chart series</param>
    /// <param name="type">Type of the chart series</param>
    /// <param name="unit">Unit of the serier</param>
    /// <param name="color">Color of the series</param>
    /// <param name="symbol">Symbol for the marker in a scatter plot series</param>
    public Series( String name, SeriesType type, String unit, Color color, ScatterMarkerSymbol symbol ) {
        this( name, type, unit, color );
        this.scatterMarkerSymbol = symbol;
//        this.Values = new List<ChartPoint>();
//        this.index = 0;
    }

    /// Add a new point to this series:
    /// <param name="time">Time of the chart point</param>
    /// <param name="value">Value of the chart point</param>
    /// <param name="liveMode">This is a live mode point</param>
    public void addPoint( ZonedDateTime time, BigDecimal value ) {
        addPoint( time, value, false );
    }
    
    public void addPoint( ZonedDateTime time, BigDecimal value, boolean liveMode ) {
        if( values.size() >= 4000 && !liveMode ) 
            // perform rate limiting in backtest mode
            return;

        final ChartPoint chartPoint = new ChartPoint( time, value );
        if( values.size() > 0 && values.get( values.size() - 1 ).x == chartPoint.x )
            // duplicate points at the same time, overwrite the value
            values.set( values.size() - 1, chartPoint );
        else
            values.add( chartPoint );
    }

    /// Get the updates since the last call to this function.
    /// <returns>List of the updates from the series</returns>
    public Series getUpdates() {
        final Series copy = new Series( name, seriesType, index, unit );
        copy.color = this.color;
        copy.scatterMarkerSymbol = this.scatterMarkerSymbol;

//        try {
            //Add the updates since the last 
            for( int i = updatePosition; i < values.size(); i++ ) 
                copy.values.add( values.get( i ) );
            //Shuffle the update point to now:
            updatePosition = values.size();
//        }
//        catch( Exception err ) {
//            log.error( "Ererr );
//        }
        
        return copy;
    }

    /// Removes the data from this series and resets the update position to 0
    public void purge() {
        values.clear();
        updatePosition = 0;
    }
}