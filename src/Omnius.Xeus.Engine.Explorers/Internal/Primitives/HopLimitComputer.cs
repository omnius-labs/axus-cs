using System;
using System.Collections.Generic;
using System.Text;
using Omnius.Core;

namespace Xeus.Engine.Internal.Search.Primitives
{
    internal sealed class HopLimitComputer
    {
        public HopLimitComputer()
        {

        }

        public int MaxHopLimit { get; set; }
        public bool DecrementAtHopLimitMaximum { get; set; }
        public bool DecrementAtHopLimitMinimum { get; set; }

        public void Decrement(ref int current)
        {
            if (current > this.MaxHopLimit)
            {
                current = this.MaxHopLimit;
            }

            if (current <= 0)
            {
                current = 0;
            }

            if (current == this.MaxHopLimit)
            {
                if (this.DecrementAtHopLimitMaximum)
                {
                    current--;
                }

                return;
            }

            if (current == 1)
            {
                if (this.DecrementAtHopLimitMinimum)
                {
                    current--;
                }

                return;
            }

            current--;
        }
    }
}
