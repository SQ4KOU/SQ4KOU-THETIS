using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
public class ComplexParameterCtorAttribute : Attribute
{
}
