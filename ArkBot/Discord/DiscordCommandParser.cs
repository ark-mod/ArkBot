using System.Collections.Generic;

namespace ArkBot.Discord
{
    // Code from Discord.NET 0.9 (https://github.com/RogueException/Discord.Net/blob/0.9/src/Discord.Net.Commands/CommandErrorEventArgs.cs)
    internal static class CommandParser
    {
        public enum CommandErrorType { Exception, UnknownCommand, BadPermissions, BadArgCount, InvalidInput }

        private enum ParserPart
        {
            None,
            Parameter,
            QuotedParameter,
            DoubleQuotedParameter
        }
        
        private static bool IsWhiteSpace(char c) => c == ' ' || c == '\n' || c == '\r' || c == '\t';

        //TODO: Check support for escaping
        public static CommandErrorType? ParseArgs(string input, int startPos, out string[] args)
        {
            if (input == null)
            {
                args = new string[] { };
                return null;
            }

            ParserPart currentPart = ParserPart.None;
            int startPosition = startPos;
            int endPosition = startPos;
            int inputLength = input.Length;
            bool isEscaped = false;

            List<string> argList = new List<string>();

            args = null;

            if (input == "")
                return CommandErrorType.InvalidInput;

            while (endPosition < inputLength)
            {
                char currentChar = input[endPosition++];
                if (isEscaped)
                    isEscaped = false;
                else if (currentChar == '\\')
                    isEscaped = true;

                bool isWhitespace = IsWhiteSpace(currentChar);
                if (endPosition == startPosition + 1 && isWhitespace) //Has no text yet, and is another whitespace
                {
                    startPosition = endPosition;
                    continue;
                }

                switch (currentPart)
                {
                    case ParserPart.None:
                        if ((!isEscaped && currentChar == '\"'))
                        {
                            currentPart = ParserPart.DoubleQuotedParameter;
                            startPosition = endPosition;
                        }
                        else if ((!isEscaped && currentChar == '\''))
                        {
                            currentPart = ParserPart.QuotedParameter;
                            startPosition = endPosition;
                        }
                        else if ((!isEscaped && isWhitespace) || endPosition >= inputLength)
                        {
                            int length = (isWhitespace ? endPosition - 1 : endPosition) - startPosition;
                            if (length == 0)
                                startPosition = endPosition;
                            else
                            {
                                string temp = input.Substring(startPosition, length);
                                argList.Add(temp);
                                currentPart = ParserPart.None;
                                startPosition = endPosition;
                            }
                        }
                        break;
                    case ParserPart.QuotedParameter:
                        if ((!isEscaped && currentChar == '\''))
                        {
                            string temp = input.Substring(startPosition, endPosition - startPosition - 1);
                            argList.Add(temp);
                            currentPart = ParserPart.None;
                            startPosition = endPosition;
                        }
                        else if (endPosition >= inputLength)
                            return CommandErrorType.InvalidInput;
                        break;
                    case ParserPart.DoubleQuotedParameter:
                        if ((!isEscaped && currentChar == '\"'))
                        {
                            string temp = input.Substring(startPosition, endPosition - startPosition - 1);
                            argList.Add(temp);
                            currentPart = ParserPart.None;
                            startPosition = endPosition;
                        }
                        else if (endPosition >= inputLength)
                            return CommandErrorType.InvalidInput;
                        break;
                }
            }

            //Unclosed quotes
            if (currentPart == ParserPart.QuotedParameter ||
                currentPart == ParserPart.DoubleQuotedParameter)
                return CommandErrorType.InvalidInput;

            args = argList.ToArray();
            return null;
        }
    }
}