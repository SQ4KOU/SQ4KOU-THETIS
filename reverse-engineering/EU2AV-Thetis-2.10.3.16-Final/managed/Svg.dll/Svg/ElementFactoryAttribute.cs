using System;

namespace Svg;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ElementFactoryAttribute : Attribute
{
}
