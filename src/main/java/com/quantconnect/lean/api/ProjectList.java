package com.quantconnect.lean.api;

import java.util.List;

/// Project list response
public class ProjectList extends RestResponse {

    /// List of projects for the authenticated user
//    [JsonProperty(PropertyName = "projects")]
    public List<Project> projects;
}