using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reflection;

namespace System.Reactive;

internal class ObservableQuery
{
	protected object? _source;

	protected Expression _expression;

	public object? Source => _source;

	public Expression Expression => _expression;

	public ObservableQuery(object source)
	{
		_source = source;
		_expression = System.Linq.Expressions.Expression.Constant(this);
	}

	public ObservableQuery(Expression expression)
	{
		_expression = expression;
	}
}
internal class ObservableQuery<TSource> : ObservableQuery, IQbservable<TSource>, IQbservable, IObservable<TSource>
{
	private class ObservableRewriter : ExpressionVisitor
	{
		private class Lazy<T>
		{
			private readonly Func<T> _factory;

			private T? _value;

			private bool _initialized;

			public T Value
			{
				get
				{
					lock (_factory)
					{
						if (!_initialized)
						{
							_value = _factory();
							_initialized = true;
						}
					}
					return _value;
				}
			}

			public Lazy(Func<T> factory)
			{
				_factory = factory;
			}
		}

		private static readonly Lazy<ILookup<string, MethodInfo>> ObservableMethods = new Lazy<ILookup<string, MethodInfo>>(() => GetMethods(typeof(Observable)));

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (node.Value is ObservableQuery observableQuery)
			{
				object source = observableQuery.Source;
				if (source != null)
				{
					return System.Linq.Expressions.Expression.Constant(source);
				}
				return Visit(observableQuery.Expression);
			}
			return node;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			MethodInfo method = node.Method;
			if (method.DeclaringType?.BaseType == typeof(QueryablePattern))
			{
				if (method.Name == "Then")
				{
					return System.Linq.Expressions.Expression.Call(Visit(node.Object), arguments: node.Arguments.Select((Expression arg) => Unquote(Visit(arg))).ToArray(), methodName: method.Name, typeArguments: method.GetGenericArguments());
				}
				if (method.Name == "And")
				{
					return System.Linq.Expressions.Expression.Call(Visit(node.Object), arguments: node.Arguments.Select((Expression arg) => Visit(arg)).ToArray(), methodName: method.Name, typeArguments: method.GetGenericArguments());
				}
			}
			else
			{
				IEnumerable<Expression> enumerable = node.Arguments.AsEnumerable();
				bool flag = false;
				ParameterInfo parameterInfo = method.GetParameters().FirstOrDefault();
				if (parameterInfo != null)
				{
					Type parameterType = parameterInfo.ParameterType;
					if (parameterType == typeof(IQbservableProvider))
					{
						flag = true;
						if (!(System.Linq.Expressions.Expression.Lambda<Func<IQbservableProvider>>(Visit(node.Arguments[0]), Array.Empty<ParameterExpression>()).Compile()() is ObservableQueryProvider))
						{
							return node;
						}
						enumerable = enumerable.Skip(1);
					}
					else if (typeof(IQbservable).IsAssignableFrom(parameterType))
					{
						flag = true;
					}
				}
				if (flag)
				{
					IList<Expression> arguments = VisitQbservableOperatorArguments(method, enumerable);
					return FindObservableMethod(method, arguments);
				}
			}
			return base.VisitMethodCall(node);
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return node;
		}

		private IList<Expression> VisitQbservableOperatorArguments(MethodInfo method, IEnumerable<Expression> arguments)
		{
			if (method.Name == "When")
			{
				Expression expression = arguments.Last();
				if (expression.NodeType == ExpressionType.NewArrayInit)
				{
					NewArrayExpression newArrayExpression = (NewArrayExpression)expression;
					List<Expression> list = new List<Expression>(1);
					list.Add(System.Linq.Expressions.Expression.NewArrayInit(typeof(Plan<>).MakeGenericType(method.GetGenericArguments()[0]), newArrayExpression.Expressions.Select((Expression param) => Visit(param))));
					return list;
				}
			}
			return arguments.Select((Expression arg) => Visit(arg)).ToList();
		}

		private static MethodCallExpression FindObservableMethod(MethodInfo method, IList<Expression> arguments)
		{
			Type type;
			ILookup<string, MethodInfo> lookup;
			if (method.DeclaringType == typeof(Qbservable))
			{
				type = typeof(Observable);
				lookup = ObservableMethods.Value;
			}
			else
			{
				type = method.DeclaringType;
				if (type.IsDefined(typeof(LocalQueryMethodImplementationTypeAttribute), inherit: false))
				{
					type = ((LocalQueryMethodImplementationTypeAttribute)type.GetCustomAttributes(typeof(LocalQueryMethodImplementationTypeAttribute), inherit: false)[0]).TargetType;
				}
				lookup = GetMethods(type);
			}
			Type[] typeArgs = (method.IsGenericMethod ? method.GetGenericArguments() : null);
			MethodInfo methodInfo = lookup[method.Name].FirstOrDefault((MethodInfo candidateMethod) => ArgsMatch(candidateMethod, arguments, typeArgs)) ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Providers.NO_MATCHING_METHOD_FOUND, method.Name, type.Name));
			if (typeArgs != null)
			{
				methodInfo = methodInfo.MakeGenericMethod(typeArgs);
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			int num = 0;
			for (int num2 = parameters.Length; num < num2; num++)
			{
				arguments[num] = Unquote(arguments[num]);
			}
			return System.Linq.Expressions.Expression.Call(null, methodInfo, arguments);
		}

		private static ILookup<string, MethodInfo> GetMethods(Type type)
		{
			return type.GetTypeInfo().DeclaredMethods.Where((MethodInfo m) => m.IsStatic && m.IsPublic).ToLookup((MethodInfo m) => m.Name);
		}

		private static bool ArgsMatch(MethodInfo method, IList<Expression> arguments, Type[]? typeArgs)
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters.Length != arguments.Count)
			{
				return false;
			}
			if (!method.IsGenericMethod && typeArgs != null && typeArgs.Length != 0)
			{
				return false;
			}
			if (method.IsGenericMethodDefinition)
			{
				if (typeArgs == null)
				{
					return false;
				}
				if (method.GetGenericArguments().Length != typeArgs.Length)
				{
					return false;
				}
				parameters = method.MakeGenericMethod(typeArgs).GetParameters();
			}
			int i = 0;
			for (int count = arguments.Count; i < count; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				Expression expression = arguments[i];
				if (!parameterType.IsAssignableFrom(expression.Type))
				{
					expression = Unquote(expression);
					if (!parameterType.IsAssignableFrom(expression.Type))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static Expression Unquote(Expression expression)
		{
			while (expression.NodeType == ExpressionType.Quote)
			{
				expression = ((UnaryExpression)expression).Operand;
			}
			return expression;
		}
	}

	public Type ElementType => typeof(TSource);

	public IQbservableProvider Provider => Qbservable.Provider;

	internal ObservableQuery(IObservable<TSource> source)
		: base(source)
	{
	}

	internal ObservableQuery(Expression expression)
		: base(expression)
	{
	}

	public IDisposable Subscribe(IObserver<TSource> observer)
	{
		if (_source == null)
		{
			Expression<Func<IObservable<TSource>>> expression = System.Linq.Expressions.Expression.Lambda<Func<IObservable<TSource>>>(new ObservableRewriter().Visit(_expression), Array.Empty<ParameterExpression>());
			_source = expression.Compile()();
		}
		return ((IObservable<TSource>)_source).Subscribe(observer);
	}

	public override string? ToString()
	{
		if (_expression is ConstantExpression constantExpression && constantExpression.Value == this)
		{
			if (_source != null)
			{
				return _source.ToString();
			}
			return "null";
		}
		return _expression.ToString();
	}
}
