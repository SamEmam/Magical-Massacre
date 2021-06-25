﻿using System;

namespace HeathenEngineering.Scriptable
{
    [Serializable]
    public class IntReference : VariableReference<int>
    {
        public IntVariable Variable;

        public override IDataVariable<int> m_variable => Variable;

        public IntReference(int value) : base(value)
        { }
    }
}
