// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;

namespace Microsoft.Repl
{
    public class Disposable : IDisposable
    {
        private Action _onDispose;

        public Disposable(Action onDispose)
        {
            _onDispose = onDispose;
        }
        public virtual void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
            GC.SuppressFinalize(this);
        }
    }

    public class Disposable<T> : Disposable
        where T : class
    {
        public Disposable(T value, Action onDispose)
            : base (onDispose)
        {
            Value = value;
        }

        public T Value { get; private set; }

        public override void Dispose()
        {
            if (Value is IDisposable d)
            {
                d.Dispose();
                Value = null;
            }

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
