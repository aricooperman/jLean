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
 *
*/

using System;

package com.quantconnect.lean.Util
{
    /**
     * Represents a read-only reference to any value, T
    */
     * <typeparam name="T The data type the reference points to</typeparam>
    public interface IReadOnlyRef<out T>
    {
        /**
         * Gets the current value this reference points to
        */
        T Value { get; }
    }

    /**
     * Represents a reference to any value, T
    */
     * <typeparam name="T The data type the reference points to</typeparam>
    public sealed class Ref<T> : IReadOnlyRef<T>
    {
        private final Func<T> _getter;
        private final Action<T> _setter;

        /**
         * Initializes a new instance of the <see cref="Ref{T}"/> class
        */
         * @param getter A function delegate to get the current value
         * @param setter A function delegate to set the current value
        public Ref(Func<T> getter, Action<T> setter) {
            _getter = getter;
            _setter = setter;
        }

        /**
         * Gets or sets the value of this reference
        */
        public T Value
        {
            get { return _getter(); }
            set { _setter(value); }
        }

        /**
         * Returns a read-only version of this instance
        */
        @returns A new instance with read-only semantics/gaurantees
        public IReadOnlyRef<T> AsReadOnly() {
            return new Ref<T>(_getter, value =>
            {
                throw new InvalidOperationException( "This instance is read-only.");
            });
        }
    }

    /**
     * Provides some helper methods that leverage C# type inference
    */
    public static class Ref
    {
        /**
         * Creates a new <see cref="Ref{T}"/> instance
        */
        public static Ref<T> Create<T>(Func<T> getter, Action<T> setter) {
            return new Ref<T>(getter, setter);
        }
        /**
         * Creates a new <see cref="IReadOnlyRef{T}"/> instance
        */
        public static IReadOnlyRef<T> CreateReadOnly<T>(Func<T> getter) {
            return new Ref<T>(getter, null ).AsReadOnly();
        }
        /**
         * Creates a new <see cref="Ref{T}"/> instance by closing over
         * the specified <paramref name="initialValue"/> variable.
         * NOTE: This won't close over the variable input to the function,
         * but rather a copy of the variable. This reference will use it's
         * own storage.
        */
        public static Ref<T> Create<T>(T initialValue) {
            return new Ref<T>(() -> initialValue, value -> { initialValue = value; });
        }
    }
}
