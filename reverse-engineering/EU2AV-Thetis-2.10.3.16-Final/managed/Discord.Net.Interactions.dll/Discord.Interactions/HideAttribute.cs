using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class HideAttribute : Attribute
{
}
