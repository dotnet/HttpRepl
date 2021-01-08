using Microsoft.HttpRepl.Fakes;
using Microsoft.Repl.Input;
using Xunit;

namespace Microsoft.Repl.Tests
{
    public class InputManagerTests
    {
        [Fact]
        public void RemovePreviousCharacter_AtBeginning_DoesNothing()
        {
            // Arrange
            string initialInput = "echo on";
            int initialPosition = 0;
            InputManager inputManager = new(initialInput, initialPosition);
            MockedShellState mockedShellState = new(inputManager);

            // Act
            inputManager.RemovePreviousCharacter(mockedShellState);

            // Assert
            Assert.Equal(initialPosition, inputManager.CaretPosition);
            Assert.Equal(initialInput, inputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RemovePreviousCharacter_AtEnd_RemovesLastCharacter()
        {
            // Arrange
            string initialInput = "echo on";
            int initialPosition = 7;
            string expectedInput = "echo o";
            int expectedPosition = 6;
            InputManager inputManager = new(initialInput, initialPosition);
            MockedShellState mockedShellState = new(inputManager);

            // Act
            inputManager.RemovePreviousCharacter(mockedShellState);

            // Assert
            Assert.Equal(expectedPosition, inputManager.CaretPosition);
            Assert.Equal(expectedInput, inputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RemovePreviousCharacter_InMiddle_RemovesProperCharacter()
        {
            // Arrange
            string initialInput = "echo on";
            int initialPosition = 4;
            string expectedInput = "ech on";
            int expectedPosition = 3;
            InputManager inputManager = new(initialInput, initialPosition);
            MockedShellState mockedShellState = new(inputManager);

            // Act
            inputManager.RemovePreviousCharacter(mockedShellState);

            // Assert
            Assert.Equal(expectedPosition, inputManager.CaretPosition);
            Assert.Equal(expectedInput, inputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RemoveCurrentCharacter_AtEnd_DoesNothing()
        {
            // Arrange
            string initialInput = "echo on";
            int initialPosition = 7;
            InputManager inputManager = new(initialInput, initialPosition);
            MockedShellState mockedShellState = new(inputManager);

            // Act
            inputManager.RemoveCurrentCharacter(mockedShellState);

            // Assert
            Assert.Equal(initialPosition, inputManager.CaretPosition);
            Assert.Equal(initialInput, inputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RemoveCurrentCharacter_AtBeginning_RemovesFirstCharacter()
        {
            // Arrange
            string initialInput = "echo on";
            int initialPosition = 0;
            string expectedInput = "cho on";
            int expectedPosition = 0;
            InputManager inputManager = new(initialInput, initialPosition);
            MockedShellState mockedShellState = new(inputManager);

            // Act
            inputManager.RemoveCurrentCharacter(mockedShellState);

            // Assert
            Assert.Equal(expectedPosition, inputManager.CaretPosition);
            Assert.Equal(expectedInput, inputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RemoveCurrentCharacter_InMiddle_RemovesProperCharacter()
        {
            // Arrange
            string initialInput = "echo on";
            int initialPosition = 4;
            string expectedInput = "echoon";
            int expectedPosition = 4;
            InputManager inputManager = new(initialInput, initialPosition);
            MockedShellState mockedShellState = new(inputManager);

            // Act
            inputManager.RemoveCurrentCharacter(mockedShellState);

            // Assert
            Assert.Equal(expectedPosition, inputManager.CaretPosition);
            Assert.Equal(expectedInput, inputManager.GetCurrentBuffer());
        }
    }
}
