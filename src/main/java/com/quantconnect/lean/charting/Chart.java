/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

package com.quantconnect.lean.charting;

import java.util.HashMap;
import java.util.Map;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

//using System.Drawing;
    
/* 
 * Single Parent Chart Object for Custom Charting
 */
//    [JsonObject]
public class Chart {
    
    private final Logger log = LoggerFactory.getLogger( getClass() );
    
    /// Name of the Chart:
    public String name = "";

    /// List of Series Objects for this Chart:
    public Map<String,Series> series = new HashMap<String,Series>();

    /// Default constructor for chart:
    public Chart() { }

    /// Constructor for a chart
    /// <param name="name">String name of the chart</param>
    public Chart( String name ) {
        this.name = name;
        this.series = new HashMap<String,Series>();
    }

    /// Add a reference to this chart series:
    /// <param name="series">Chart series class object</param>
    public void addSeries( Series s ) {
        //If we dont already have this series, add to the chrt:
        if( !series.containsKey( s.name ) )
            series.put( s.name, s );
        else 
            throw new IllegalArgumentException( "Chart.AddSeries(): Chart series name already exists" );
    }

    /// Fetch the updates of the chart, and save the index position.
    /// <returns></returns>
    public Chart GetUpdates() {
        final Chart copy = new Chart( name );
        
        try {   
            for( Series series : series.values() )
                copy.addSeries( series.getUpdates() );
        }
        catch( Exception err ) {
            log.error( "Exception occured", err );
        }
        
        return copy;
    }
}
