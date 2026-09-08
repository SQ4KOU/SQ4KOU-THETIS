using System;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class SyntaxTreeDiagnosticEnumerator
{
	private struct NodeIteration(GreenNode node)
	{
		public readonly GreenNode Node = node;

		public int SlotIndex = -1;

		public bool ProcessedDiagnostics = false;
	}

	private struct NodeIterationStack(int capacity) : IDisposable
	{
		private NodeIteration[] _stack = ArrayPool<NodeIteration>.Shared.Rent(capacity);

		private int _count = 0;

		public readonly ref NodeIteration Top => ref _stack[_count - 1];

		public readonly void Dispose()
		{
			ArrayPool<NodeIteration>.Shared.Return(_stack, clearArray: true);
		}

		public void PushNodeOrToken(GreenNode node)
		{
			if (node is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token)
			{
				PushToken(token);
			}
			else
			{
				Push(node);
			}
		}

		private void PushToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token)
		{
			Push(token.GetTrailingTrivia());
			Push(token);
			Push(token.GetLeadingTrivia());
		}

		private void Push(GreenNode? node)
		{
			if (node != null)
			{
				if (_count >= _stack.Length)
				{
					NodeIteration[] array = ArrayPool<NodeIteration>.Shared.Rent(_stack.Length * 2);
					Array.Copy(_stack, array, _stack.Length);
					ArrayPool<NodeIteration>.Shared.Return(_stack, clearArray: true);
					_stack = array;
				}
				_stack[_count] = new NodeIteration(node);
				_count++;
			}
		}

		public void Pop()
		{
			_count--;
		}

		public readonly bool Any()
		{
			return _count > 0;
		}
	}

	private const int DefaultStackCapacity = 8;

	public static IEnumerable<Diagnostic> EnumerateDiagnostics(SyntaxTree syntaxTree, GreenNode root, int position)
	{
		NodeIterationStack stack = new NodeIterationStack(8);
		try
		{
			stack.PushNodeOrToken(root);
			int fullTreeLength = syntaxTree.GetRoot().FullSpan.Length;
			while (stack.Any())
			{
				GreenNode node = stack.Top.Node;
				if (!stack.Top.ProcessedDiagnostics)
				{
					DiagnosticInfo[] diagnostics = node.GetDiagnostics();
					for (int i = 0; i < diagnostics.Length; i++)
					{
						SyntaxDiagnosticInfo syntaxDiagnosticInfo = (SyntaxDiagnosticInfo)diagnostics[i];
						int num = ((!node.IsToken) ? node.GetLeadingTriviaWidth() : 0);
						int num2 = Math.Min(position + num + syntaxDiagnosticInfo.Offset, fullTreeLength);
						int end = Math.Min(num2 + syntaxDiagnosticInfo.Width, fullTreeLength);
						yield return new CSDiagnostic(syntaxDiagnosticInfo, new SourceLocation(syntaxTree, TextSpan.FromBounds(num2, end)));
					}
					stack.Top.ProcessedDiagnostics = true;
				}
				processNode(node);
			}
		}
		finally
		{
			((IDisposable)stack/*cast due to constrained. prefix*/).Dispose();
			void processNode(GreenNode greenNode)
			{
				if (greenNode.SlotCount == 0)
				{
					position += greenNode.Width;
				}
				else
				{
					for (int j = stack.Top.SlotIndex + 1; j < greenNode.SlotCount; j++)
					{
						GreenNode slot = greenNode.GetSlot(j);
						if (slot != null)
						{
							if (slot.ContainsDiagnostics)
							{
								stack.Top.SlotIndex = j;
								stack.PushNodeOrToken(slot);
								return;
							}
							position += slot.FullWidth;
						}
					}
				}
				stack.Pop();
			}
		}
	}
}
