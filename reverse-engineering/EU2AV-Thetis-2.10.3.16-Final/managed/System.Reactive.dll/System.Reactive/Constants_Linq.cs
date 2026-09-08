namespace System.Reactive;

internal static class Constants_Linq
{
	public const string UseAsync = "This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.";

	public const string UseTaskFromAsyncPattern = "This conversion is no longer supported. Replace use of the Begin/End asynchronous method pair with a new Task-based async method, and convert the result using ToObservable. If no Task-based async method is available, use Task.Factory.FromAsync to obtain a Task object.";
}
