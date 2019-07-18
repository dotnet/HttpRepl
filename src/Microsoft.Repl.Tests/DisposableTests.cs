// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Repl.Tests
{
    public class DisposableTests
    {
        [Fact]
        public void NonGeneric_Dispose_WithNullAction_DoesNotCrash()
        {
            using (Disposable disposable = new Disposable(null))
            {

            }
        }

        [Fact]
        public void Generic_Dispose_WithNullProperty_DoesNotCrash()
        {
            using (Disposable<DisposableStub> disposable = new Disposable<DisposableStub>(null, null))
            {

            }
        }

        [Fact]
        public void NonGeneric_Dispose_WithAction_CallsOnDispose()
        {
            bool onDisposeWasCalled = false;
            Action onDispose = () => onDisposeWasCalled = true;

            using (Disposable disposable = new Disposable(onDispose))
            {

            }

            Assert.True(onDisposeWasCalled);
        }

        [Fact]
        public void Generic_Dispose_WithAction_CallsOnDispose()
        {
            bool onDisposeWasCalled = false;
            Action onDispose = () => onDisposeWasCalled = true;

            using (Disposable<ClassStub> disposable = new Disposable<ClassStub>(new ClassStub(), onDispose))
            {

            }

            Assert.True(onDisposeWasCalled);
        }

        [Fact]
        public void Generic_Dispose_WithDisposable_CallsDispose()
        {
            DisposableStub disposableStub = new DisposableStub();
            using (Disposable<DisposableStub> disposable = new Disposable<DisposableStub>(disposableStub, () => { }))
            {

            }

            Assert.True(disposableStub.DisposeWasCalled);
        }

        public class ClassStub { }
        public class DisposableStub : IDisposable
        {
            public bool DisposeWasCalled { get; private set; } = false;
            public void Dispose()
            {
                DisposeWasCalled = true;
            }
        }
    }
}
