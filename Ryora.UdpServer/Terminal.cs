using System;

namespace Ryora.UdpServer
{
    public static class Terminal
    {
        private static int _line = 1;

        private static int Line
        {
            get { return _line; }
            set
            {
                _line = value;
                if (_line <= 80) return;
                Console.Clear();
                _line = 1;
            }
        }

        public static void LogLine(string message, int line)
        {
            Console.SetCursorPosition(0, line);
            Console.WriteLine(message);
        }

        public static void LogLine(string message)
        {
            LogLine(message, Line++);
        }

        public static void LogLine(string message, ConsoleColor color)
        {
            LogLine(message, Line++, color);
        }

        public static void LogLine(string message, int line, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            LogLine(message, line);
            Console.ResetColor();
        }

        public static void Log(string message, int line, bool beginningOfLine = false)
        {
            Console.SetCursorPosition((beginningOfLine ? 0 : Console.CursorLeft), line);
            Console.Write(message);
        }

        public static void Log(string message, bool beginningOfLine = false)
        {
            Log(message, Line, beginningOfLine);
        }

        public static void Log(string message, ConsoleColor color, bool beginningOfLine = false)
        {
            Log(message, Line, color, beginningOfLine);
        }

        public static void Log(string message, int line, ConsoleColor color, bool beginningOfLine = false)
        {
            Console.ForegroundColor = color;
            Log(message, line, beginningOfLine);
            Console.ResetColor();
        }
    }
}
