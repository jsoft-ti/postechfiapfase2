namespace Application.Dto.Result;

public class OperationResult
{
    public bool Succeeded { get; protected set; }
    public IReadOnlyList<string> Errors { get; protected set; } = new List<string>();

    public static OperationResult Success()
    {
        return new OperationResult { Succeeded = true };
    }

    public static OperationResult Failure(params string[] errors)
    {
        return new OperationResult
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }

    public void AddError(string error)
    {
        var errors = Errors.ToList();
        errors.Add(error);
        Errors = errors;
        Succeeded = false;
    }
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; private set; }

    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T> { Succeeded = true, Data = data };
    }

    public static new OperationResult<T> Failure(params string[] errors)
    {
        return new OperationResult<T>
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }
}