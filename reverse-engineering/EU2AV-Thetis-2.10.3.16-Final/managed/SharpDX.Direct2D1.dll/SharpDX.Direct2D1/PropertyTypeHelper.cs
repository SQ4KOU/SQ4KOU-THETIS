namespace SharpDX.Direct2D1;

public static class PropertyTypeHelper
{
	public static string ConvertToString(PropertyType propertyType)
	{
		return propertyType switch
		{
			PropertyType.Bool => "bool", 
			PropertyType.Clsid => "clsid", 
			PropertyType.ColorContext => "colorcontext", 
			PropertyType.Enum => "enum", 
			PropertyType.Float => "float", 
			PropertyType.Int32 => "int32", 
			PropertyType.IUnknown => "iunknown", 
			PropertyType.Matrix3x2 => "matrix3x2", 
			PropertyType.Matrix4x3 => "matrix4x3", 
			PropertyType.Matrix4x4 => "matrix4x4", 
			PropertyType.Matrix5x4 => "matrix5x4", 
			PropertyType.String => "string", 
			PropertyType.UInt32 => "uint32", 
			PropertyType.Unknown => "unknown", 
			PropertyType.Vector2 => "vector2", 
			PropertyType.Vector3 => "vector3", 
			PropertyType.Vector4 => "vector4", 
			_ => "unknown", 
		};
	}
}
