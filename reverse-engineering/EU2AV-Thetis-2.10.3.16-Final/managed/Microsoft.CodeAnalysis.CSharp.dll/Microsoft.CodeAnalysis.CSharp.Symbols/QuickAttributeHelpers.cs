namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class QuickAttributeHelpers
{
	public static QuickAttributes GetQuickAttributes(string name, bool inAttribute)
	{
		QuickAttributes quickAttributes = QuickAttributes.None;
		if (matches(AttributeDescription.TypeIdentifierAttribute))
		{
			quickAttributes |= QuickAttributes.TypeIdentifier;
		}
		else if (matches(AttributeDescription.TypeForwardedToAttribute))
		{
			quickAttributes |= QuickAttributes.TypeForwardedTo;
		}
		else if (matches(AttributeDescription.IndexerNameAttribute))
		{
			quickAttributes |= QuickAttributes.IndexerName;
		}
		else if (matches(AttributeDescription.AssemblyKeyNameAttribute))
		{
			quickAttributes |= QuickAttributes.AssemblyKeyName;
		}
		else if (matches(AttributeDescription.AssemblyKeyFileAttribute))
		{
			quickAttributes |= QuickAttributes.AssemblyKeyFile;
		}
		else if (matches(AttributeDescription.AssemblySignatureKeyAttribute))
		{
			quickAttributes |= QuickAttributes.AssemblySignatureKey;
		}
		return quickAttributes;
		bool matches(AttributeDescription attributeDescription)
		{
			if (name == attributeDescription.Name)
			{
				return true;
			}
			if (inAttribute && name.Length + "Attribute".Length == attributeDescription.Name.Length && attributeDescription.Name.StartsWith(name))
			{
				return true;
			}
			return false;
		}
	}
}
