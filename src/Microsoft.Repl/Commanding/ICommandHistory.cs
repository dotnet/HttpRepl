// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;

namespace Microsoft.Repl.Commanding
{
    public interface ICommandHistory
    {
        string GetPreviousCommand();

        string GetNextCommand();

        void AddCommand(string command);

        IDisposable SuspendHistory();
    }
}
