using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DontAutoRegisterAttribute : Attribute
{
}
