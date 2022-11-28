using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Builder
{
    internal sealed class StringConstraint
    {
        public static readonly StringConstraint Name = new StringConstraint(
            pattern: @"^[a-z][a-z0-9_\-]*$",
            ignoreCase: true,
            minLength: 1,
            maxLength: 256);

        private readonly string pattern;

        private readonly int minLength;

        private readonly int maxLength;

        private readonly Regex regex;

        public StringConstraint(string pattern, bool ignoreCase, int minLength, int maxLength)
        {
            this.pattern = pattern;
            this.minLength = minLength;
            this.maxLength = maxLength;

            var options = ignoreCase ? RegexOptions.IgnoreCase | RegexOptions.Compiled : RegexOptions.Compiled;
            this.regex = new Regex(pattern, options);
        }

        public bool IsValid(string input, out string reason)
        {
            if (input == null)
            {
                reason = "is null";
                return false;
            }

            if (input.Length < this.minLength)
            {
                reason = $"length must be greater than or equal to {this.minLength}";
                return false;
            }

            if (input.Length > this.maxLength)
            {
                reason = $"length must be less than or equal to {this.maxLength}";
                return false;
            }

            if (this.regex.IsMatch(input))
            {
                reason = $"doesn't match pattern \"{this.pattern}\"";
                return false;
            }

            reason = String.Empty;
            return true;
        }
    }
}
