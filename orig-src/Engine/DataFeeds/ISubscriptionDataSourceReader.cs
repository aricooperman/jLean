using System;
using System.Collections.Generic;
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Represents a type responsible for accepting an input <see cref="SubscriptionDataSource"/>
     * and returning an enumerable of the source's <see cref="BaseData"/>
    */
    public interface ISubscriptionDataSourceReader
    {
        /**
         * Event fired when the specified source is considered invalid, this may
         * be from a missing file or failure to download a remote source
        */
        event EventHandler<InvalidSourceEventArgs> InvalidSource;

        /**
         * Reads the specified <paramref name="source"/>
        */
         * @param source The source to be read
        @returns An <see cref="IEnumerable{BaseData}"/> that contains the data in the source
        IEnumerable<BaseData> Read(SubscriptionDataSource source);
    }
}