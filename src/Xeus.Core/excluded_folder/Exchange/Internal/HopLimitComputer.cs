using System;
using System.Collections.Generic;
using System.Text;
using Omnix.Base;

namespace Xeus.Core.Exchange.Internal
{
    internal class HopLimitComputer
    {
        public HopLimitComputer()
        {

        }

        public int MaxHopLimit { get; set; }

        public int Decrement(int current, bool decrementAtHopLimitMaximum, bool decrementAtHopLimitMinimum)
        {
            if (current > this.MaxHopLimit)
            {
                current = this.MaxHopLimit;
            }

            if (current <= 0)
            {
                return 0;
            }

            if (current == this.MaxHopLimit)
            {
                if (decrementAtHopLimitMaximum)
                {
                    return current - 1;
                }

                return current;
            }

            if (current == 1)
            {
                if (decrementAtHopLimitMinimum)
                {
                    return current - 1;
                }

                return current;
            }

            return current - 1;
        }
    }
}
