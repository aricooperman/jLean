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

    /**
     * Name of the Series:
     */
    public String name = "";

    /**
     * Axis for the chart series.
     */
    public String unit = "$";

    /**
     * Index/position of the series on the chart.
     */
    public int index = 0;

    /**
     * Values for the series plot:
     * These values are assumed to be in ascending time order (first points earliest, last points latest)
     */
    public List<ChartPoint> values = new ArrayList<>();

    /**
     * Chart type for the series:
     */
    public SeriesType seriesType = SeriesType.Line;

    /**
     * Color the series
     */
    @JsonSerialize( using = ColorJsonSerializer.class, as = String.class ) //[JsonConverter(typeof(ColorJsonConverter))]
    @JsonDeserialize( using = ColorJsonDeserializer.class, as = Color.class )
    public Color color = null; //Color.Empty;

    /**
     * Shape or symbol for the marker in a scatter plot
     */
    public ScatterMarkerSymbol scatterMarkerSymbol = ScatterMarkerSymbol.None;

    /*
     * Get the index of the last fetch update request to only retrieve the "delta" of the previous request.
     */
    private int updatePosition;

    /**
     * Default constructor for chart series
     */
    public Series() { }

    /**
     * Constructor method for Chart Series
     * @param name Name of the chart series
     */
    public Series( final String name ) {
        this.name = name;
//        this.seriesType = SeriesType.Line;
//        this.unit = "$";
//        this.index = 0;
//        this.color = null;
//        this.scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /**
     * Foundational constructor on the series class
     * @param name Name of the series
     * @param type Class of the series
     */
    public Series( final String name, final SeriesType type ) {
        this( name );
        seriesType = type;
//        index = 0;
//        unit = "$";
//        color = null;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /**
     * Foundational constructor on the series class
     * @param name Name of the series
     * @param type Class of the series
     * @param index Index position on the chart of the series
     */
    public Series( final String name, final SeriesType type, final int index ) {
        this( name, type );
        this.index = index;
//        unit = "$";
//        color = null;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /**
     * Foundational constructor on the series class
     * @param name Name of the series
     * @param type Class of the series
     * @param index Index position on the chart of the series
     * @param unit Unit for the series axis
     */
    public Series( final String name, final SeriesType type, final int index, final String unit ) {
        this( name, type, index );
        this.unit = unit;
//        color = null;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /**
     * Constructor method for Chart Series
     * @param name Name of the chart series
     * @param type Class of the chart series
     * @param unit Unit of the serier
     */
    public Series( final String name, final SeriesType type, final String unit ) {
        this( name, type );
        this.unit = unit;
//        Values = new ArrayList<ChartPoint>();
//        index = 0;
//        color = Color.Empty;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /**
     * Constructor method for Chart Series
     * @param name Name of the chart series
     * @param type Class of the chart series
     * @param unit Unit of the serier
     * @param color Color of the series
     */
    public Series( final String name, final SeriesType type, final String unit, final Color color ) {
        this( name, type, unit );
        this.color = color;
//        Values = new ArrayList<ChartPoint>();
//        index = 0;
//        scatterMarkerSymbol = scatterMarkerSymbol.None;
    }

    /**
     * Constructor method for Chart Series
     * @param name Name of the chart series
     * @param type Class of the chart series
     * @param unit Unit of the serier
     * @param color Color of the series
     * @param symbol Symbol for the marker in a scatter plot series
     */
    public Series( final String name, final SeriesType type, final String unit, final Color color, final ScatterMarkerSymbol symbol ) {
        this( name, type, unit, color );
        scatterMarkerSymbol = symbol;
//        this.Values = new List<ChartPoint>();
//        this.index = 0;
    }

    /**
     * Add a new point to this series:
     * @param time Time of the chart point
     * @param value Value of the chart point
     * @param liveMode This is a live mode point
     */
    public void addPoint( final ZonedDateTime time, final BigDecimal value ) {
        addPoint( time, value, false );
    }
    
    public void addPoint( final ZonedDateTime time, final BigDecimal value, final boolean liveMode ) {
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

    /**
     * Get the updates since the last call to this function.
     * @returns List of the updates from the series
     */
    public Series getUpdates() {
        final Series copy = new Series( name, seriesType, index, unit );
        copy.color = color;
        copy.scatterMarkerSymbol = scatterMarkerSymbol;

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

    /**
     * Removes the data from this series and resets the update position to 0
     */
    public void purge() {
        values.clear();
        updatePosition = 0;
    }
}