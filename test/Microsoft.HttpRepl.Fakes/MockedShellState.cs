// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Input;
using Microsoft.Repl.Suggestions;
using Moq;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockedShellState : IShellState
    {
        private readonly ShellState _shellState;
        private readonly StringBuilder _output = new StringBuilder();
        public MockedShellState(IInputManager inputManager = null)
        {
            DefaultCommandDispatcher<object> defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, new object());
            Mock<IConsoleManager> mockedConsoleManager = new Mock<IConsoleManager>();
            Mock<IWritable> mockedErrorWritable = new Mock<IWritable>();
            mockedErrorWritable.Setup(x => x.WriteLine(It.IsAny<string>())).Callback((string s) => ErrorMessage = s);
            mockedConsoleManager.Setup(x => x.Error).Returns(mockedErrorWritable.Object);
            mockedConsoleManager.Setup(x => x.Write(It.IsAny<string>())).Callback((string s) => _output.Append(s));
            mockedConsoleManager.Setup(x => x.WriteLine(It.IsAny<string>())).Callback((string s) => _output.AppendLine(s));

            _shellState = new ShellState(defaultCommandDispatcher, inputManager: inputManager, consoleManager: mockedConsoleManager.Object);
        }

        public string ErrorMessage { get; private set; }

        public List<string> Output => _output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();

        public IInputManager InputManager => _shellState.InputManager;

        public ICommandHistory CommandHistory => _shellState.CommandHistory;

        public IConsoleManager ConsoleManager => _shellState.ConsoleManager;

        public ICommandDispatcher CommandDispatcher => _shellState.CommandDispatcher;

        public ISuggestionManager SuggestionManager => _shellState.SuggestionManager;

        public bool IsExiting { get => _shellState.IsExiting; set => _shellState.IsExiting = value; }

        public void MoveCarets(int positions)
        {
            ConsoleManager?.MoveCaret(positions);
            InputManager?.MoveCaret(positions);
        }
    }
}
