// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Input
{
    public static class KeyHandlers
    {
        public static void RegisterDefaultKeyHandlers(IInputManager inputManager)
        {
            inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));

            //Navigation in line
            inputManager.RegisterKeyHandler(ConsoleKey.LeftArrow, LeftArrow);
            inputManager.RegisterKeyHandler(ConsoleKey.LeftArrow, ConsoleModifiers.Control, LeftArrow);
            inputManager.RegisterKeyHandler(ConsoleKey.RightArrow, RightArrow);
            inputManager.RegisterKeyHandler(ConsoleKey.RightArrow, ConsoleModifiers.Control, RightArrow);
            inputManager.RegisterKeyHandler(ConsoleKey.Home, Home);
            inputManager.RegisterKeyHandler(ConsoleKey.A, ConsoleModifiers.Control, Home);
            inputManager.RegisterKeyHandler(ConsoleKey.End, End);
            inputManager.RegisterKeyHandler(ConsoleKey.E, ConsoleModifiers.Control, End);

            //Command history
            inputManager.RegisterKeyHandler(ConsoleKey.UpArrow, UpArrow);
            inputManager.RegisterKeyHandler(ConsoleKey.DownArrow, DownArrow);

            //Completion
            inputManager.RegisterKeyHandler(ConsoleKey.Tab, Tab);
            inputManager.RegisterKeyHandler(ConsoleKey.Tab, ConsoleModifiers.Shift, Tab);

            //Input manipulation
            inputManager.RegisterKeyHandler(ConsoleKey.Escape, Escape);
            inputManager.RegisterKeyHandler(ConsoleKey.U, ConsoleModifiers.Control, Escape);
            inputManager.RegisterKeyHandler(ConsoleKey.Delete, Delete);
            inputManager.RegisterKeyHandler(ConsoleKey.Backspace, Backspace);

            //Insert/Overwrite mode
            inputManager.RegisterKeyHandler(ConsoleKey.Insert, Insert);

            //Execute command
            inputManager.RegisterKeyHandler(ConsoleKey.Enter, Enter);

            //Map non-printable keys that aren't handled by default
            inputManager.RegisterKeyHandler(ConsoleKey.F1, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F2, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F3, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F4, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F5, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F6, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F7, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F8, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F9, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F10, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F11, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F12, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F13, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F14, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F15, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F16, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F17, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F18, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F19, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F20, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F21, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F22, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F23, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.F24, Unhandled);

            inputManager.RegisterKeyHandler(ConsoleKey.Clear, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Execute, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Help, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.PageDown, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.PageUp, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Pause, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Print, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.PrintScreen, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Select, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Separator, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Sleep, Unhandled);

            inputManager.RegisterKeyHandler(ConsoleKey.LeftWindows, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.RightWindows, Unhandled);
            inputManager.RegisterKeyHandler(ConsoleKey.Applications, Unhandled);
        }

        private static Task End(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state.MoveCarets(state.InputManager.GetCurrentBuffer().Length - state.InputManager.CaretPosition);
            return Task.CompletedTask;
        }

        public static Task Home(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            state.MoveCarets(-state.InputManager.CaretPosition);
            return Task.CompletedTask;
        }

        public static Task LeftArrow(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            if (state.InputManager.CaretPosition > 0)
            {
                if (!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    state.MoveCarets(-1);
                }
                else
                {
                    string line = state.InputManager.GetCurrentBuffer();
                    ICoreParseResult parseResult = state.CommandDispatcher.Parser.Parse(line, state.InputManager.CaretPosition);
                    int targetSection = parseResult.SelectedSection - (parseResult.CaretPositionWithinSelectedSection > 0 ? 0 : 1);

                    if (targetSection < 0)
                    {
                        targetSection = 0;
                    }

                    int desiredPosition = parseResult.SectionStartLookup[targetSection];
                    state.MoveCarets(desiredPosition - state.InputManager.CaretPosition);
                }
            }

            return Task.CompletedTask;
        }

        public static Task RightArrow(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            string line = state.InputManager.GetCurrentBuffer();

            if (state.InputManager.CaretPosition < line.Length)
            {
                if (!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    state.MoveCarets(1);
                }
                else
                {
                    ICoreParseResult parseResult = state.CommandDispatcher.Parser.Parse(line, state.InputManager.CaretPosition);
                    int targetSection = parseResult.SelectedSection + 1;

                    if (targetSection >= parseResult.Sections.Count)
                    {
                        state.MoveCarets(line.Length - state.InputManager.CaretPosition);
                    }
                    else
                    {
                        int desiredPosition = parseResult.SectionStartLookup[targetSection];
                        state.MoveCarets(desiredPosition - state.InputManager.CaretPosition);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public static Task UpArrow(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            string line = state.CommandHistory.GetPreviousCommand();
            state.InputManager.SetInput(state, line);
            return Task.CompletedTask;
        }

        public static Task DownArrow(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            string line = state.CommandHistory.GetNextCommand();
            state.InputManager.SetInput(state, line);
            return Task.CompletedTask;
        }

        public static Task Enter(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            return state.CommandDispatcher.ExecuteCommandAsync(state, cancellationToken);
        }

        public static Task Backspace(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            state.InputManager.RemovePreviousCharacter(state);
            return Task.CompletedTask;
        }

        public static Task Unhandled(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public static Task Escape(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            state.InputManager.SetInput(state, string.Empty);
            return Task.CompletedTask;
        }

        public static Task Tab(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                state.SuggestionManager.PreviousSuggestion(state);
            }
            else
            {
                state.SuggestionManager.NextSuggestion(state);
            }

            return Task.CompletedTask;
        }

        public static Task Delete(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            state.InputManager.RemoveCurrentCharacter(state);
            return Task.CompletedTask;
        }

        public static Task Insert(ConsoleKeyInfo keyInfo, IShellState state, CancellationToken cancellationToken)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));

            state.InputManager.IsOverwriteMode = !state.InputManager.IsOverwriteMode;
            return Task.CompletedTask;
        }
    }
}
