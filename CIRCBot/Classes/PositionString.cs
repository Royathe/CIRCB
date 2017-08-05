using System;

namespace CIRCBot
{
    public class PositionString
    {
        private string text { get; set; }

        private int position { get; set; }

        public string SectionSplit { get; set; }

        public PositionString(int length = 1000)
        {
            text = "";
            increaseLength(length);
            SectionSplit = " | ";
        }

        private void increaseLength(int length)
        {
            for (int i = 0; i < length; i++)
            {
                text += " ";
            }
        }

        public PositionString Position(int sectionLength, string insertText)
        {
            if(insertText.Length > sectionLength)
            {
                insertText = insertText.Substring(0, sectionLength);
            }
            if(position > text.Length)
            {
                increaseLength(sectionLength);
            }
            text = text.Insert(position, insertText);
            position += sectionLength;
            return this;
        }

        public PositionString Position(int toNextPosition, int sectionLength, string insertText)
        {
            position += toNextPosition;
            Position(sectionLength, insertText);
            return this;
        }

        public PositionString Position(string insertText)
        {
            Position(insertText.Length, insertText);
            return this;
        }

        public PositionString Insert(int startIndex, string insertText)
        {
            position = startIndex;
            Position(insertText);
            return this;
        }
        
        public PositionString Section(int sectionLength, string insertText = "")
        {
            Position(sectionLength, SectionSplit + insertText);
            return this;
        }

        public PositionString Section(string insertText = "")
        {
            insertText = SectionSplit + insertText;
            Position(insertText.Length, insertText);
            return this;
        }

        public PositionString SectionInsert(int startIndex, string insertText = "")
        {
            position = startIndex;
            Section(insertText);
            return this;
        }

        public PositionString Colorize(MircColor color, int offset = 0)
        {
            int index = text.LastIndexOf('|') + offset;
            text = text.Insert(index, color);
            return this;
        }

        public PositionString Colorize(string blockColor, string defaultColor)
        {
            int index = text.LastIndexOf('|') + 1;
            text = text.Insert(index, blockColor);
            text = text.Insert(Text.Length, defaultColor);//Text + defaultColor;
            return this;
        }

        public string Text
        {
            get
            {
                return text.TrimEnd(' ');
            }
        }

        public string Get(int length)
        {
            return text.Substring(0, length);
        }

        public static PositionString Empty
        {
            get
            {
                return new PositionString();
            }
        }
        
        public static implicit operator String(PositionString ps)
        {
            return ps.Text;
        }

        public static implicit operator PositionString(string s)
        {
            PositionString ps = new PositionString();
            ps.Position(s);
            return ps;
        }

    }
}
