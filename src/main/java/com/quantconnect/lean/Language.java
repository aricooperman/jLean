package com.quantconnect.lean;

/// Multilanguage support enum: which language is this project for the interop bridge converter.
//    [JsonConverter(typeof(StringEnumConverter))]
    public enum Language {
        /// C# Language Project
//        [EnumMember(Value = "C#")]
        CSharp,

        /// FSharp Project
//        [EnumMember(Value = "F#")]
        FSharp,

        /// Visual Basic Project
//        [EnumMember(Value = "VB")]
        VisualBasic,

        /// Java Language Project
//        [EnumMember(Value = "Ja")]
        Java,

        /// Python Language Project
//        [EnumMember(Value = "Py")]
        Python
    }