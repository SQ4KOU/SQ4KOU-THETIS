using System;
using System.Collections.Generic;
using System.Linq;
using ExCSS;

namespace Svg.Css;

internal static class ExCssQuery
{
	public static IEnumerable<SvgElement> QuerySelectorAll(this SvgElement elem, ISelector selector, SvgElementFactory elementFactory)
	{
		IEnumerable<SvgElement> arg = Enumerable.Repeat(elem, 1);
		ExSvgElementOps exSvgElementOps = new ExSvgElementOps(elementFactory);
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func = GetFunc(selector, exSvgElementOps, exSvgElementOps.Universal());
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> descendants = exSvgElementOps.Descendant();
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func2 = func;
		func = (IEnumerable<SvgElement> f) => func2(descendants(f));
		return func(arg).Distinct();
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(CompoundSelector selector, ExSvgElementOps ops, Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> inFunc)
	{
		foreach (ISelector item in selector)
		{
			inFunc = GetFunc(item, ops, inFunc);
		}
		return inFunc;
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(FirstChildSelector selector, ExSvgElementOps ops)
	{
		int step = selector.Step;
		int offset = selector.Offset;
		if (offset == 0)
		{
			return ops.FirstChild();
		}
		return ops.NthChild(step, offset);
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(FirstTypeSelector selector, ExSvgElementOps ops)
	{
		int step = selector.Step;
		int offset = selector.Offset;
		return ops.NthType(step, offset);
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(LastTypeSelector selector, ExSvgElementOps ops)
	{
		int step = selector.Step;
		int offset = selector.Offset;
		return ops.NthLastType(step, offset);
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(LastChildSelector selector, ExSvgElementOps ops)
	{
		int step = selector.Step;
		int offset = selector.Offset;
		if (offset == 0)
		{
			return ops.LastChild();
		}
		return ops.NthLastChild(step, offset);
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(ListSelector listSelector, ExSvgElementOps ops, Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> inFunc)
	{
		List<Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>> results = new List<Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>>();
		foreach (ISelector item in listSelector)
		{
			results.Add(GetFunc(item, ops, null));
		}
		return delegate(IEnumerable<SvgElement> f)
		{
			IEnumerable<SvgElement> arg = inFunc(f);
			IEnumerable<SvgElement> enumerable = results[0](arg);
			for (int i = 1; i < results.Count; i++)
			{
				enumerable = enumerable.Union(results[i](arg));
			}
			return enumerable;
		};
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(PseudoClassSelector selector, ExSvgElementOps ops, Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> inFunc)
	{
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> pseudoFunc;
		if (selector.Class == PseudoClassNames.FirstChild)
		{
			pseudoFunc = ops.FirstChild();
		}
		else if (selector.Class == PseudoClassNames.LastChild)
		{
			pseudoFunc = ops.LastChild();
		}
		else if (selector.Class == PseudoClassNames.Empty)
		{
			pseudoFunc = ops.Empty();
		}
		else if (selector.Class == PseudoClassNames.OnlyChild)
		{
			pseudoFunc = ops.OnlyChild();
		}
		else if (selector.Class == PseudoClassNames.Hover)
		{
			pseudoFunc = ops.Empty();
		}
		else if (selector.Class.StartsWith(PseudoClassNames.Not))
		{
			string content = selector.Class.Substring(PseudoClassNames.Not.Length + 1, selector.Class.Length - 2 - PseudoClassNames.Not.Length);
			ISelector selector2 = new StylesheetParser(includeUnknownRules: true, includeUnknownDeclarations: true, tolerateInvalidSelectors: false, tolerateInvalidValues: true).Parse(content).StyleRules.First().Selector;
			Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func = GetFunc(selector2, ops, ops.Universal());
			Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> descendants = ops.Descendant();
			Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func2 = func;
			func = (IEnumerable<SvgElement> f) => func2(descendants(f));
			HashSet<SvgElement> notElements = null;
			pseudoFunc = (IEnumerable<SvgElement> f) => f.Where(delegate(SvgElement e)
			{
				if (notElements == null)
				{
					notElements = func(f).ToHashSet();
				}
				return !notElements.Contains(e);
			});
		}
		else if (selector.Class.StartsWith(PseudoClassNames.Lang))
		{
			pseudoFunc = ops.Empty();
		}
		else
		{
			if (!selector.Class.StartsWith(PseudoClassNames.Root))
			{
				throw new NotImplementedException();
			}
			pseudoFunc = ops.Root();
		}
		if (inFunc == null)
		{
			return pseudoFunc;
		}
		return (IEnumerable<SvgElement> f) => pseudoFunc(inFunc(f));
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(ComplexSelector selector, ExSvgElementOps ops, Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> inFunc)
	{
		List<Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>> list = new List<Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>>();
		foreach (CombinatorSelector item in selector)
		{
			list.Add(GetFunc(item.Selector, ops, null));
			Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func;
			if (item.Delimiter == Combinator.Child.Delimiter)
			{
				func = ops.Child();
			}
			else if (item.Delimiter == Combinators.Descendent)
			{
				func = ops.Descendant();
			}
			else
			{
				if (item.Delimiter == Combinator.Deep.Delimiter)
				{
					throw new NotImplementedException();
				}
				if (item.Delimiter == Combinators.Adjacent)
				{
					func = ops.Adjacent();
				}
				else if (item.Delimiter == Combinators.Sibling)
				{
					func = ops.GeneralSibling();
				}
				else
				{
					if (item.Delimiter == Combinators.Pipe)
					{
						throw new NotImplementedException();
					}
					if (item.Delimiter == Combinators.Column)
					{
						throw new NotImplementedException();
					}
					if (item.Delimiter != null)
					{
						throw new NotImplementedException();
					}
					func = null;
				}
			}
			if (func != null)
			{
				list.Add(func);
			}
		}
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func2 = inFunc;
		foreach (Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> it in list)
		{
			if (func2 == null)
			{
				func2 = it;
				continue;
			}
			Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> temp = func2;
			func2 = (IEnumerable<SvgElement> f) => it(temp(f));
		}
		return func2;
	}

	private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(ISelector selector, ExSvgElementOps ops, Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> inFunc)
	{
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func;
		if (!(selector is AllSelector))
		{
			if (!(selector is AttrAvailableSelector attrAvailableSelector))
			{
				if (!(selector is AttrBeginsSelector attrBeginsSelector))
				{
					if (!(selector is AttrContainsSelector attrContainsSelector))
					{
						if (!(selector is AttrEndsSelector attrEndsSelector))
						{
							if (!(selector is AttrHyphenSelector attrHyphenSelector))
							{
								if (!(selector is AttrListSelector attrListSelector))
								{
									if (!(selector is AttrMatchSelector attrMatchSelector))
									{
										if (!(selector is AttrNotMatchSelector attrNotMatchSelector))
										{
											if (!(selector is ClassSelector classSelector))
											{
												if (!(selector is ComplexSelector selector2))
												{
													if (!(selector is CompoundSelector selector3))
													{
														if (!(selector is FirstChildSelector selector4))
														{
															if (!(selector is LastChildSelector selector5))
															{
																if (selector is FirstColumnSelector)
																{
																	throw new NotImplementedException();
																}
																if (selector is LastColumnSelector)
																{
																	throw new NotImplementedException();
																}
																if (!(selector is FirstTypeSelector selector6))
																{
																	if (!(selector is LastTypeSelector selector7))
																	{
																		if (!(selector is ChildSelector))
																		{
																			if (!(selector is ListSelector listSelector))
																			{
																				if (selector is NamespaceSelector)
																				{
																					throw new NotImplementedException();
																				}
																				if (!(selector is PseudoClassSelector selector8))
																				{
																					if (selector is PseudoElementSelector)
																					{
																						throw new NotImplementedException();
																					}
																					if (!(selector is TypeSelector typeSelector))
																					{
																						if (selector is UnknownSelector)
																						{
																							throw new NotImplementedException();
																						}
																						if (!(selector is IdSelector idSelector))
																						{
																							if (selector is PageSelector)
																							{
																								throw new NotImplementedException();
																							}
																							throw new NotImplementedException();
																						}
																						func = ops.Id(idSelector.Id);
																					}
																					else
																					{
																						func = ops.Type(typeSelector.Name);
																					}
																				}
																				else
																				{
																					func = GetFunc(selector8, ops, inFunc);
																				}
																			}
																			else
																			{
																				func = GetFunc(listSelector, ops, inFunc);
																			}
																		}
																		else
																		{
																			func = ops.Child();
																		}
																	}
																	else
																	{
																		func = GetFunc(selector7, ops);
																	}
																}
																else
																{
																	func = GetFunc(selector6, ops);
																}
															}
															else
															{
																func = GetFunc(selector5, ops);
															}
														}
														else
														{
															func = GetFunc(selector4, ops);
														}
													}
													else
													{
														func = GetFunc(selector3, ops, inFunc);
													}
												}
												else
												{
													func = GetFunc(selector2, ops, inFunc);
												}
											}
											else
											{
												func = ops.Class(classSelector.Class);
											}
										}
										else
										{
											func = ops.AttributeNotMatch(attrNotMatchSelector.Attribute, attrNotMatchSelector.Value);
										}
									}
									else
									{
										func = ops.AttributeExact(attrMatchSelector.Attribute, attrMatchSelector.Value);
									}
								}
								else
								{
									func = ops.AttributeIncludes(attrListSelector.Attribute, attrListSelector.Value);
								}
							}
							else
							{
								func = ops.AttributeDashMatch(attrHyphenSelector.Attribute, attrHyphenSelector.Value);
							}
						}
						else
						{
							func = ops.AttributeSuffixMatch(attrEndsSelector.Attribute, attrEndsSelector.Value);
						}
					}
					else
					{
						func = ops.AttributeSubstring(attrContainsSelector.Attribute, attrContainsSelector.Value);
					}
				}
				else
				{
					func = ops.AttributePrefixMatch(attrBeginsSelector.Attribute, attrBeginsSelector.Value);
				}
			}
			else
			{
				func = ops.AttributeExists(attrAvailableSelector.Attribute);
			}
		}
		else
		{
			func = ops.Universal();
		}
		Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> func2 = func;
		if (inFunc == null)
		{
			return func2;
		}
		return (IEnumerable<SvgElement> f) => func2(inFunc(f));
	}

	private static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumarable)
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T item in enumarable)
		{
			hashSet.Add(item);
		}
		return hashSet;
	}

	public static int GetSpecificity(this ISelector selector)
	{
		return 0 | (4096 * selector.Specificity.Ids) | (256 * selector.Specificity.Classes) | (16 * selector.Specificity.Tags);
	}
}
