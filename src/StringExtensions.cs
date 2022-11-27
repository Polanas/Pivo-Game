using System;

namespace Game;

static class StringExtensions
{

    public static LineSplitEumerator SplitLines(this string str) => new LineSplitEumerator(str);

    public ref struct LineSplitEumerator
    {

        public LineSplitEntry Current { get; private set; }

        private ReadOnlySpan<char> _str;

        public LineSplitEumerator(ReadOnlySpan<char> str)
        {
            _str = str;
            Current = default;
        }

        public LineSplitEumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_str.Length == 0)
                return false;

            var index = _str.IndexOfAny('\n', '\r');

            if (index == -1)
            {
                Current = new LineSplitEntry(_str, ReadOnlySpan<char>.Empty);
                _str = ReadOnlySpan<char>.Empty;
                return true;
            }

            if (index < _str.Length - 1 && _str[index] == '\r' && _str[index + 1] == '\n')
            {
                Current = new LineSplitEntry(_str.Slice(0, index), _str.Slice(index, 2));
                _str = _str.Slice(index + 2);
                return true;
            }

            Current = new LineSplitEntry(_str.Slice(0, index), _str.Slice(index, 1));
            _str = _str.Slice(index + 1);
            return true;
        }
    }

    public ref struct LineSplitEntry
    {

        public ReadOnlySpan<char> Line { get; }

        public ReadOnlySpan<char> Separator { get; }

        public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
        {
            Line = line;
            Separator = separator;
        }

        public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
    }
}
