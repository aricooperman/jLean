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
using System.Threading;

package com.quantconnect.lean.Util
{
    /**
    /// Provides extension methods to make working with the <see cref="ReaderWriterLockSlim"/> class easier
    */
    public static class ReaderWriterLockSlimExtensions
    {
        /**
        /// Opens the read lock
        */
         * @param readerWriterLockSlim">The lock to open for read
        @returns A disposable reference which will release the lock upon disposal
        public static IDisposable Read(this ReaderWriterLockSlim readerWriterLockSlim) {
            return new ReaderLockToken(readerWriterLockSlim);
        }

        /**
        /// Opens the write lock
        */
         * @param readerWriterLockSlim">The lock to open for write
        @returns A disposale reference which will release thelock upon disposal
        public static IDisposable Write(this ReaderWriterLockSlim readerWriterLockSlim) {
            return new WriteLockToken(readerWriterLockSlim);
        }

        private sealed class ReaderLockToken : ReaderWriterLockSlimToken
        {
            public ReaderLockToken(ReaderWriterLockSlim readerWriterLockSlim)
                : base(readerWriterLockSlim) {
            }

            protected @Override void EnterLock(ReaderWriterLockSlim readerWriterLockSlim) {
                readerWriterLockSlim.EnterReadLock();
            }

            protected @Override void ExitLock(ReaderWriterLockSlim readerWriterLockSlim) {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        private sealed class WriteLockToken : ReaderWriterLockSlimToken
        {
            public WriteLockToken(ReaderWriterLockSlim readerWriterLockSlim)
                : base(readerWriterLockSlim) {
            }

            protected @Override void EnterLock(ReaderWriterLockSlim readerWriterLockSlim) {
                readerWriterLockSlim.EnterWriteLock();
            }

            protected @Override void ExitLock(ReaderWriterLockSlim readerWriterLockSlim) {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        private abstract class ReaderWriterLockSlimToken : IDisposable
        {
            private ReaderWriterLockSlim _readerWriterLockSlim;

            public ReaderWriterLockSlimToken(ReaderWriterLockSlim readerWriterLockSlim) {
                _readerWriterLockSlim = readerWriterLockSlim;
                // ReSharper disable once DoNotCallOverridableMethodsInConstructor -- we control the subclasses, this is fine
                EnterLock(_readerWriterLockSlim);
            }

            protected abstract void EnterLock(ReaderWriterLockSlim readerWriterLockSlim);
            protected abstract void ExitLock(ReaderWriterLockSlim readerWriterLockSlim);

            public void Dispose() {
                if( _readerWriterLockSlim != null ) {
                    ExitLock(_readerWriterLockSlim);
                    _readerWriterLockSlim = null;
                }
            }
        }
    }
}
