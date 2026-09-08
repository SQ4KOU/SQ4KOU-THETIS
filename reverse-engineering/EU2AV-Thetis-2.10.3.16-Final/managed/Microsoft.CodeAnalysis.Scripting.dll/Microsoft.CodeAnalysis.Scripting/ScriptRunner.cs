using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Scripting;

public delegate Task<T> ScriptRunner<T>(object globals = null, CancellationToken cancellationToken = default(CancellationToken));
