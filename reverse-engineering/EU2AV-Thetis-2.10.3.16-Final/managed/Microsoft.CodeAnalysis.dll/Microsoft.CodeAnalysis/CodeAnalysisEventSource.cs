using System;
using System.Diagnostics.Tracing;

namespace Microsoft.CodeAnalysis;

[EventSource(Name = "Microsoft-CodeAnalysis-General")]
internal sealed class CodeAnalysisEventSource : EventSource
{
	public static class Keywords
	{
		public const EventKeywords Performance = (EventKeywords)1L;

		public const EventKeywords Correctness = (EventKeywords)2L;

		public const EventKeywords AnalyzerLoading = (EventKeywords)4L;
	}

	public static class Tasks
	{
		public const EventTask GeneratorDriverRunTime = (EventTask)1;

		public const EventTask SingleGeneratorRunTime = (EventTask)2;

		public const EventTask BuildStateTable = (EventTask)3;

		public const EventTask Compilation = (EventTask)4;

		public const EventTask AnalyzerAssemblyLoader = (EventTask)5;
	}

	public static readonly CodeAnalysisEventSource Log = new CodeAnalysisEventSource();

	[Event(1, Keywords = (EventKeywords)1L, Level = EventLevel.Informational, Opcode = EventOpcode.Start, Task = (EventTask)1)]
	internal void StartGeneratorDriverRunTime(string id)
	{
		WriteEvent(1, id);
	}

	[Event(2, Message = "Generators ran for {0} ticks", Keywords = (EventKeywords)1L, Level = EventLevel.Informational, Opcode = EventOpcode.Stop, Task = (EventTask)1)]
	internal void StopGeneratorDriverRunTime(long elapsedTicks, string id)
	{
		WriteEvent(2, elapsedTicks, id);
	}

	[Event(3, Keywords = (EventKeywords)1L, Level = EventLevel.Informational, Opcode = EventOpcode.Start, Task = (EventTask)2)]
	internal void StartSingleGeneratorRunTime(string generatorName, string assemblyPath, string id)
	{
		WriteEvent(3, generatorName, assemblyPath, id);
	}

	[Event(4, Message = "Generator {0} ran for {2} ticks", Keywords = (EventKeywords)1L, Level = EventLevel.Informational, Opcode = EventOpcode.Stop, Task = (EventTask)2)]
	internal unsafe void StopSingleGeneratorRunTime(string generatorName, string assemblyPath, long elapsedTicks, string id)
	{
		if (!IsEnabled())
		{
			return;
		}
		fixed (char* ptr = generatorName)
		{
			fixed (char* ptr2 = assemblyPath)
			{
				fixed (char* ptr3 = id)
				{
					Span<EventData> span = stackalloc EventData[4]
					{
						GetEventDataForString(generatorName, ptr),
						GetEventDataForString(assemblyPath, ptr2),
						GetEventDataForInt64(&elapsedTicks),
						GetEventDataForString(id, ptr3)
					};
					fixed (EventData* data = span)
					{
						WriteEventCore(4, span.Length, data);
					}
				}
			}
		}
	}

	[Event(5, Message = "Generator '{0}' failed with exception: {1}", Level = EventLevel.Error)]
	internal void GeneratorException(string generatorName, string exception)
	{
		WriteEvent(5, generatorName, exception);
	}

	[Event(6, Message = "Node {0} transformed", Keywords = (EventKeywords)2L, Level = EventLevel.Verbose, Task = (EventTask)3)]
	internal unsafe void NodeTransform(int nodeHashCode, string name, string tableType, int previousTable, string previousTableContent, int newTable, string newTableContent, int input1, int input2)
	{
		if (!IsEnabled())
		{
			return;
		}
		fixed (char* ptr = name)
		{
			fixed (char* ptr2 = tableType)
			{
				fixed (char* ptr3 = previousTableContent)
				{
					fixed (char* ptr4 = newTableContent)
					{
						Span<EventData> span = stackalloc EventData[9]
						{
							GetEventDataForInt32(&nodeHashCode),
							GetEventDataForString(name, ptr),
							GetEventDataForString(tableType, ptr2),
							GetEventDataForInt32(&previousTable),
							GetEventDataForString(previousTableContent, ptr3),
							GetEventDataForInt32(&newTable),
							GetEventDataForString(newTableContent, ptr4),
							GetEventDataForInt32(&input1),
							GetEventDataForInt32(&input2)
						};
						fixed (EventData* data = span)
						{
							WriteEventCore(6, span.Length, data);
						}
					}
				}
			}
		}
	}

	[Event(7, Message = "Server compilation {0} started", Keywords = (EventKeywords)1L, Level = EventLevel.Informational, Opcode = EventOpcode.Start, Task = (EventTask)4)]
	internal void StartServerCompilation(string name)
	{
		WriteEvent(7, name);
	}

	[Event(8, Message = "Server compilation {0} completed", Keywords = (EventKeywords)1L, Level = EventLevel.Informational, Opcode = EventOpcode.Stop, Task = (EventTask)4)]
	internal void StopServerCompilation(string name)
	{
		WriteEvent(8, name);
	}

	[Event(9, Message = "ALC for directory '{0}' created", Keywords = (EventKeywords)4L, Level = EventLevel.Informational, Opcode = EventOpcode.Start, Task = (EventTask)5)]
	internal void CreateAssemblyLoadContext(string directory, string? alc)
	{
		WriteEvent(9, directory, alc);
	}

	[Event(10, Message = "ALC for directory '{0}' disposed", Keywords = (EventKeywords)4L, Level = EventLevel.Informational, Opcode = EventOpcode.Stop, Task = (EventTask)5)]
	internal void DisposeAssemblyLoadContext(string directory, string? alc)
	{
		WriteEvent(10, directory, alc);
	}

	[Event(11, Message = "ALC for directory '{0}' disposal failed with exception '{1}'", Keywords = (EventKeywords)4L, Level = EventLevel.Error, Opcode = EventOpcode.Stop, Task = (EventTask)5)]
	internal void DisposeAssemblyLoadContextException(string directory, string errorMessage, string? alc)
	{
		WriteEvent(11, directory, errorMessage, alc);
	}

	[Event(12, Message = "CreateNonLockingLoader", Keywords = (EventKeywords)4L, Level = EventLevel.Informational, Task = (EventTask)5)]
	internal void CreateNonLockingLoader(string directory)
	{
		WriteEvent(12, directory);
	}

	[Event(13, Message = "Request add Analyzer reference '{0}' to project '{1}'", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal void AnalyzerReferenceRequestAddToProject(string path, string projectName)
	{
		WriteEvent(13, path, projectName);
	}

	[Event(14, Message = "Analyzer reference '{0}' was added to project '{1}'", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal void AnalyzerReferenceAddedToProject(string path, string projectName)
	{
		WriteEvent(14, path, projectName);
	}

	[Event(15, Message = "Request remove Analyzer reference '{0}' from project '{1}'", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal void AnalyzerReferenceRequestRemoveFromProject(string path, string projectName)
	{
		WriteEvent(15, path, projectName);
	}

	[Event(16, Message = "Analyzer reference '{0}' was removed from project '{1}'", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal void AnalyzerReferenceRemovedFromProject(string path, string projectName)
	{
		WriteEvent(16, path, projectName);
	}

	[Event(17, Message = "Analyzer reference was redirected by '{0}' from '{1}' to '{2}' for project '{3}'", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal unsafe void AnanlyzerReferenceRedirected(string redirectorType, string originalPath, string newPath, string project)
	{
		if (!IsEnabled())
		{
			return;
		}
		fixed (char* ptr = redirectorType)
		{
			fixed (char* ptr2 = originalPath)
			{
				fixed (char* ptr3 = newPath)
				{
					fixed (char* ptr4 = project)
					{
						Span<EventData> span = new Span<EventData>(new EventData[4]
						{
							GetEventDataForString(redirectorType, ptr),
							GetEventDataForString(originalPath, ptr2),
							GetEventDataForString(newPath, ptr3),
							GetEventDataForString(project, ptr4)
						});
						fixed (EventData* data = span)
						{
							WriteEventCore(17, span.Length, data);
						}
					}
				}
			}
		}
	}

	[Event(18, Message = "ALC for directory '{0}': Assembly '{1}' was resolved by '{2}' ", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal unsafe void ResolvedAssembly(string directory, string assemblyName, string resolver, string filePath, string alc)
	{
		if (!IsEnabled())
		{
			return;
		}
		fixed (char* ptr = directory)
		{
			fixed (char* ptr2 = assemblyName)
			{
				fixed (char* ptr3 = resolver)
				{
					fixed (char* ptr4 = filePath)
					{
						fixed (char* ptr5 = alc)
						{
							Span<EventData> span = new Span<EventData>(new EventData[5]
							{
								GetEventDataForString(directory, ptr),
								GetEventDataForString(assemblyName, ptr2),
								GetEventDataForString(resolver, ptr3),
								GetEventDataForString(filePath, ptr4),
								GetEventDataForString(alc, ptr5)
							});
							fixed (EventData* data = span)
							{
								WriteEventCore(18, span.Length, data);
							}
						}
					}
				}
			}
		}
	}

	[Event(19, Message = "ALC for directory '{0}': Failed to resolve assembly '{1}' ", Keywords = (EventKeywords)4L, Level = EventLevel.Informational)]
	internal void ResolveAssemblyFailed(string directory, string assemblyName)
	{
		WriteEvent(19, directory, assemblyName);
	}

	[Event(20, Message = "Project '{0}' created with file path '{1}'", Level = EventLevel.Informational)]
	internal void ProjectCreated(string projectSystemName, string? filePath)
	{
		WriteEvent(20, projectSystemName, filePath ?? string.Empty);
	}

	private unsafe static EventData GetEventDataForString(string value, char* ptr)
	{
		fixed (char* ptr2 = value)
		{
			if (ptr2 != ptr)
			{
				throw new ArgumentException("Pinned value must match string.");
			}
		}
		return new EventData
		{
			DataPointer = (IntPtr)ptr,
			Size = (value.Length + 1) * 2
		};
	}

	private unsafe static EventData GetEventDataForInt32(int* ptr)
	{
		return new EventData
		{
			DataPointer = (IntPtr)ptr,
			Size = 4
		};
	}

	private unsafe static EventData GetEventDataForInt64(long* ptr)
	{
		return new EventData
		{
			DataPointer = (IntPtr)ptr,
			Size = 8
		};
	}
}
