using System.Threading;
using System.Threading.Tasks;

namespace OPI.Core.Validators;

public abstract record ModelValidatorBase
{
    public ModelValidatorBase()
    {
        Reason = $"Is {nameof(ValidateAsync)} called yet?";
    }

    /// <summary>
    /// Validates the model.
    /// </summary>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken)
    {
        bool result = await ValidateAsyncImp(cancellationToken);
        result = result && ValidateImp();
        return result;
    }

    /// <summary>
    /// The reason when the validation failed.
    /// </summary>
    public string Reason { get; protected set; }

    protected virtual Task<bool> ValidateAsyncImp(CancellationToken cancellationToken) => Task.FromResult(true);
    protected abstract bool ValidateImp();
}
