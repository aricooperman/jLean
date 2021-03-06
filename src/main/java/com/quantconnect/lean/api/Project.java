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
*/

package com.quantconnect.lean.api;

import java.time.LocalDateTime;
import java.util.List;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Response from reading a project by id.
 */
public class Project extends RestResponse {

    /**
     * Project id
     */
    @JsonProperty( "projectId" )
    public int projectId;

    /**
     * Name of the project
     */
    @JsonProperty( "name" )
    public String name;

    /**
     * Date the project was created
     */
    @JsonProperty( "created" )
    public LocalDateTime created;

    /**
     * Modified date for the project
     */
    @JsonProperty( "modified" )
    public LocalDateTime modified;

    /**
     * Files for the project
     */
    @JsonProperty( "files" )
    public List<ProjectFile> files;
}