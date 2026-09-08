using System;
using System.IO;
using System.Reflection;

namespace Thetis
{
    internal static class GPUWaterfallShaderLoader
    {
        internal static byte[] Load(string filename)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                foreach (string resource in asm.GetManifestResourceNames())
                {
                    if (resource.EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream s = asm.GetManifestResourceStream(resource))
                        {
                            if (s == null) continue;
                            byte[] bytes = new byte[s.Length];
                            int off = 0;
                            while (off < bytes.Length)
                            {
                                int n = s.Read(bytes, off, bytes.Length - off);
                                if (n <= 0) break;
                                off += n;
                            }
                            if (off == bytes.Length) return bytes;
                        }
                    }
                }
                string disk = Path.Combine(Path.GetDirectoryName(asm.Location), filename);
                return File.Exists(disk) ? File.ReadAllBytes(disk) : null;
            }
            catch { return null; }
        }
    }
}
