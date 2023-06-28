using System;

namespace Markdown.Avalonia.SyntaxHigh
{
    public class Alias
    {
        public string? Name { get; set; }

        private string? _realName;
        private Uri? _xshd;


        public string? RealName
        {
            get => _realName;
            set
            {
                _realName = value;
                Validation(nameof(RealName));
            }
        }
        public Uri? XSHD
        {
            get => _xshd;
            set
            {
                _xshd = value;
                Validation(nameof(XSHD));
            }
        }


        private void Validation(string name)
        {
            if (_realName != null && _xshd != null)
            {
                throw new ArgumentException(name);
            }
        }
    }
}
