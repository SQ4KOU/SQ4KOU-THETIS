namespace Discord.Interactions;

public interface IResult
{
	InteractionCommandError? Error { get; }

	string ErrorReason { get; }

	bool IsSuccess { get; }
}
