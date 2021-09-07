// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Repl.Input
{
    public delegate Task AsyncKeyPressHandler(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken);
}
