using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class ReadOnlyAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class ReadOnlyRangeAttribute : PropertyAttribute
{
    public readonly float min;
    public readonly float max;

    public ReadOnlyRangeAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

