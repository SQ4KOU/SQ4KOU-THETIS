using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal abstract class CommonObjectFormatter : ObjectFormatter
{
	private sealed class Builder
	{
		private readonly StringBuilder _sb;

		private readonly bool _suppressEllipsis;

		private readonly BuilderOptions _options;

		private int _currentLimit;

		public int Remaining => _options.MaximumOutputLength - _sb.Length;

		private int CurrentRemaining => _currentLimit - _sb.Length;

		public Builder(BuilderOptions options, bool suppressEllipsis)
		{
			_sb = new StringBuilder();
			_suppressEllipsis = suppressEllipsis;
			_options = options;
			_currentLimit = Math.Min(_options.MaximumLineLength, _options.MaximumOutputLength);
		}

		public void AppendLine()
		{
			_currentLimit = _options.MaximumOutputLength;
			Append(_options.NewLine);
			_currentLimit = (int)Math.Min((long)_sb.Length + (long)_options.MaximumLineLength, _options.MaximumOutputLength);
		}

		private void AppendEllipsis()
		{
			if (!_suppressEllipsis)
			{
				string ellipsis = _options.Ellipsis;
				if (!string.IsNullOrEmpty(ellipsis))
				{
					_sb.Append(ellipsis);
				}
			}
		}

		public void Append(char c, int count = 1)
		{
			if (CurrentRemaining >= 0)
			{
				int num = Math.Min(count, CurrentRemaining);
				_sb.Append(c, num);
				if (!_suppressEllipsis && num < count)
				{
					AppendEllipsis();
				}
			}
		}

		public void Append(string str, int start = 0, int count = int.MaxValue)
		{
			if (str != null && CurrentRemaining >= 0)
			{
				count = Math.Min(count, str.Length - start);
				int num = Math.Min(count, CurrentRemaining);
				_sb.Append(str, start, num);
				if (!_suppressEllipsis && num < count)
				{
					AppendEllipsis();
				}
			}
		}

		public void AppendFormat(string format, params object[] args)
		{
			Append(string.Format(format, args));
		}

		public void AppendGroupOpening()
		{
			Append('{');
		}

		public void AppendGroupClosing(bool inline)
		{
			if (inline)
			{
				Append(" }");
				return;
			}
			AppendLine();
			Append('}');
			AppendLine();
		}

		public void AppendCollectionItemSeparator(bool isFirst, bool inline)
		{
			if (isFirst)
			{
				if (inline)
				{
					Append(' ');
				}
				else
				{
					AppendLine();
				}
			}
			else if (inline)
			{
				Append(", ");
			}
			else
			{
				Append(',');
				AppendLine();
			}
			if (!inline)
			{
				Append(_options.Indentation);
			}
		}

		internal void AppendInfiniteRecursionMarker()
		{
			AppendGroupOpening();
			AppendCollectionItemSeparator(isFirst: true, inline: true);
			Append("...");
			AppendGroupClosing(inline: true);
		}

		public override string ToString()
		{
			return _sb.ToString();
		}
	}

	internal readonly struct BuilderOptions(string indentation, string newLine, string ellipsis, int maximumLineLength, int maximumOutputLength)
	{
		public readonly string Indentation = indentation;

		public readonly string NewLine = newLine;

		public readonly string Ellipsis = ellipsis;

		public readonly int MaximumLineLength = maximumLineLength;

		public readonly int MaximumOutputLength = maximumOutputLength;

		public BuilderOptions WithMaximumOutputLength(int maximumOutputLength)
		{
			return new BuilderOptions(Indentation, NewLine, Ellipsis, MaximumLineLength, maximumOutputLength);
		}
	}

	private sealed class Visitor
	{
		private readonly struct FormattedMember(int index, string name, string value)
		{
			public readonly int Index = index;

			public readonly string Name = name;

			public readonly string Value = value;

			public int MinimalLength => ((Name != null) ? Name.Length : "[0]".Length) + Value.Length;

			public string GetDisplayName()
			{
				string text = Name;
				if (text == null)
				{
					int index = Index;
					text = "[" + index + "]";
				}
				return text;
			}

			public bool HasKeyName()
			{
				if (Index >= 0 && Name != null && Name.Length >= 2 && Name[0] == '[')
				{
					string name = Name;
					return name[name.Length - 1] == ']';
				}
				return false;
			}

			public bool AppendAsCollectionEntry(Builder result)
			{
				if (HasKeyName())
				{
					result.AppendGroupOpening();
					result.AppendCollectionItemSeparator(isFirst: true, inline: true);
					result.Append(Name, 1, Name.Length - 2);
					result.AppendCollectionItemSeparator(isFirst: false, inline: true);
					result.Append(Value);
					result.AppendGroupClosing(inline: true);
				}
				else
				{
					result.Append(Value);
				}
				return true;
			}

			public bool Append(Builder result, string separator)
			{
				result.Append(GetDisplayName());
				result.Append(separator);
				result.Append(Value);
				return true;
			}
		}

		private readonly CommonObjectFormatter _formatter;

		private readonly BuilderOptions _builderOptions;

		private CommonPrimitiveFormatterOptions _primitiveOptions;

		private readonly CommonTypeNameFormatterOptions _typeNameOptions;

		private MemberDisplayFormat _memberDisplayFormat;

		private HashSet<object> _lazyVisitedObjects;

		private HashSet<object> VisitedObjects
		{
			get
			{
				if (_lazyVisitedObjects == null)
				{
					_lazyVisitedObjects = new HashSet<object>(ReferenceEqualityComparer.Instance);
				}
				return _lazyVisitedObjects;
			}
		}

		public Visitor(CommonObjectFormatter formatter, BuilderOptions builderOptions, CommonPrimitiveFormatterOptions primitiveOptions, CommonTypeNameFormatterOptions typeNameOptions, MemberDisplayFormat memberDisplayFormat)
		{
			_formatter = formatter;
			_builderOptions = builderOptions;
			_primitiveOptions = primitiveOptions;
			_typeNameOptions = typeNameOptions;
			_memberDisplayFormat = memberDisplayFormat;
		}

		private Builder MakeMemberBuilder(int limit)
		{
			return new Builder(_builderOptions.WithMaximumOutputLength(Math.Min(_builderOptions.MaximumLineLength, limit)), suppressEllipsis: true);
		}

		public string FormatObject(object obj)
		{
			try
			{
				Builder result = new Builder(_builderOptions, suppressEllipsis: false);
				string debuggerDisplayName;
				return FormatObjectRecursive(result, obj, isRoot: true, out debuggerDisplayName).ToString();
			}
			catch (InsufficientExecutionStackException)
			{
				return ScriptingResources.StackOverflowWhileEvaluating;
			}
		}

		private Builder FormatObjectRecursive(Builder result, object obj, bool isRoot, out string debuggerDisplayName)
		{
			if (!isRoot && _memberDisplayFormat == MemberDisplayFormat.SeparateLines)
			{
				_memberDisplayFormat = MemberDisplayFormat.SingleLine;
			}
			debuggerDisplayName = null;
			string text = _formatter.PrimitiveFormatter.FormatPrimitive(obj, _primitiveOptions);
			if (text != null)
			{
				result.Append(text);
				return result;
			}
			Type type = obj.GetType();
			System.Reflection.TypeInfo typeInfo = type.GetTypeInfo();
			if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(KeyValuePair<, >))
			{
				if (isRoot)
				{
					result.Append(_formatter.TypeNameFormatter.FormatTypeName(type, _typeNameOptions));
					result.Append(' ');
				}
				FormatKeyValuePair(result, obj);
				return result;
			}
			if (typeInfo.IsArray)
			{
				if (VisitedObjects.Add(obj))
				{
					FormatArray(result, (Array)obj);
					VisitedObjects.Remove(obj);
				}
				else
				{
					result.AppendInfiniteRecursionMarker();
				}
				return result;
			}
			DebuggerDisplayAttribute applicableDebuggerDisplayAttribute = ObjectFormatterHelpers.GetApplicableDebuggerDisplayAttribute(typeInfo);
			if (applicableDebuggerDisplayAttribute != null)
			{
				debuggerDisplayName = applicableDebuggerDisplayAttribute.Name;
			}
			bool flag = false;
			ICollection collection;
			if ((collection = obj as ICollection) != null)
			{
				FormatCollectionHeader(result, collection);
			}
			else if (applicableDebuggerDisplayAttribute != null && !string.IsNullOrEmpty(applicableDebuggerDisplayAttribute.Value))
			{
				if (isRoot)
				{
					result.Append(_formatter.TypeNameFormatter.FormatTypeName(type, _typeNameOptions));
					result.Append('(');
				}
				FormatWithEmbeddedExpressions(result, applicableDebuggerDisplayAttribute.Value, obj);
				if (isRoot)
				{
					result.Append(')');
				}
				flag = true;
			}
			else if (ObjectFormatterHelpers.HasOverriddenToString(typeInfo))
			{
				ObjectToString(result, obj);
				flag = true;
			}
			else
			{
				result.Append(_formatter.TypeNameFormatter.FormatTypeName(type, _typeNameOptions));
			}
			MemberDisplayFormat memberDisplayFormat = _memberDisplayFormat;
			if (memberDisplayFormat == MemberDisplayFormat.Hidden)
			{
				if (collection == null)
				{
					return result;
				}
				memberDisplayFormat = MemberDisplayFormat.SingleLine;
			}
			bool includeNonPublic = memberDisplayFormat == MemberDisplayFormat.SeparateLines;
			bool flag2 = memberDisplayFormat == MemberDisplayFormat.SingleLine;
			object debuggerTypeProxy = ObjectFormatterHelpers.GetDebuggerTypeProxy(obj);
			if (debuggerTypeProxy != null)
			{
				includeNonPublic = false;
				flag = false;
			}
			if (!flag || !flag2)
			{
				FormatMembers(result, obj, debuggerTypeProxy, includeNonPublic, flag2);
			}
			return result;
		}

		private void FormatMembers(Builder result, object obj, object proxy, bool includeNonPublic, bool inlineMembers)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			result.Append(' ');
			if (!VisitedObjects.Add(obj))
			{
				result.AppendInfiniteRecursionMarker();
				return;
			}
			bool flag = false;
			if (proxy == null)
			{
				if (obj is IDictionary dict)
				{
					FormatDictionaryMembers(result, dict, inlineMembers);
					flag = true;
				}
				else if (obj is IEnumerable sequence)
				{
					FormatSequenceMembers(result, sequence, inlineMembers);
					flag = true;
				}
			}
			if (!flag)
			{
				FormatObjectMembers(result, proxy ?? obj, obj.GetType().GetTypeInfo(), includeNonPublic, inlineMembers);
			}
			VisitedObjects.Remove(obj);
		}

		private void FormatObjectMembers(Builder result, object obj, System.Reflection.TypeInfo preProxyTypeInfo, bool includeNonPublic, bool inline)
		{
			int lengthLimit = result.Remaining;
			if (lengthLimit < 0)
			{
				return;
			}
			List<FormattedMember> list = new List<FormattedMember>();
			FormatObjectMembersRecursive(list, obj, includeNonPublic, ref lengthLimit);
			bool flag = UseCollectionFormat(list, preProxyTypeInfo);
			result.AppendGroupOpening();
			for (int i = 0; i < list.Count; i++)
			{
				result.AppendCollectionItemSeparator(i == 0, inline);
				if (flag)
				{
					list[i].AppendAsCollectionEntry(result);
				}
				else
				{
					list[i].Append(result, inline ? "=" : ": ");
				}
				if (result.Remaining <= 0)
				{
					break;
				}
			}
			result.AppendGroupClosing(inline);
		}

		private static bool UseCollectionFormat(IEnumerable<FormattedMember> members, System.Reflection.TypeInfo originalType)
		{
			if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(originalType))
			{
				return members.All((FormattedMember member) => member.Index >= 0);
			}
			return false;
		}

		private void FormatObjectMembersRecursive(List<FormattedMember> result, object obj, bool includeNonPublic, ref int lengthLimit)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			System.Reflection.TypeInfo typeInfo = obj.GetType().GetTypeInfo();
			while (typeInfo != null)
			{
				list.AddRange(typeInfo.DeclaredFields.Where((FieldInfo f) => !f.IsStatic));
				list.AddRange(typeInfo.DeclaredProperties.Where((PropertyInfo f) => f.GetMethod != null && !f.GetMethod.IsStatic));
				typeInfo = typeInfo.BaseType?.GetTypeInfo();
			}
			list.Sort(delegate(MemberInfo x, MemberInfo y)
			{
				int num2 = StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
				if (num2 == 0)
				{
					num2 = StringComparer.Ordinal.Compare(x.Name, y.Name);
				}
				return num2;
			});
			foreach (MemberInfo item in list)
			{
				if (!_formatter.Filter.Include(item))
				{
					continue;
				}
				bool flag = false;
				bool flag2 = false;
				DebuggerBrowsableAttribute debuggerBrowsableAttribute = (DebuggerBrowsableAttribute)item.GetCustomAttributes(typeof(DebuggerBrowsableAttribute), inherit: false).FirstOrDefault();
				if (debuggerBrowsableAttribute != null)
				{
					if (debuggerBrowsableAttribute.State == DebuggerBrowsableState.Never)
					{
						continue;
					}
					flag2 = true;
					flag = debuggerBrowsableAttribute.State == DebuggerBrowsableState.RootHidden;
				}
				if (item is FieldInfo fieldInfo)
				{
					if (!(includeNonPublic | flag2) && !fieldInfo.IsPublic && !fieldInfo.IsFamily && !fieldInfo.IsFamilyOrAssembly)
					{
						continue;
					}
				}
				else
				{
					PropertyInfo propertyInfo = (PropertyInfo)item;
					MethodInfo getMethod = propertyInfo.GetMethod;
					if (getMethod == null)
					{
						continue;
					}
					MethodInfo setMethod = propertyInfo.SetMethod;
					if ((!(includeNonPublic | flag2) && !getMethod.IsPublic && !getMethod.IsFamily && !getMethod.IsFamilyOrAssembly && (!(setMethod != null) || (!setMethod.IsPublic && !setMethod.IsFamily && !setMethod.IsFamilyOrAssembly))) || getMethod.GetParameters().Length != 0)
					{
						continue;
					}
				}
				DebuggerDisplayAttribute applicableDebuggerDisplayAttribute = ObjectFormatterHelpers.GetApplicableDebuggerDisplayAttribute(item);
				if (applicableDebuggerDisplayAttribute != null)
				{
					string name = FormatWithEmbeddedExpressions(lengthLimit, applicableDebuggerDisplayAttribute.Name, obj) ?? item.Name;
					string value = FormatWithEmbeddedExpressions(lengthLimit, applicableDebuggerDisplayAttribute.Value, obj) ?? string.Empty;
					if (!AddMember(result, new FormattedMember(-1, name, value), ref lengthLimit))
					{
						break;
					}
					continue;
				}
				object memberValue = ObjectFormatterHelpers.GetMemberValue(item, obj, out var exception);
				if (exception != null)
				{
					Builder builder = MakeMemberBuilder(lengthLimit);
					FormatException(builder, exception);
					if (!AddMember(result, new FormattedMember(-1, item.Name, builder.ToString()), ref lengthLimit))
					{
						break;
					}
				}
				else if (flag)
				{
					if (memberValue == null || VisitedObjects.Contains(memberValue))
					{
						continue;
					}
					if (memberValue is Array array)
					{
						int num = 0;
						foreach (object item2 in array)
						{
							Builder builder2 = MakeMemberBuilder(lengthLimit);
							FormatObjectRecursive(builder2, item2, isRoot: false, out var debuggerDisplayName);
							if (!string.IsNullOrEmpty(debuggerDisplayName))
							{
								debuggerDisplayName = FormatWithEmbeddedExpressions(MakeMemberBuilder(lengthLimit), debuggerDisplayName, item2).ToString();
							}
							if (!AddMember(result, new FormattedMember(num, debuggerDisplayName, builder2.ToString()), ref lengthLimit))
							{
								return;
							}
							num++;
						}
					}
					else if (_formatter.PrimitiveFormatter.FormatPrimitive(memberValue, _primitiveOptions) == null && VisitedObjects.Add(memberValue))
					{
						FormatObjectMembersRecursive(result, memberValue, includeNonPublic, ref lengthLimit);
						VisitedObjects.Remove(memberValue);
					}
				}
				else
				{
					Builder builder3 = MakeMemberBuilder(lengthLimit);
					FormatObjectRecursive(builder3, memberValue, isRoot: false, out var debuggerDisplayName2);
					debuggerDisplayName2 = ((!string.IsNullOrEmpty(debuggerDisplayName2)) ? FormatWithEmbeddedExpressions(MakeMemberBuilder(lengthLimit), debuggerDisplayName2, memberValue).ToString() : item.Name);
					if (!AddMember(result, new FormattedMember(-1, debuggerDisplayName2, builder3.ToString()), ref lengthLimit))
					{
						break;
					}
				}
			}
		}

		private bool AddMember(List<FormattedMember> members, FormattedMember member, ref int remainingLength)
		{
			members.Add(member);
			if (remainingLength == int.MinValue)
			{
				return false;
			}
			remainingLength -= member.MinimalLength;
			if (remainingLength <= 0)
			{
				remainingLength = int.MinValue;
			}
			return true;
		}

		private void FormatException(Builder result, Exception exception)
		{
			result.Append("!<");
			result.Append(_formatter.TypeNameFormatter.FormatTypeName(exception.GetType(), _typeNameOptions));
			result.Append('>');
		}

		private void FormatKeyValuePair(Builder result, object obj)
		{
			System.Reflection.TypeInfo typeInfo = obj.GetType().GetTypeInfo();
			object value = typeInfo.GetDeclaredProperty("Key").GetValue(obj, Array.Empty<object>());
			object value2 = typeInfo.GetDeclaredProperty("Value").GetValue(obj, Array.Empty<object>());
			result.AppendGroupOpening();
			result.AppendCollectionItemSeparator(isFirst: true, inline: true);
			FormatObjectRecursive(result, value, isRoot: false, out var debuggerDisplayName);
			result.AppendCollectionItemSeparator(isFirst: false, inline: true);
			FormatObjectRecursive(result, value2, isRoot: false, out debuggerDisplayName);
			result.AppendGroupClosing(inline: true);
		}

		private void FormatCollectionHeader(Builder result, ICollection collection)
		{
			if (collection is Array array)
			{
				result.Append(_formatter.TypeNameFormatter.FormatArrayTypeName(array.GetType(), array, _typeNameOptions));
				return;
			}
			result.Append(_formatter.TypeNameFormatter.FormatTypeName(collection.GetType(), _typeNameOptions));
			try
			{
				result.Append('(');
				result.Append(collection.Count.ToString());
				result.Append(')');
			}
			catch (Exception)
			{
			}
		}

		private void FormatArray(Builder result, Array array)
		{
			FormatCollectionHeader(result, array);
			if (array.Rank > 1)
			{
				FormatMultidimensionalArrayElements(result, array, _memberDisplayFormat != MemberDisplayFormat.SeparateLines);
				return;
			}
			result.Append(' ');
			FormatSequenceMembers(result, array, _memberDisplayFormat != MemberDisplayFormat.SeparateLines);
		}

		private void FormatDictionaryMembers(Builder result, IDictionary dict, bool inline)
		{
			result.AppendGroupOpening();
			int num = 0;
			try
			{
				IDictionaryEnumerator enumerator = dict.GetEnumerator();
				using (enumerator as IDisposable)
				{
					while (enumerator.MoveNext())
					{
						DictionaryEntry entry = enumerator.Entry;
						result.AppendCollectionItemSeparator(num == 0, inline);
						result.AppendGroupOpening();
						result.AppendCollectionItemSeparator(isFirst: true, inline: true);
						FormatObjectRecursive(result, entry.Key, isRoot: false, out var debuggerDisplayName);
						result.AppendCollectionItemSeparator(isFirst: false, inline: true);
						FormatObjectRecursive(result, entry.Value, isRoot: false, out debuggerDisplayName);
						result.AppendGroupClosing(inline: true);
						num++;
					}
				}
			}
			catch (Exception exception)
			{
				result.AppendCollectionItemSeparator(num == 0, inline);
				FormatException(result, exception);
				result.Append(' ');
				result.Append(_builderOptions.Ellipsis);
			}
			result.AppendGroupClosing(inline);
		}

		private void FormatSequenceMembers(Builder result, IEnumerable sequence, bool inline)
		{
			result.AppendGroupOpening();
			int num = 0;
			try
			{
				foreach (object item in sequence)
				{
					result.AppendCollectionItemSeparator(num == 0, inline);
					FormatObjectRecursive(result, item, isRoot: false, out var _);
					num++;
				}
			}
			catch (Exception exception)
			{
				result.AppendCollectionItemSeparator(num == 0, inline);
				FormatException(result, exception);
				result.Append(" ...");
			}
			result.AppendGroupClosing(inline);
		}

		private void FormatMultidimensionalArrayElements(Builder result, Array array, bool inline)
		{
			if (array.Length == 0)
			{
				result.AppendCollectionItemSeparator(isFirst: true, inline: true);
				result.AppendGroupOpening();
				result.AppendGroupClosing(inline: true);
				return;
			}
			int[] array2 = new int[array.Rank];
			for (int num = array.Rank - 1; num >= 0; num--)
			{
				array2[num] = array.GetLowerBound(num);
			}
			int num2 = 0;
			int num3 = 0;
			while (true)
			{
				int num4 = array2.Length - 1;
				while (array2[num4] > array.GetUpperBound(num4))
				{
					array2[num4] = array.GetLowerBound(num4);
					result.AppendGroupClosing(inline || num2 != 1);
					num2--;
					num4--;
					if (num4 < 0)
					{
						return;
					}
					array2[num4]++;
				}
				result.AppendCollectionItemSeparator(num3 == 0, inline || num2 != 1);
				num4 = array2.Length - 1;
				while (num4 >= 0 && array2[num4] == array.GetLowerBound(num4))
				{
					result.AppendGroupOpening();
					num2++;
					result.AppendCollectionItemSeparator(isFirst: true, inline || num2 != 1);
					num4--;
				}
				FormatObjectRecursive(result, array.GetValue(array2), isRoot: false, out var _);
				array2[^1]++;
				num3++;
			}
		}

		private bool IsTuple(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Type type = obj.GetType();
			if (!type.IsGenericType)
			{
				return false;
			}
			int num = type.FullName.IndexOf('`');
			if (num < 0)
			{
				return false;
			}
			string text = type.FullName.Substring(0, num);
			if (!(text == "System.ValueTuple"))
			{
				return text == "System.Tuple";
			}
			return true;
		}

		private void ObjectToString(Builder result, object obj)
		{
			try
			{
				string str = obj.ToString();
				if (IsTuple(obj))
				{
					result.Append(str);
					return;
				}
				result.Append('[');
				result.Append(str);
				result.Append(']');
			}
			catch (Exception exception)
			{
				FormatException(result, exception);
			}
		}

		private string FormatWithEmbeddedExpressions(int lengthLimit, string format, object obj)
		{
			if (string.IsNullOrEmpty(format))
			{
				return null;
			}
			Builder result = new Builder(_builderOptions.WithMaximumOutputLength(lengthLimit), suppressEllipsis: true);
			return FormatWithEmbeddedExpressions(result, format, obj).ToString();
		}

		private Builder FormatWithEmbeddedExpressions(Builder result, string format, object obj)
		{
			int num = 0;
			while (num < format.Length)
			{
				char c = format[num++];
				if (c == '{')
				{
					if (num >= 2 && format[num - 2] == '\\')
					{
						result.Append('{');
						continue;
					}
					int num2 = format.IndexOf('}', num);
					string text;
					if (num2 == -1 || (text = ObjectFormatterHelpers.ParseSimpleMemberName(format, num, num2, out var noQuotes, out var isCallable)) == null)
					{
						result.Append(format, num - 1, format.Length - num + 1);
						break;
					}
					MemberInfo memberInfo = ObjectFormatterHelpers.ResolveMember(obj, text, isCallable);
					if (memberInfo == null)
					{
						result.AppendFormat(isCallable ? "!<Method '{0}' not found>" : "!<Member '{0}' not found>", text);
					}
					else
					{
						object memberValue = ObjectFormatterHelpers.GetMemberValue(memberInfo, obj, out var exception);
						if (exception != null)
						{
							FormatException(result, exception);
						}
						else
						{
							MemberDisplayFormat memberDisplayFormat = _memberDisplayFormat;
							CommonPrimitiveFormatterOptions primitiveOptions = _primitiveOptions;
							_memberDisplayFormat = MemberDisplayFormat.Hidden;
							_primitiveOptions = new CommonPrimitiveFormatterOptions(_primitiveOptions.NumberRadix, _primitiveOptions.IncludeCharacterCodePoints, !noQuotes, _primitiveOptions.EscapeNonPrintableCharacters, _primitiveOptions.CultureInfo);
							FormatObjectRecursive(result, memberValue, isRoot: false, out var _);
							_primitiveOptions = primitiveOptions;
							_memberDisplayFormat = memberDisplayFormat;
						}
					}
					num = num2 + 1;
				}
				else
				{
					result.Append(c);
				}
			}
			return result;
		}
	}

	protected virtual MemberFilter Filter { get; } = new CommonMemberFilter();

	protected abstract CommonTypeNameFormatter TypeNameFormatter { get; }

	protected abstract CommonPrimitiveFormatter PrimitiveFormatter { get; }

	public override string FormatObject(object obj, PrintOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return new Visitor(this, GetInternalBuilderOptions(options), GetPrimitiveOptions(options), GetTypeNameOptions(options), options.MemberDisplayFormat).FormatObject(obj);
	}

	protected virtual BuilderOptions GetInternalBuilderOptions(PrintOptions printOptions)
	{
		return new BuilderOptions("  ", Environment.NewLine, printOptions.Ellipsis, int.MaxValue, printOptions.MaximumOutputLength);
	}

	protected virtual CommonPrimitiveFormatterOptions GetPrimitiveOptions(PrintOptions printOptions)
	{
		return new CommonPrimitiveFormatterOptions(printOptions.NumberRadix, includeCodePoints: false, quoteStringsAndCharacters: true, printOptions.EscapeNonPrintableCharacters, CultureInfo.CurrentUICulture);
	}

	protected virtual CommonTypeNameFormatterOptions GetTypeNameOptions(PrintOptions printOptions)
	{
		return new CommonTypeNameFormatterOptions(printOptions.NumberRadix, showNamespaces: false);
	}

	public override string FormatException(Exception e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(e.GetType());
		builder.Append(": ");
		builder.Append(e.Message);
		builder.Append(Environment.NewLine);
		StackFrame[] frames = new StackTrace(e, fNeedFileInfo: true).GetFrames();
		foreach (StackFrame stackFrame in frames)
		{
			if (!Filter.Include(stackFrame))
			{
				continue;
			}
			MethodBase method = stackFrame.GetMethod();
			string text = FormatMethodSignature(method);
			if (text != null)
			{
				builder.Append("  + ");
				builder.Append(text);
				string fileName = stackFrame.GetFileName();
				if (fileName != null)
				{
					builder.Append(string.Format(CultureInfo.CurrentUICulture, ScriptingResources.AtFileLine, fileName, stackFrame.GetFileLineNumber()));
				}
				builder.AppendLine();
			}
		}
		return instance.ToStringAndFree();
	}

	protected internal virtual string FormatMethodSignature(MethodBase method)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		Type declaringType = method.DeclaringType;
		CommonTypeNameFormatterOptions options = new CommonTypeNameFormatterOptions(10, showNamespaces: true);
		builder.Append(TypeNameFormatter.FormatTypeName(declaringType, options));
		builder.Append('.');
		builder.Append(method.Name);
		if (method.IsGenericMethod)
		{
			builder.Append(TypeNameFormatter.FormatTypeArguments(method.GetGenericArguments(), options));
		}
		builder.Append('(');
		bool flag = true;
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				builder.Append(", ");
			}
			if (parameterInfo.ParameterType.IsByRef)
			{
				builder.Append(FormatRefKind(parameterInfo));
				builder.Append(' ');
				builder.Append(TypeNameFormatter.FormatTypeName(parameterInfo.ParameterType.GetElementType(), options));
			}
			else
			{
				builder.Append(TypeNameFormatter.FormatTypeName(parameterInfo.ParameterType, options));
			}
		}
		builder.Append(')');
		return instance.ToStringAndFree();
	}

	protected abstract string FormatRefKind(ParameterInfo parameter);
}
