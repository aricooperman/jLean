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

using NodaTime;

package com.quantconnect.lean
{
    /**
    /// Provides access to common time zones
    */
    public static class TimeZones
    {
        /**
        /// Gets the Universal Coordinated time zone.
        */
        public static final ZoneId Utc = ZoneId.Utc;

        /**
        /// Gets the time zone for New York City, USA. This is a daylight savings time zone.
        */
        public static final ZoneId NewYork = ZoneIdProviders.Tzdb["America/New_York"];

        /**
        /// Get the Eastern Standard Time (EST) WITHOUT daylight savings, this is a constant -5 hour offset
        */
        public static final ZoneId EasternStandard = ZoneIdProviders.Tzdb["UTC-05"];

        /**
        /// Gets the time zone for London, England. This is a daylight savings time zone.
        */
        public static final ZoneId London = ZoneIdProviders.Tzdb["Europe/London"];

        /**
        /// Gets the time zone for Hong Kong, China.
        */
        public static final ZoneId HongKong = ZoneIdProviders.Tzdb["Asia/Hong_Kong"];

        /**
        /// Gets the time zone for Tokyo, Japan.
        */
        public static final ZoneId Tokyo = ZoneIdProviders.Tzdb["Asia/Tokyo"];

        /**
        /// Gets the time zone for Rome, Italy. This is a daylight savings time zone.
        */
        public static final ZoneId Rome = ZoneIdProviders.Tzdb["Europe/Rome"];

        /**
        /// Gets the time zone for Sydney, Australia. This is a daylight savings time zone.
        */
        public static final ZoneId Sydney = ZoneIdProviders.Tzdb["Australia/Sydney"];

        /**
        /// Gets the time zone for Vancouver, Canada.
        */
        public static final ZoneId Vancouver = ZoneIdProviders.Tzdb["America/Vancouver"];

        /**
        /// Gets the time zone for Toronto, Canada. This is a daylight savings time zone.
        */
        public static final ZoneId Toronto = ZoneIdProviders.Tzdb["America/Toronto"];

        /**
        /// Gets the time zone for Chicago, USA. This is a daylight savings time zone.
        */
        public static final ZoneId Chicao = ZoneIdProviders.Tzdb["America/Chicago"];

        /**
        /// Gets the time zone for Los Angeles, USA. This is a daylight savings time zone.
        */
        public static final ZoneId LosAngeles = ZoneIdProviders.Tzdb["America/Los_Angeles"];

        /**
        /// Gets the time zone for Phoenix, USA. This is a daylight savings time zone.
        */
        public static final ZoneId Phoenix = ZoneIdProviders.Tzdb["America/Phoenix"];

        /**
        /// Gets the time zone for Auckland, New Zealand. This is a daylight savings time zone.
        */
        public static final ZoneId Auckland = ZoneIdProviders.Tzdb["Pacific/Auckland"];

        /**
        /// Gets the time zone for Moscow, Russia.
        */
        public static final ZoneId Moscow = ZoneIdProviders.Tzdb["Europe/Moscow"];

        /**
        /// Gets the time zone for Madrid, Span. This is a daylight savings time zone.
        */
        public static final ZoneId Madrid = ZoneIdProviders.Tzdb["Europe/Madrid"];

        /**
        /// Gets the time zone for Buenos Aires, Argentia.
        */
        public static final ZoneId BuenosAires = ZoneIdProviders.Tzdb["America/Argentina/Buenos_Aires"];

        /**
        /// Gets the time zone for Brisbane, Australia.
        */
        public static final ZoneId Brisbane = ZoneIdProviders.Tzdb["Australia/Brisbane"];

        /**
        /// Gets the time zone for Sao Paulo, Brazil. This is a daylight savings time zone.
        */
        public static final ZoneId SaoPaulo = ZoneIdProviders.Tzdb["America/Sao_Paulo"];

        /**
        /// Gets the time zone for Cairo, Egypt.
        */
        public static final ZoneId Cairo = ZoneIdProviders.Tzdb["Africa/Cairo"];

        /**
        /// Gets the time zone for Johannesburg, South Africa.
        */
        public static final ZoneId Johannesburg = ZoneIdProviders.Tzdb["Africa/Johannesburg"];

        /**
        /// Gets the time zone for Anchorage, USA. This is a daylight savings time zone.
        */
        public static final ZoneId Anchorage = ZoneIdProviders.Tzdb["America/Anchorage"];

        /**
        /// Gets the time zone for Denver, USA. This is a daylight savings time zone.
        */
        public static final ZoneId Denver = ZoneIdProviders.Tzdb["America/Denver"];

        /**
        /// Gets the time zone for Detroit, USA. This is a daylight savings time zone.
        */
        public static final ZoneId Detroit = ZoneIdProviders.Tzdb["America/Detroit"];

        /**
        /// Gets the time zone for Mexico City, Mexico. This is a daylight savings time zone.
        */
        public static final ZoneId MexicoCity = ZoneIdProviders.Tzdb["America/Mexico_City"];

        /**
        /// Gets the time zone for Jerusalem, Israel. This is a daylight savings time zone.
        */
        public static final ZoneId Jerusalem = ZoneIdProviders.Tzdb["Asia/Jerusalem"];

        /**
        /// Gets the time zone for Shanghai, China.
        */
        public static final ZoneId Shanghai = ZoneIdProviders.Tzdb["Asia/Shanghai"];

        /**
        /// Gets the time zone for Melbourne, Australia. This is a daylight savings time zone.
        */
        public static final ZoneId Melbourne = ZoneIdProviders.Tzdb["Australia/Melbourne"];

        /**
        /// Gets the time zone for Amsterdam, Netherlands. This is a daylight savings time zone.
        */
        public static final ZoneId Amsterdam = ZoneIdProviders.Tzdb["Europe/Amsterdam"];

        /**
        /// Gets the time zone for Athens, Greece. This is a daylight savings time zone.
        */
        public static final ZoneId Athens = ZoneIdProviders.Tzdb["Europe/Athens"];

        /**
        /// Gets the time zone for Berlin, Germany. This is a daylight savings time zone.
        */
        public static final ZoneId Berlin = ZoneIdProviders.Tzdb["Europe/Berlin"];

        /**
        /// Gets the time zone for Bucharest, Romania. This is a daylight savings time zone.
        */
        public static final ZoneId Bucharest = ZoneIdProviders.Tzdb["Europe/Bucharest"];

        /**
        /// Gets the time zone for Dublin, Ireland. This is a daylight savings time zone.
        */
        public static final ZoneId Dublin = ZoneIdProviders.Tzdb["Europe/Dublin"];

        /**
        /// Gets the time zone for Helsinki, Finland. This is a daylight savings time zone.
        */
        public static final ZoneId Helsinki = ZoneIdProviders.Tzdb["Europe/Helsinki"];

        /**
        /// Gets the time zone for Istanbul, Turkey. This is a daylight savings time zone.
        */
        public static final ZoneId Istanbul = ZoneIdProviders.Tzdb["Europe/Istanbul"];

        /**
        /// Gets the time zone for Minsk, Belarus.
        */
        public static final ZoneId Minsk = ZoneIdProviders.Tzdb["Europe/Minsk"];

        /**
        /// Gets the time zone for Paris, France. This is a daylight savings time zone.
        */
        public static final ZoneId Paris = ZoneIdProviders.Tzdb["Europe/Paris"];

        /**
        /// Gets the time zone for Zurich, Switzerland. This is a daylight savings time zone.
        */
        public static final ZoneId Zurich = ZoneIdProviders.Tzdb["Europe/Zurich"];

        /**
        /// Gets the time zone for Honolulu, USA. This is a daylight savings time zone.
        */
        public static final ZoneId Honolulu = ZoneIdProviders.Tzdb["Pacific/Honolulu"];
    }
}