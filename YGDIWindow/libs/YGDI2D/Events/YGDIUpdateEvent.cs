﻿using System;

namespace YGDIWindow_2D.YGDI2D.Events
{
    public class YGDIUpdateEvent : EventArgs
    {
        public YGDIContext Context { get; private set; }
        public double DeltaTime { get; private set; }

        public YGDIUpdateEvent(YGDIContext context, double deltaTime)
        {
            // set ygdi context and deltatime
            Context = context;
            DeltaTime = deltaTime;
        }
    }
}
