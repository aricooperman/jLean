﻿/*
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
 *
*/

package com.quantconnect.lean.interfaces;

import com.quantconnect.lean.data.auxiliary.MapFileResolver;

//using System.ComponentModel.Composition;
//using QuantConnect.Data.Auxiliary;

 * Provides instances of <see cref="MapFileResolver"/> at run time
//[InheritedExport(typeof(IMapFileProvider))]
public interface IMapFileProvider {
     * Gets a <see cref="MapFileResolver"/> representing all the map
     * files for the specified market
     * @param market The equity market, for example, 'usa'
    @returns A <see cref="MapFileResolver"/> containing all map files for the specified market
    MapFileResolver get( String market );
}
