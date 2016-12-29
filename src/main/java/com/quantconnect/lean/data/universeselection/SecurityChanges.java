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

//using System.Linq;

package com.quantconnect.lean.data.universeselection;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.Iterables;
import com.google.common.collect.Sets;
import com.quantconnect.lean.securities.Security;

/**
 * Defines the additions and subtractions to the algorithm's security subscriptions
 */
public class SecurityChanges {

    /**
     * Gets an instance that represents no changes have been made
     */
    public static final SecurityChanges None = new SecurityChanges( new ArrayList<Security>(), new ArrayList<Security>() );

    private final Set<Security> addedSecurities;
    private final Set<Security> removedSecurities;

    /**
     * Gets the symbols that were added by universe selection
    */
    public ImmutableList<Security> getAddedSecurities() {
        return ImmutableList.copyOf( addedSecurities.stream().sorted( Comparator.comparing( x -> x.getSymbol().getValue() ) ).collect( Collectors.toList() ) );
    }

    /**
     * Gets the symbols that were removed by universe selection. This list may
     * include symbols that were removed, but are still receiving data due to
     * existing holdings or open orders
    */
    public ImmutableList<Security> getRemovedSecurities() {
        return ImmutableList.copyOf( removedSecurities.stream().sorted( Comparator.comparing( x -> x.getSymbol().getValue() ) ).collect( Collectors.toList() ) );
    }

    /**
     * Initializes a new instance of the <see cref="SecurityChanges"/> class
     * @param addedSecurities Added symbols list
     * @param removedSecurities Removed symbols list
     */
    public SecurityChanges( final Iterable<Security> addedSecurities, final Iterable<Security> removedSecurities ) {
        this.addedSecurities = new HashSet<>();
        this.removedSecurities = new HashSet<>();
        
        Iterables.addAll( this.addedSecurities, addedSecurities );
        Iterables.addAll( this.removedSecurities, removedSecurities );
    }

    /**
     * Returns a new instance of <see cref="SecurityChanges"/> with the specified securities marked as added
     * @param securities The added securities
     * @returns A new security changes instance with the specified securities marked as added
     */
    public static SecurityChanges added( final Security... securities ) {
        if( securities == null || securities.length == 0 )
            return None;
       
        return new SecurityChanges( Arrays.asList( securities ), new ArrayList<Security>() );
    }

    /**
     * Returns a new instance of <see cref="SecurityChanges"/> with the specified securities marked as removed
     * @param securities The removed securities
     * @returns A new security changes instance with the specified securities marked as removed
     */
    public static SecurityChanges removed( final Security... securities ) {
        if( securities == null || securities.length == 0 )
            return None;
        
        return new SecurityChanges( new ArrayList<Security>(), Arrays.asList( securities ) );
    }

    /**
     * Combines the results of two <see cref="SecurityChanges"/>
     * @param left The left side of the operand
     * @param right The right side of the operand
     * @returns Adds the additions together and removes any removals found in the additions, that is, additions take precendence
     */
    public static SecurityChanges merge( final SecurityChanges left, final SecurityChanges right ) {
        // common case is adding something to nothing, shortcut these to prevent linqness
        if( left == None ) return right;
        if( right == None ) return left;

        final Set<Security> additions = Sets.union( left.addedSecurities, right.addedSecurities );
        final List<Security> removals = Sets.union( left.removedSecurities, right.removedSecurities ).stream().filter( x -> !additions.contains( x ) ).collect( Collectors.toList() );
        return new SecurityChanges( additions, removals );
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        if( addedSecurities.size() == 0 && removedSecurities.size() == 0 )
            return "SecurityChanges: None";
        
        String added = "";
        if( addedSecurities.size() != 0 )
            added = " Added: " + String.join( ",", getAddedSecurities().stream().map( x -> x.getSymbol().getId().toString() ).collect( Collectors.toList() ) );
        
        String removed = "";
        if( removedSecurities.size() != 0 )
            removed = " Removed: " + String.join( ",", getRemovedSecurities().stream().map( x -> x.getSymbol().getId().toString() ).collect( Collectors.toList() ) );

        return "SecurityChanges: " + added + removed;
    }
}
