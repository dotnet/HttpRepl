// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.Fakes;
using Xunit;

namespace Microsoft.HttpRepl.Tests
{
    public class VerbosityLoggerTests
    {
        [Fact]
        public void Writes_WithNullConsoleManager_DoesNotThrow()
        {
            VerbosityLogger logger = VerbosityLogger.FromConsoleManager(consoleManager: null, isVerbosityEnabled: true);

            logger.Write("");
            logger.WriteVerbose("");
            logger.WriteLine();
            logger.WriteLineVerbose();
            logger.WriteLine("");
            logger.WriteLineVerbose("");
        }

        [Fact]
        public void VerboseWrites_WithVerbosityOff_WritesNothing()
        {
            MockConsoleManager mockConsoleManager = new MockConsoleManager();
            VerbosityLogger logger = VerbosityLogger.FromConsoleManager(mockConsoleManager, isVerbosityEnabled: false);

            logger.WriteVerbose("Some Text");
            Assert.Equal(string.Empty, mockConsoleManager.Output);

            logger.WriteLineVerbose();
            Assert.Equal(string.Empty, mockConsoleManager.Output);

            logger.WriteLineVerbose("Some Text");
            Assert.Equal(string.Empty, mockConsoleManager.Output);
        }
    }
}
