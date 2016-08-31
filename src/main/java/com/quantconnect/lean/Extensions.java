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

package com.quantconnect.lean;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.Duration;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.time.temporal.ChronoUnit;
import java.util.ArrayList;
import java.util.List;

//using System.Collections.Concurrent;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using NodaTime;
//using QuantConnect.Data;
//using QuantConnect.Securities;
//using Timer = System.Timers.Timer;

/**
 * Extensions function collections - group all static extensions functions here.
 */
//TODO extract into seperate util classes
public class Extensions {
    
    public static final BigDecimal ONE_THOUSAND = BigDecimal.valueOf( 1000 );
    public static final BigDecimal TWO = BigDecimal.valueOf( 2 );

    
    private Extensions() { }

    public static BigDecimal midPrice( BigDecimal price1, BigDecimal price2 ) {
        return (price1.add( price2 )).divide( TWO, RoundingMode.HALF_UP );
    }
    
    /**
     * Extension to move one element from list from A to position B.
     * @param list List we're operating on.
     * @param oldIndex Index of variable we want to move.
     * @param newInde New location for the variable
     */
    public static <T> void move( List<T> list, int oldIndex, int newIndex ) {
        final T oItem = list.get( oldIndex );
        list.remove( oldIndex );
        if( newIndex > oldIndex ) 
            newIndex--;
        list.add( newIndex, oItem );
    }

    /**
     * Extension method to convert a String into a byte array
     * @param str String to convert to bytes.
     * @returns Byte array
     */
    public static byte[] getBytes( String str ) {
        return str.getBytes();
//        bytes = new byte[str.Length * sizeof(char)];
//        Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
//        return bytes;
    }

//     * Extentsion method to clear all items from a thread safe queue
//     * Small risk of race condition if a producer is adding to the list.
//     * <typeparam name="T Queue type</typeparam>
//     * @param queue queue object
//    public static void clear<T>( ConcurrentQueue<T> queue) 
//    {
//        T item;
//        while (queue.TryDequeue(out item)) {
//            // NOP
//        }
//    }
//
//    /**
//     * Extension method to convert a byte array into a string.
//    */
//     * @param bytes Byte array to convert.
//    @returns String from bytes.
//    public static String GetString(this byte[] bytes) 
//    {
//        chars = new char[bytes.Length / sizeof(char)];
//        Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
//        return new string(chars);
//    }
//
//    /**
//     * Extension method to convert a String to a MD5 hash.
//    */
//     * @param str String we want to MD5 encode.
//    @returns MD5 hash of a string
//    public static String ToMD5(this String str) 
//    {
//        builder = new StringBuilder();
//        using (md5Hash = MD5.Create())
//        {
//            data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
//            foreach (t in data) builder.Append(t.toString( "x2"));
//        }
//        return builder.toString();
//    }
//
//    /**
//     * Extension method to automatically set the update value to same as "add" value for TryAddUpdate. 
//     * This makes the API similar for traditional and concurrent dictionaries.
//    */
//     * <typeparam name="K Key type for Map</typeparam>
//     * <typeparam name="V Value type for dictonary</typeparam>
//     * @param dictionary Dictionary object we're operating on
//     * @param key Key we want to add or update.
//     * @param value Value we want to set.
//    public static void AddOrUpdate<K, V>(this ConcurrentMap<K, V> dictionary, K key, V value)
//    {
//        dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) -> value);
//    }
//
//    /**
//     * Adds the specified element to the collection with the specified key. If an entry does not exist for th
//     * specified key then one will be created.
//    */
//     * <typeparam name="TKey The key type</typeparam>
//     * <typeparam name="TElement The collection element type</typeparam>
//     * <typeparam name="TCollection The collection type</typeparam>
//     * @param dictionary The source dictionary to be added to
//     * @param key The key
//     * @param element The element to be added
//    public static void Add<TKey, TElement, TCollection>(this Map<TKey, TCollection> dictionary, TKey key, TElement element)
//        where TCollection : ICollection<TElement>, new()
//    {
//        TCollection list;
//        if( !dictionary.TryGetValue(key, out list))
//        {
//            list = new TCollection();
//            dictionary.Add(key, list);
//        }
//        list.Add(element);
//    }
//
//    /**
//     * Extension method to round a double value to a fixed number of significant figures instead of a fixed BigDecimal places.
//    */
//     * @param d Double we're rounding
//     * @param digits Number of significant figures
//    @returns New double rounded to digits-significant figures
//    public static double RoundToSignificantDigits(this double d, int digits)
//    {
//        if( d == 0) return 0;
//        scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
//        return scale * Math.Round(d / scale, digits);
//    }
//
//    /**
//     * Extension method to round a double value to a fixed number of significant figures instead of a fixed BigDecimal places.
//    */
//     * @param d Double we're rounding
//     * @param digits Number of significant figures
//    @returns New double rounded to digits-significant figures
//    public static BigDecimal RoundToSignificantDigits(this BigDecimal d, int digits)
//    {
//        if( d == 0) return 0;
//        scale = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double) Math.Abs(d))) + 1);
//        return scale * Math.Round(d / scale, digits);
//    }

    /**
     * Provides global smart rounding, numbers larger than 1000 will round to 4 BigDecimal places,
     * while numbers smaller will round to 7 significant digits
     */
    public static BigDecimal smartRounding( BigDecimal input ) {
        // any larger numbers we still want some decimal places
        if( input.compareTo( ONE_THOUSAND ) > 0 )
            return input.setScale( 4, RoundingMode.HALF_UP );

        // this is good for forex and other small numbers
        return input.setScale( 7, RoundingMode.HALF_UP );
    }

//    /**
//     * Casts the specified input value to a BigDecimal while acknowledging the overflow conditions
//    */
//     * @param input The value to be cast
//    @returns The input value as a decimal, if the value is too large or to small to be represented
//     * as a decimal, then the closest BigDecimal value will be returned
//    public static BigDecimal SafeDecimalCast(this double input)
//    {
//        if( input <= (double) decimal.MinValue) return decimal.MinValue;
//        if( input >= (double) decimal.MaxValue) return decimal.MaxValue;
//        return (decimal) input;
//    }
//
//    /**
//     * Extension method for faster String to BigDecimal conversion. 
//    */
//     * @param str String to be converted to positive BigDecimal value
//     * Method makes some assuptions - always numbers, no "signs" +,- etc.
//    @returns Decimal value of the string
//    public static BigDecimal ToDecimal(this String str)
//    {
//        long value = 0;
//        decimalPlaces = 0;
//        boolean hasDecimals = false;
//
//        for (i = 0; i < str.Length; i++)
//        {
//            ch = str[i];
//            if( ch == '.')
//            {
//                hasDecimals = true;
//                decimalPlaces = 0;
//            }
//            else
//            {
//                value = value * 10 + (ch - '0');
//                decimalPlaces++;
//            }
//        }
//
//        lo = (int)value;
//        mid = (int)(value >> 32);
//        return new decimal(lo, mid, 0, false, (byte)(hasDecimals ? decimalPlaces : 0));
//    }
//
//    /**
//     * Extension method for faster String to Int32 conversion. 
//    */
//     * @param str String to be converted to positive Int32 value
//     * Method makes some assuptions - always numbers, no "signs" +,- etc.
//    @returns Int32 value of the string
//    public static int ToInt32(this String str)
//    {
//        int value = 0;
//        for (i = 0; i < str.Length; i++)
//        {
//            value = value * 10 + (str[i] - '0');
//        }
//        return value;
//    }
//
//    /**
//     * Extension method for faster String to Int64 conversion. 
//    */
//     * @param str String to be converted to positive Int64 value
//     * Method makes some assuptions - always numbers, no "signs" +,- etc.
//    @returns Int32 value of the string
//    public static long ToInt64(this String str)
//    {
//        long value = 0;
//        for (i = 0; i < str.Length; i++)
//        {
//            value = value * 10 + (str[i] - '0');
//        }
//        return value;
//    }

    /**
     * Breaks the specified String into csv components, all commas are considered separators
     * @param str The String to be broken into csv
     * @param size The expected size of the output list
     * @returns A list of the csv pieces
     */
    public static String[] toCsv( String str ) {
        return toCsv( str, 4 );
    }
    
    public static String[] toCsv( String str, int size ) {
        int last = 0;
        
        final List<String> csv = new ArrayList<String>( size );
        final char[] chars = str.toCharArray();
        for( int i = 0; i < chars.length; i++ ) {
            if( chars[i] == ',' ) {
                if( last != 0 ) 
                    last++;

                csv.add( new String( chars, last, i - last ) );
                last = i;
            }
        }
        
        if( last != 0 ) 
            last++;
        
        csv.add( new String( chars, last, chars.length - last - 1 ) );
        return csv.toArray( new String[csv.size()] );
    }

//    /**
//     * Check if a number is NaN or equal to zero
//    */
//     * @param value The double value to check
//    public static boolean IsNaNOrZero(this double value)
//    {
//        return double.IsNaN(value) || Math.Abs(value) < double.Epsilon;
//    }
//
//    /**
//     * Gets the smallest positive number that can be added to a BigDecimal instance and return
//     * a new value that does not == the old value
//    */
//    public static BigDecimal GetDecimalEpsilon()
//    {
//        return new decimal(1, 0, 0, false, 27); //1e-27m;
//    }
//
//    /**
//     * Extension method to extract the extension part of this file name if it matches a safe list, or return a ".custom" extension for ones which do not match.
//    */
//     * @param str String we're looking for the extension for.
//    @returns Last 4 character String of string.
//    public static String GetExtension(this String str) {
//        ext = str.Substring(Math.Max(0, str.Length - 4));
//        allowedExt = new List<String>() { ".zip", ".csv", ".json" };
//        if( !allowedExt.Contains(ext))
//        {
//            ext = ".custom";
//        }
//        return ext;
//    }
//
//    /**
//     * Extension method to convert strings to stream to be read.
//    */
//     * @param str String to convert to stream
//    @returns Stream instance
//    public static Stream ToStream(this String str) 
//    {
//        stream = new MemoryStream();
//        writer = new StreamWriter(stream);
//        writer.Write(str);
//        writer.Flush();
//        stream.Position = 0;
//        return stream;
//    }
//
//    /**
//     * Extension method to round a timeSpan to nearest timespan period.
//    */
//     * @param time TimeSpan To Round
//     * @param roundingInterval Rounding Unit
//     * @param roundingType Rounding method
//    @returns Rounded timespan
//    public static Duration Round(this Duration time, Duration roundingInterval, MidpointRounding roundingType) 
//    {
//        if( roundingInterval == Duration.ZERO)
//        {
//            // divide by zero exception
//            return time;
//        }
//
//        return new TimeSpan(
//            Convert.ToInt64(Math.Round(
//                time.Ticks / (decimal)roundingInterval.Ticks,
//                roundingType
//            )) * roundingInterval.Ticks
//        );
//    }
//
//    
//    /**
//     * Extension method to round timespan to nearest timespan period.
//    */
//     * @param time Base timespan we're looking to round.
//     * @param roundingInterval Timespan period we're rounding.
//    @returns Rounded timespan period
//    public static Duration Round(this Duration time, Duration roundingInterval)
//    {
//        return Round(time, roundingInterval, MidpointRounding.ToEven);
//    }
//
//    /**
//     * Extension method to round a datetime down by a timespan interval.
//    */
//     * @param dateTime Base DateTime object we're rounding down.
//     * @param interval Timespan interval to round to.
//    @returns Rounded datetime
//    public static DateTime RoundDown(this DateTime dateTime, Duration interval)
//    {
//        if( interval == Duration.ZERO)
//        {
//            // divide by zero exception
//            return dateTime;
//        }
//        return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
//    }
//
//    /**
//     * Extension method to round a datetime down by a timespan interval until it's
//     * within the specified exchange's open hours. This works by first rounding down
//     * the specified time using the interval, then producing a bar between that
//     * rounded time and the interval plus the rounded time and incrementally walking
//     * backwards until the exchange is open
//    */
//     * @param dateTime Time to be rounded down
//     * @param interval Timespan interval to round to.
//     * @param exchangeHours The exchange hours to determine open times
//     * @param extendedMarket True for extended market hours, otherwise false
//    @returns Rounded datetime
//    public static DateTime ExchangeRoundDown(this DateTime dateTime, Duration interval, SecurityExchangeHours exchangeHours, boolean extendedMarket)
//    {
//        // can't round against a zero interval
//        if( interval == Duration.ZERO) return dateTime;
//
//        rounded = dateTime.RoundDown(interval);
//        while (!exchangeHours.IsOpen(rounded, rounded + interval, extendedMarket))
//        {
//            rounded -= interval;
//        }
//        return rounded;
//    }
//
//    /**
//     * Extension method to round a datetime to the nearest unit timespan.
//    */
//     * @param datetime Datetime object we're rounding.
//     * @param roundingInterval Timespan rounding period.s
//    @returns Rounded datetime
//    public static DateTime Round(this DateTime datetime, Duration roundingInterval) 
//    {
//        return new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
//    }
//
//    /**
//     * Extension method to explicitly round up to the nearest timespan interval.
//    */
//     * @param time Base datetime object to round up.
//     * @param d Timespan interval for rounding
//    @returns Rounded datetime
//    public static DateTime RoundUp(this DateTime time, Duration d)
//    {
//        if( d == Duration.ZERO)
//        {
//            // divide by zero exception
//            return time;
//        }
//        return new DateTime(((time.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
//    }

    /**
     * Converts the specified time from the <paramref name="from"/> time zone to the <paramref name="to"/> time zone
     * @param time The time to be converted in terms of the <paramref name="from"/> time zone
     * @param from The time zone the specified <paramref name="time"/> is in
     * @param to The time zone to be converted to
     * @param strict True for strict conversion, this will throw during ambiguitities, false for lenient conversion
     * @returns The time in terms of the to time zone
     */
    public static LocalDateTime convertTo( LocalDateTime time, ZoneId from, ZoneId to/*, boolean strict = false*/ ) {
        if( from.equals( to ) )
            return time;

//        if( strict)
//        {
//            return from.AtStrictly(LocalDateTime.FromDateTime(time)).WithZone(to).ToDateTimeUnspecified();
//        }
        
//        return from.AtLeniently(LocalDateTime.FromDateTime(time)).WithZone(to).ToDateTimeUnspecified();
            
        return time.atZone( from ).withZoneSameInstant( to ).toLocalDateTime();
    }

    /**
     * Converts the specified time from UTC to the <paramref name="to"/> time zone
     * @param time The time to be converted expressed in UTC
     * @param to The destinatio time zone
     * @param strict True for strict conversion, this will throw during ambiguitities, false for lenient conversion
     * @returns The time in terms of the <paramref name="to"/> time zone
     */
    public static LocalDateTime convertFromUtc( LocalDateTime time, ZoneId to/*, boolean strict = false*/ ) {
        return convertTo( time, Global.UTC_ZONE_ID, to/*, strict*/ );
    }

    /**
     * Converts the specified time from the <paramref name="from"/> time zone to <see cref="TimeZones.Utc"/>
     * @param time The time to be converted in terms of the <paramref name="from"/> time zone
     * @param from The time zone the specified <paramref name="time"/> is in
     * @param strict True for strict conversion, this will throw during ambiguitities, false for lenient conversion
     * @returns The time in terms of the to time zone
     */
    public static LocalDateTime convertToUtc( LocalDateTime time, ZoneId from/*, boolean strict = false*/ ) {
//        if( strict)
//        {
//            return from.AtStrictly(LocalDateTime.FromDateTime(time)).ToDateTimeUtc();
//        }
//
//        return from.AtLeniently(LocalDateTime.FromDateTime(time)).ToDateTimeUtc();
        
        return convertTo( time, from, Global.UTC_ZONE_ID );
    }

    public static Duration timeOfDay( LocalDateTime dateTime ) {
        return Duration.between( dateTime.truncatedTo( ChronoUnit.DAYS ), dateTime );
    }

//    /**
//     * Add the reset method to the System.Timer class.
//    */
//     * @param timer System.timer object
//    public static void Reset(this Timer timer)
//    {
//        timer.Stop();
//        timer.Start();
//    }
//
//    /**
//     * Function used to match a type against a String type name. This function compares on the AssemblyQualfiedName,
//     * the FullName, and then just the Name of the type.
//    */
//     * @param type The type to test for a match
//     * @param typeName The name of the type to match
//    @returns True if the specified type matches the type name, false otherwise
//    public static boolean MatchesTypeName(this Type type, String typeName)
//    {
//        if( type.AssemblyQualifiedName == typeName)
//        {
//            return true;
//        }
//        if( type.FullName == typeName)
//        {
//            return true;
//        }
//        if( type.Name == typeName)
//        {
//            return true;
//        }
//        return false;
//    }
//
//    /**
//     * Checks the specified type to see if it is a subclass of the <paramref name="possibleSuperType"/>. This method will
//     * crawl up the inheritance heirarchy to check for equality using generic type definitions (if exists)
//    */
//     * @param type The type to be checked as a subclass of <paramref name="possibleSuperType"/>
//     * @param possibleSuperType The possible superclass of <paramref name="type"/>
//    @returns True if <paramref name="type"/> is a subclass of the generic type definition <paramref name="possibleSuperType"/>
//    public static boolean IsSubclassOfGeneric(this Type type, Type possibleSuperType)
//    {
//        while (type != null && type != typeof(object))
//        {
//            Type cur;
//            if( type.IsGenericType && possibleSuperType.IsGenericTypeDefinition)
//            {
//                cur = type.GetGenericTypeDefinition();
//            }
//            else
//            {
//                cur = type;
//            }
//            if( possibleSuperType == cur)
//            {
//                return true;
//            }
//            type = type.BaseType;
//        }
//        return false;
//    }
//
//    /**
//     * Gets a type's name with the generic parameters filled in the way they would look when
//     * defined in code, such as converting Dictionary&lt;`1,`2&gt; to Dictionary&lt;string,int&gt;
//    */
//     * @param type The type who's name we seek
//    @returns A better type name
//    public static String GetBetterTypeName(this Type type)
//    {
//        String name = type.Name;
//        if( type.IsGenericType)
//        {
//            genericArguments = type.GetGenericArguments();
//            toBeReplaced = "`" + (genericArguments.Length);
//            name = name.Replace(toBeReplaced, "<" + String.join( ", ", genericArguments.Select(x -> x.GetBetterTypeName())) + ">");
//        }
//        return name;
//    }
//
//    /**
//     * Converts the Resolution instance into a Duration instance
//    */
//     * @param resolution The resolution to be converted
//    @returns A Duration instance that represents the resolution specified
//    public static Duration ToTimeSpan(this Resolution resolution)
//    {
//        switch (resolution)
//        {
//            case Resolution.Tick:
//                // ticks can be instantaneous
//                return Duration.ofTicks(0);
//            case Resolution.Second:
//                return Duration.ofSeconds(1);
//            case Resolution.Minute:
//                return Duration.ofMinutes(1);
//            case Resolution.Hour:
//                return Duration.ofHours(1);
//            case Resolution.Daily:
//                return Duration.ofDays(1);
//            default:
//                throw new ArgumentOutOfRangeException( "resolution");
//        }
//    }
//
//    /**
//     * Converts the specified String value into the specified type
//    */
//     * <typeparam name="T The output type</typeparam>
//     * @param value The String value to be converted
//    @returns The converted value
//    public static T ConvertTo<T>(this String value)
//    {
//        return (T) value Extensions.convertTo(  )typeof (T));
//    }
//
//    /**
//     * Converts the specified String value into the specified type
//    */
//     * @param value The String value to be converted
//     * @param type The output type
//    @returns The converted value
//    public static object ConvertTo(this String value, Type type)
//    {
//        if( type.IsEnum)
//        {
//            return Enum.Parse(type, value);
//        }
//
//        if( typeof (IConvertible).IsAssignableFrom(type))
//        {
//            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
//        }
//
//        // try and find a static parse method
//        parse = type.GetMethod( "Parse", new[] {typeof ( String)});
//        if( parse != null )
//        {
//            result = parse.Invoke(null, new object[] {value});
//            return result;
//        }
//
//        return JsonConvert.DeserializeObject(value, type);
//    }
//
//    /**
//     * Blocks the current thread until the current <see cref="T:System.Threading.WaitHandle"/> receives a signal, while observing a <see cref="T:System.Threading.CancellationToken"/>.
//    */
//     * @param waitHandle The wait handle to wait on
//     * @param cancellationToken The <see cref="T:System.Threading.CancellationToken"/> to observe.
//     * <exception cref="T:System.InvalidOperationException The maximum number of waiters has been exceeded.</exception>
//     * <exception cref="T:System.OperationCanceledExcepton"><paramref name="cancellationToken"/> was canceled.</exception>
//     * <exception cref="T:System.ObjectDisposedException The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
//    public static boolean WaitOne(this WaitHandle waitHandle, CancellationToken cancellationToken)
//    {
//        return waitHandle.WaitOne(Timeout.Infinite, cancellationToken);
//    }
//
//    /**
//     * Blocks the current thread until the current <see cref="T:System.Threading.WaitHandle"/> is set, using a <see cref="T:System.TimeSpan"/> to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken"/>.
//    */
//     * 
//    @returns 
//     * true if the <see cref="T:System.Threading.WaitHandle"/> was set; otherwise, false.
//     * 
//     * @param waitHandle The wait handle to wait on
//     * @param timeout A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan"/> that represents -1 milliseconds to wait indefinitely.
//     * @param cancellationToken The <see cref="T:System.Threading.CancellationToken"/> to observe.
//     * <exception cref="T:System.Threading.OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
//     * <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue"/>.</exception>
//     * <exception cref="T:System.InvalidOperationException The maximum number of waiters has been exceeded. </exception><exception cref="T:System.ObjectDisposedException The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
//    public static boolean WaitOne(this WaitHandle waitHandle, Duration timeout, CancellationToken cancellationToken)
//    {
//        return waitHandle.WaitOne((int) timeout.TotalMilliseconds, cancellationToken);
//    }
//
//    /**
//     * Blocks the current thread until the current <see cref="T:System.Threading.WaitHandle"/> is set, using a 32-bit signed integer to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken"/>.
//    */
//     * 
//    @returns 
//     * true if the <see cref="T:System.Threading.WaitHandle"/> was set; otherwise, false.
//     * 
//     * @param waitHandle The wait handle to wait on
//     * @param millisecondsTimeout The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/>(-1) to wait indefinitely.
//     * @param cancellationToken The <see cref="T:System.Threading.CancellationToken"/> to observe.
//     * <exception cref="T:System.Threading.OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
//     * <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an infinite time-out.</exception>
//     * <exception cref="T:System.InvalidOperationException The maximum number of waiters has been exceeded.</exception>
//     * <exception cref="T:System.ObjectDisposedException The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
//    public static boolean WaitOne(this WaitHandle waitHandle, int millisecondsTimeout, CancellationToken cancellationToken)
//    {
//        return WaitHandle.WaitAny(new[] { waitHandle, cancellationToken.WaitHandle }, millisecondsTimeout) == 0;
//    }
//
//    /**
//     * Gets the MD5 hash from a stream
//    */
//     * @param stream The stream to compute a hash for
//    @returns The MD5 hash
//    public static byte[] GetMD5Hash(this Stream stream)
//    {
//        using (md5 = MD5.Create())
//        {
//            return md5.ComputeHash(stream);
//        }
//    }
//
//    /**
//     * Convert a String into the same String with a URL! :)
//    */
//     * @param source The source String to be converted
//    @returns The same source String but with anchor tags around substrings matching a link regex
//    public static String WithEmbeddedHtmlAnchors(this String source)
//    {
//        regx = new Regex( "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*([a-zA-Z0-9\\?\\#\\=\\/])%2$s)?", RegexOptions.IgnoreCase);
//        matches = regx.Matches(source);
//        foreach (Match match in matches)
//        {
//            source = source.Replace(match.Value, "<a href='" + match.Value + "' target='blank'>" + match.Value + "</a>");
//        }
//        return source;
//    }
//
//    /**
//     * Converts the specified <paramref name="enum"/> value to its corresponding lower-case String representation
//    */
//     * @param enum The enumeration value
//    @returns A lower-case String representation of the specified enumeration value
//    public static String ToLower(this Enum @enum)
//    {
//        return @enum.toString().toLowerCase();
//    }
}
