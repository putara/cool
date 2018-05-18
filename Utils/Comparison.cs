using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool
{
    public static class Comparison
    {
        public static int CompareFalseFirst(bool x, bool y)
        {
            return Compare(x ? 1 : 0, y ? 1 : 0);
        }

        public static int CompareTrueFirst(bool x, bool y)
        {
            return Compare(x ? 0 : 1, y ? 0 : 1);
        }

        public static int Compare(int x, int y)
        {
            return Math.Sign(x - y);
        }

        public static int Compare(long x, long y)
        {
            return Math.Sign(x - y);
        }

        public static int Compare(float x, float y)
        {
            return Math.Sign(x - y);
        }

        public static int Compare(double x, double y)
        {
            return Math.Sign(x - y);
        }

        public static int Compare(decimal x, decimal y)
        {
            return Math.Sign(x - y);
        }

        public static int Compare(DateTime x, DateTime y)
        {
            return DateTime.Compare(x, y);
        }

        public static int Compare(string x, string y)
        {
            return string.Compare(x, y);
        }

        public static int Compare(string x, string y, bool ignoreCase)
        {
            return string.Compare(x, y, ignoreCase);
        }

        public static int CompareOrdinal(string x, string y)
        {
            return string.CompareOrdinal(x, y);
        }
    }

    // yes, this is inspired by Guava.
    public class ComparisonChain
    {
        int currentResult;

        public int Result
        {
            get { return this.currentResult; }
        }

        public ComparisonChain CompareFalseFirst(bool x, bool y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.CompareFalseFirst(x, y);
            }
            return this;
        }

        public ComparisonChain CompareTrueFirst(bool x, bool y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.CompareTrueFirst(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(int x, int y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(long x, long y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(float x, float y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(double x, double y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(DateTime x, DateTime y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(string x, string y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y);
            }
            return this;
        }

        public ComparisonChain Compare(string x, string y, bool ignoreCase)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.Compare(x, y, ignoreCase);
            }
            return this;
        }

        public ComparisonChain CompareOrdinal(string x, string y)
        {
            if (this.currentResult == 0)
            {
                this.currentResult = Comparison.CompareOrdinal(x, y);
            }
            return this;
        }
    }
}
