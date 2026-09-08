using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.DependencyInjection;

public static class ActivatorUtilities
{
	private readonly struct FactoryParameterContext(Type parameterType, bool hasDefaultValue, object defaultValue, int argumentIndex, object serviceKey)
	{
		public Type ParameterType { get; } = parameterType;

		public bool HasDefaultValue { get; } = hasDefaultValue;

		public object DefaultValue { get; } = defaultValue;

		public int ArgumentIndex { get; } = argumentIndex;

		public object ServiceKey { get; } = serviceKey;
	}

	private sealed class ConstructorInfoEx
	{
		public readonly ConstructorInfo Info;

		public readonly ParameterInfo[] Parameters;

		public readonly bool IsPreferred;

		private readonly object[] _parameterKeys;

		public ConstructorInfoEx(ConstructorInfo constructor)
		{
			Info = constructor;
			Parameters = constructor.GetParameters();
			IsPreferred = constructor.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), inherit: false);
			for (int i = 0; i < Parameters.Length; i++)
			{
				FromKeyedServicesAttribute fromKeyedServicesAttribute = (FromKeyedServicesAttribute)Attribute.GetCustomAttribute(Parameters[i], typeof(FromKeyedServicesAttribute), inherit: false);
				if (fromKeyedServicesAttribute != null)
				{
					if (_parameterKeys == null)
					{
						_parameterKeys = new object[Parameters.Length];
					}
					_parameterKeys[i] = fromKeyedServicesAttribute.Key;
				}
			}
		}

		public bool IsService(IServiceProviderIsService serviceProviderIsService, int parameterIndex)
		{
			ParameterInfo parameterInfo = Parameters[parameterIndex];
			object[] parameterKeys = _parameterKeys;
			object obj = ((parameterKeys != null) ? parameterKeys[parameterIndex] : null);
			if (obj != null)
			{
				if (serviceProviderIsService is IServiceProviderIsKeyedService serviceProviderIsKeyedService)
				{
					return serviceProviderIsKeyedService.IsKeyedService(parameterInfo.ParameterType, obj);
				}
				throw new InvalidOperationException(System.SR.KeyedServicesNotSupported);
			}
			return serviceProviderIsService.IsService(parameterInfo.ParameterType);
		}

		public object GetService(IServiceProvider serviceProvider, int parameterIndex)
		{
			ParameterInfo parameterInfo = Parameters[parameterIndex];
			object[] parameterKeys = _parameterKeys;
			object obj = ((parameterKeys != null) ? parameterKeys[parameterIndex] : null);
			if (obj != null)
			{
				if (serviceProvider is IKeyedServiceProvider keyedServiceProvider)
				{
					return keyedServiceProvider.GetKeyedService(parameterInfo.ParameterType, obj);
				}
				throw new InvalidOperationException(System.SR.KeyedServicesNotSupported);
			}
			return serviceProvider.GetService(parameterInfo.ParameterType);
		}
	}

	private readonly ref struct ConstructorMatcher(ConstructorInfoEx constructor, object[] parameterValues)
	{
		private readonly ConstructorInfoEx _constructor = constructor;

		private readonly object[] _parameterValues = parameterValues;

		public ConstructorInfoEx ConstructorInfo => _constructor;

		public int Match(object[] givenParameters, IServiceProviderIsService serviceProviderIsService)
		{
			for (int i = 0; i < givenParameters.Length; i++)
			{
				Type c = givenParameters[i]?.GetType();
				bool flag = false;
				for (int j = 0; j < _constructor.Parameters.Length; j++)
				{
					if (_parameterValues[j] == null && _constructor.Parameters[j].ParameterType.IsAssignableFrom(c))
					{
						flag = true;
						_parameterValues[j] = givenParameters[i];
						break;
					}
				}
				if (!flag)
				{
					return -1;
				}
			}
			for (int k = 0; k < _constructor.Parameters.Length; k++)
			{
				if (_parameterValues[k] == null && !_constructor.IsService(serviceProviderIsService, k))
				{
					if (!ParameterDefaultValue.TryGetDefaultValue(_constructor.Parameters[k], out object defaultValue))
					{
						return -1;
					}
					_parameterValues[k] = defaultValue;
				}
			}
			return _constructor.Parameters.Length;
		}

		public object CreateInstance(IServiceProvider provider)
		{
			for (int i = 0; i < _constructor.Parameters.Length; i++)
			{
				if (_parameterValues[i] != null)
				{
					continue;
				}
				object service = _constructor.GetService(provider, i);
				if (service == null)
				{
					if (!ParameterDefaultValue.TryGetDefaultValue(_constructor.Parameters[i], out object defaultValue))
					{
						throw new InvalidOperationException(System.SR.Format(System.SR.UnableToResolveService, _constructor.Parameters[i].ParameterType, _constructor.Info.DeclaringType));
					}
					_parameterValues[i] = defaultValue;
				}
				else
				{
					_parameterValues[i] = service;
				}
			}
			try
			{
				return _constructor.Info.Invoke(_parameterValues);
			}
			catch (TargetInvocationException ex) when (ex.InnerException != null)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;
			}
		}

		public void MapParameters(int?[] parameterMap, object[] givenParameters)
		{
			for (int i = 0; i < _constructor.Parameters.Length; i++)
			{
				if (parameterMap[i].HasValue)
				{
					_parameterValues[i] = givenParameters[parameterMap[i].Value];
				}
			}
		}
	}

	private static readonly MethodInfo GetServiceInfo = new Func<IServiceProvider, Type, Type, bool, object, object>(GetService).Method;

	public static object CreateInstance(IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, params object[] parameters)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		if (instanceType.IsAbstract)
		{
			throw new InvalidOperationException(System.SR.CannotCreateAbstractClasses);
		}
		ConstructorInfoEx[] array = CreateConstructorInfoExs(instanceType);
		object[] ctorArgs = null;
		object[] array2 = null;
		ConstructorMatcher constructorMatcher = default(ConstructorMatcher);
		IServiceProviderIsService service = provider.GetService<IServiceProviderIsService>();
		ConstructorInfoEx constructorInfoEx;
		if (service != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				constructorInfoEx = array[i];
				if (!constructorInfoEx.IsPreferred)
				{
					continue;
				}
				for (int j = i + 1; j < array.Length; j++)
				{
					if (array[j].IsPreferred)
					{
						ThrowMultipleCtorsMarkedWithAttributeException();
					}
				}
				InitializeCtorArgValues(ref ctorArgs, constructorInfoEx.Parameters.Length);
				constructorMatcher = new ConstructorMatcher(constructorInfoEx, ctorArgs);
				if (constructorMatcher.Match(parameters, service) == -1)
				{
					ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
				}
				return constructorMatcher.CreateInstance(provider);
			}
			int num = -1;
			ConstructorMatcher constructorMatcher2 = default(ConstructorMatcher);
			bool flag = false;
			for (int k = 0; k < array.Length; k++)
			{
				constructorInfoEx = array[k];
				InitializeCtorArgValues(ref ctorArgs, constructorInfoEx.Parameters.Length);
				constructorMatcher = new ConstructorMatcher(constructorInfoEx, ctorArgs);
				int num2 = constructorMatcher.Match(parameters, service);
				if (num < num2)
				{
					num = num2;
					if (k == array.Length - 1)
					{
						array2 = ctorArgs;
					}
					else
					{
						array2 = new object[num2];
						ctorArgs.CopyTo(array2, 0);
					}
					constructorMatcher2 = new ConstructorMatcher(constructorMatcher.ConstructorInfo, array2);
					flag = false;
				}
				else if (num == num2)
				{
					flag = true;
				}
			}
			if (num != -1)
			{
				if (flag)
				{
					throw new InvalidOperationException(System.SR.Format(System.SR.MultipleCtorsFoundWithBestLength, instanceType, num));
				}
				return constructorMatcher2.CreateInstance(provider);
			}
		}
		Type[] array3;
		if (parameters.Length == 0)
		{
			array3 = Type.EmptyTypes;
		}
		else
		{
			array3 = new Type[parameters.Length];
			for (int l = 0; l < array3.Length; l++)
			{
				array3[l] = parameters[l]?.GetType();
			}
		}
		FindApplicableConstructor(instanceType, array3, array, out var matchingConstructor, out var matchingParameterMap);
		constructorInfoEx = FindConstructorEx(matchingConstructor, array);
		InitializeCtorArgValues(ref ctorArgs, constructorInfoEx.Parameters.Length);
		constructorMatcher = new ConstructorMatcher(constructorInfoEx, ctorArgs);
		constructorMatcher.MapParameters(matchingParameterMap, parameters);
		return constructorMatcher.CreateInstance(provider);
		static void InitializeCtorArgValues(ref object[] reference, int length)
		{
			if (reference != null && reference.Length == length)
			{
				Array.Clear(reference, 0, length);
			}
			else
			{
				reference = new object[length];
			}
		}
	}

	private static ConstructorInfoEx[] CreateConstructorInfoExs([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
	{
		ConstructorInfo[] constructors = type.GetConstructors();
		ConstructorInfoEx[] array = new ConstructorInfoEx[constructors.Length];
		for (int i = 0; i < constructors.Length; i++)
		{
			array[i] = new ConstructorInfoEx(constructors[i]);
		}
		return array;
	}

	public static ObjectFactory CreateFactory([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes)
	{
		CreateFactoryInternal(instanceType, argumentTypes, out var provider, out var argumentArray, out var factoryExpressionBody);
		return Expression.Lambda<ObjectFactory>(factoryExpressionBody, new ParameterExpression[2] { provider, argumentArray }).Compile();
	}

	public static ObjectFactory<T> CreateFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(Type[] argumentTypes)
	{
		CreateFactoryInternal(typeof(T), argumentTypes, out var provider, out var argumentArray, out var factoryExpressionBody);
		return Expression.Lambda<ObjectFactory<T>>(factoryExpressionBody, new ParameterExpression[2] { provider, argumentArray }).Compile();
	}

	private static void CreateFactoryInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, out ParameterExpression provider, out ParameterExpression argumentArray, out Expression factoryExpressionBody)
	{
		FindApplicableConstructor(instanceType, argumentTypes, null, out var matchingConstructor, out var matchingParameterMap);
		provider = Expression.Parameter(typeof(IServiceProvider), "provider");
		argumentArray = Expression.Parameter(typeof(object[]), "argumentArray");
		factoryExpressionBody = BuildFactoryExpression(matchingConstructor, matchingParameterMap, provider, argumentArray);
	}

	public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider, params object[] parameters)
	{
		return (T)CreateInstance(provider, typeof(T), parameters);
	}

	public static T GetServiceOrCreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider)
	{
		return (T)GetServiceOrCreateInstance(provider, typeof(T));
	}

	public static object GetServiceOrCreateInstance(IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
	{
		return provider.GetService(type) ?? CreateInstance(provider, type);
	}

	private static object GetService(IServiceProvider sp, Type type, Type requiredBy, bool hasDefaultValue, object key)
	{
		object obj = ((key == null) ? sp.GetService(type) : GetKeyedService(sp, type, key));
		if (obj == null && !hasDefaultValue)
		{
			ThrowHelperUnableToResolveService(type, requiredBy);
		}
		return obj;
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	private static void ThrowHelperUnableToResolveService(Type type, Type requiredBy)
	{
		throw new InvalidOperationException(System.SR.Format(System.SR.UnableToResolveService, type, requiredBy));
	}

	private static BlockExpression BuildFactoryExpression(ConstructorInfo constructor, int?[] parameterMap, Expression serviceProvider, Expression factoryArgumentArray)
	{
		ParameterInfo[] parameters = constructor.GetParameters();
		Expression[] array = new Expression[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type parameterType = parameterInfo.ParameterType;
			bool flag = ParameterDefaultValue.TryGetDefaultValue(parameterInfo, out object defaultValue);
			if (parameterMap[i].HasValue)
			{
				array[i] = Expression.ArrayAccess(factoryArgumentArray, Expression.Constant(parameterMap[i]));
			}
			else
			{
				FromKeyedServicesAttribute fromKeyedServicesAttribute = (FromKeyedServicesAttribute)Attribute.GetCustomAttribute(parameterInfo, typeof(FromKeyedServicesAttribute), inherit: false);
				Expression[] arguments = new Expression[5]
				{
					serviceProvider,
					Expression.Constant(parameterType, typeof(Type)),
					Expression.Constant(constructor.DeclaringType, typeof(Type)),
					Expression.Constant(flag),
					Expression.Constant(fromKeyedServicesAttribute?.Key, typeof(object))
				};
				array[i] = Expression.Call(GetServiceInfo, arguments);
			}
			if (flag)
			{
				ConstantExpression right = Expression.Constant(defaultValue);
				array[i] = Expression.Coalesce(array[i], right);
			}
			array[i] = Expression.Convert(array[i], parameterType);
		}
		return Expression.Block(Expression.IfThen(Expression.Equal(serviceProvider, Expression.Constant(null)), Expression.Throw(Expression.Constant(new ArgumentNullException("serviceProvider")))), Expression.New(constructor, array));
	}

	private static void FindApplicableConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, ConstructorInfoEx[] constructors, out ConstructorInfo matchingConstructor, out int?[] matchingParameterMap)
	{
		if (!TryFindPreferredConstructor(instanceType, argumentTypes, constructors, out var matchingConstructor2, out var parameterMap) && !TryFindMatchingConstructor(instanceType, argumentTypes, out matchingConstructor2, out parameterMap))
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.CtorNotLocated, instanceType));
		}
		matchingConstructor = matchingConstructor2;
		matchingParameterMap = parameterMap;
	}

	private static ConstructorInfoEx FindConstructorEx(ConstructorInfo constructorInfo, ConstructorInfoEx[] constructorExs)
	{
		for (int i = 0; i < constructorExs.Length; i++)
		{
			if ((object)constructorExs[i].Info == constructorInfo)
			{
				return constructorExs[i];
			}
		}
		return null;
	}

	private static bool TryFindMatchingConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ConstructorInfo matchingConstructor, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out int?[] parameterMap)
	{
		matchingConstructor = null;
		parameterMap = null;
		ConstructorInfo[] constructors = instanceType.GetConstructors();
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			if (TryCreateParameterMap(constructorInfo.GetParameters(), argumentTypes, out var parameterMap2))
			{
				if (matchingConstructor != null)
				{
					throw new InvalidOperationException(System.SR.Format(System.SR.MultipleCtorsFound, instanceType));
				}
				matchingConstructor = constructorInfo;
				parameterMap = parameterMap2;
			}
		}
		if (matchingConstructor != null)
		{
			return true;
		}
		return false;
	}

	private static bool TryFindPreferredConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, ConstructorInfoEx[] constructors, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ConstructorInfo matchingConstructor, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out int?[] parameterMap)
	{
		bool flag = false;
		matchingConstructor = null;
		parameterMap = null;
		if (constructors == null)
		{
			constructors = CreateConstructorInfoExs(instanceType);
		}
		ConstructorInfoEx[] array = constructors;
		foreach (ConstructorInfoEx constructorInfoEx in array)
		{
			if (constructorInfoEx.IsPreferred)
			{
				if (flag)
				{
					ThrowMultipleCtorsMarkedWithAttributeException();
				}
				if (!TryCreateParameterMap(constructorInfoEx.Info.GetParameters(), argumentTypes, out var parameterMap2))
				{
					ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
				}
				matchingConstructor = constructorInfoEx.Info;
				parameterMap = parameterMap2;
				flag = true;
			}
		}
		if (matchingConstructor != null)
		{
			return true;
		}
		return false;
	}

	private static bool TryCreateParameterMap(ParameterInfo[] constructorParameters, Type[] argumentTypes, out int?[] parameterMap)
	{
		parameterMap = new int?[constructorParameters.Length];
		for (int i = 0; i < argumentTypes.Length; i++)
		{
			bool flag = false;
			Type c = argumentTypes[i];
			for (int j = 0; j < constructorParameters.Length; j++)
			{
				if (!parameterMap[j].HasValue && constructorParameters[j].ParameterType.IsAssignableFrom(c))
				{
					flag = true;
					parameterMap[j] = i;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private static void ThrowMultipleCtorsMarkedWithAttributeException()
	{
		throw new InvalidOperationException(System.SR.Format(System.SR.MultipleCtorsMarkedWithAttribute, "ActivatorUtilitiesConstructorAttribute"));
	}

	private static void ThrowMarkedCtorDoesNotTakeAllProvidedArguments()
	{
		throw new InvalidOperationException(System.SR.Format(System.SR.MarkedCtorMissingArgumentTypes, "ActivatorUtilitiesConstructorAttribute"));
	}

	private static object GetKeyedService(IServiceProvider provider, Type type, object serviceKey)
	{
		System.ExceptionPolyfills.ThrowIfNull(provider, "provider");
		if (provider is IKeyedServiceProvider keyedServiceProvider)
		{
			return keyedServiceProvider.GetKeyedService(type, serviceKey);
		}
		throw new InvalidOperationException(System.SR.KeyedServicesNotSupported);
	}
}
