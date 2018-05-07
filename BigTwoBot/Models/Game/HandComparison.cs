using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class HandComparison
    {
        public bool Success { get; set; } = false;
        public HandComparisonFailReason? Reason { get; set; } = null;

        public HandComparison(bool success = false, HandComparisonFailReason? reason = null)
        {
            Success = success;
            Reason = reason;
        }
    }

    public enum HandComparisonFailReason
    {
        Unknown = 0,
        HandCountDifferent = 1,
        ValueSmaller = 2,
        TypeSmaller = 4,
        StraightMaxSmaller = 8,
        FullHouseTripleSmaller = 16,
        QuadrupleSmaller = 32,
        StraightFlushMaxSmaller = 64,
        FlushMaxSmaller = 128,
    }
}
