// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Suggestions
{
    public class SuggestionManager : ISuggestionManager
    {
        private int _currentSuggestion;
        private IReadOnlyList<string> _suggestions;
        private ICoreParseResult _expected;

        public void NextSuggestion(IShellState shellState)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            string line = shellState.InputManager.GetCurrentBuffer();
            ICoreParseResult parseResult = shellState.CommandDispatcher.Parser.Parse(line, shellState.InputManager.CaretPosition);
            string currentSuggestion;

            //Check to see if we're continuing before querying for suggestions again
            if (_expected != null
                && string.Equals(_expected.CommandText, parseResult.CommandText, StringComparison.Ordinal)
                && _expected.SelectedSection == parseResult.SelectedSection
                && _expected.CaretPositionWithinSelectedSection == parseResult.CaretPositionWithinSelectedSection)
            {
                if (_suggestions == null || _suggestions.Count == 0)
                {
                    return;
                }

                _currentSuggestion = (_currentSuggestion + 1) % _suggestions.Count;
                currentSuggestion = _suggestions[_currentSuggestion];
            }
            else
            {
                _currentSuggestion = 0;
                _suggestions = shellState.CommandDispatcher.CollectSuggestions(shellState);

                if (_suggestions == null || _suggestions.Count == 0)
                {
                    return;
                }

                currentSuggestion = _suggestions[0];
            }

            //We now have a suggestion, take the command text leading up to the section being suggested for,
            //  concatenate that and the suggestion text, turn it in to keys, submit it to the input manager,
            //  reset the caret, store the parse result of the new text as what's expected for a continuation
            string newText = parseResult.CommandText.Substring(0, parseResult.SectionStartLookup[parseResult.SelectedSection]) + currentSuggestion;
            _expected = shellState.CommandDispatcher.Parser.Parse(newText, newText.Length);
            shellState.InputManager.SetInput(shellState, newText);
        }

        public void PreviousSuggestion(IShellState shellState)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            string line = shellState.InputManager.GetCurrentBuffer();
            ICoreParseResult parseResult = shellState.CommandDispatcher.Parser.Parse(line, shellState.InputManager.CaretPosition);
            string currentSuggestion;

            //Check to see if we're continuing before querying for suggestions again
            if (_expected != null
                && string.Equals(_expected.CommandText, parseResult.CommandText, StringComparison.Ordinal)
                && _expected.SelectedSection == parseResult.SelectedSection
                && _expected.CaretPositionWithinSelectedSection == parseResult.CaretPositionWithinSelectedSection)
            {
                if (_suggestions == null || _suggestions.Count == 0)
                {
                    return;
                }

                _currentSuggestion = (_currentSuggestion - 1 + _suggestions.Count) % _suggestions.Count;
                currentSuggestion = _suggestions[_currentSuggestion];
            }
            else
            {
                _suggestions = shellState.CommandDispatcher.CollectSuggestions(shellState);
                _currentSuggestion = _suggestions.Count - 1;

                if (_suggestions == null || _suggestions.Count == 0)
                {
                    return;
                }

                currentSuggestion = _suggestions[_suggestions.Count - 1];
            }

            //We now have a suggestion, take the command text leading up to the section being suggested for,
            //  concatenate that and the suggestion text, turn it in to keys, submit it to the input manager,
            //  reset the caret, store the parse result of the new text as what's expected for a continuation
            string newText = parseResult.CommandText.Substring(0, parseResult.SectionStartLookup[parseResult.SelectedSection]) + currentSuggestion;
            _expected = shellState.CommandDispatcher.Parser.Parse(newText, newText.Length);
            shellState.InputManager.SetInput(shellState, newText);
        }
    }
}
