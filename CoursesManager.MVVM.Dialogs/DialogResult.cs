namespace CoursesManager.MVVM.Dialogs;

/// <summary>
/// Represents the result of a dialog operation, including the outcome, an optional message, and any associated data.
/// </summary>
/// <typeparam name="TDialogResultType">The type of the data returned by the dialog.</typeparam>
public sealed class DialogResult<TDialogResultType>
{
    /// <summary>
    /// Gets the outcome of the dialog, such as Success, Failure, or Canceled.
    /// </summary>
    public DialogOutcome Outcome { get; private set; }

    /// <summary>
    /// Gets the optional message describing the outcome.
    /// </summary>
    public string? OutcomeMessage { get; private set; }

    /// <summary>
    /// Gets the data returned by the dialog.
    /// </summary>
    public TDialogResultType? Data { get; private set; }

    // Private constructor to force use of the builder.
    private DialogResult()
    { }


    /// <summary>
    /// Creates and returns a new builder for constructing a <see cref="DialogResult{TDialogResultType}"/>.
    /// </summary>
    public static DialogResultBuilder Builder() => new();

    /// <summary>
    /// Provides a fluent API for building a <see cref="DialogResult{TDialogResultType}"/> instance.
    /// </summary>
    public class DialogResultBuilder
    {
        private readonly DialogResult<TDialogResultType> _dialogResult = new();

        /// <summary>
        /// Sets the outcome of the dialog.
        /// </summary>
        /// <param name="outcome">The outcome of the dialog.</param>
        /// <returns>The current <see cref="DialogResultBuilder"/> instance.</returns>
        public DialogResultBuilder SetOutcome(DialogOutcome outcome)
        {
            _dialogResult.Outcome = outcome;
            return this;
        }

        /// <summary>
        /// Sets the data returned by the dialog.
        /// </summary>
        /// <param name="data">The data returned by the dialog.</param>
        /// <returns>The current <see cref="DialogResultBuilder"/> instance.</returns>
        public DialogResultBuilder SetData(TDialogResultType data)
        {
            _dialogResult.Data = data;
            return this;
        }

        /// <summary>
        /// Sets the outcome message describing the result.
        /// </summary>
        /// <param name="outcomeMessage">The message describing the outcome.</param>
        /// <returns>The current <see cref="DialogResultBuilder"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="outcomeMessage"/> is null or whitespace.</exception>
        public DialogResultBuilder SetOutcomeMessage(string outcomeMessage)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(outcomeMessage);
            _dialogResult.OutcomeMessage = outcomeMessage;
            return this;
        }

        /// <summary>
        /// Sets the dialog result as a success with the given data and an optional outcome message.
        /// </summary>
        /// <param name="data">The data returned by the dialog.</param>
        /// <param name="outcomeMessage">An optional message describing the successful outcome.</param>
        /// <returns>The current <see cref="DialogResultBuilder"/> instance.</returns>
        public DialogResultBuilder SetSuccess(TDialogResultType data, string? outcomeMessage = null)
        {
            _dialogResult.Data = data;
            _dialogResult.OutcomeMessage = outcomeMessage;
            _dialogResult.Outcome = DialogOutcome.Success;
            return this;
        }

        /// <summary>
        /// Sets the dialog result as a failure with the given outcome message.
        /// </summary>
        /// <param name="outcomeMessage">The message describing the failure.</param>
        /// <returns>The current <see cref="DialogResultBuilder"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="outcomeMessage"/> is null or whitespace.</exception>
        public DialogResultBuilder SetFailure(string outcomeMessage)
        {
            SetOutcomeMessage(outcomeMessage);
            _dialogResult.Outcome = DialogOutcome.Failure;
            return this;
        }

        /// <summary>
        /// Sets the dialog result as canceled with the given outcome message.
        /// </summary>
        /// <param name="outcomeMessage">The message describing the cancellation.</param>
        /// <returns>The current <see cref="DialogResultBuilder"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="outcomeMessage"/> is null or whitespace.</exception>
        public DialogResultBuilder SetCanceled(string outcomeMessage)
        {
            SetOutcomeMessage(outcomeMessage);
            _dialogResult.Outcome = DialogOutcome.Canceled;
            return this;
        }

        /// <summary>
        /// Builds and returns the constructed <see cref="DialogResult{TDialogResultType}"/> instance.
        /// </summary>
        /// <returns>A <see cref="DialogResult{TDialogResultType}"/> representing the dialog's result.</returns>
        public DialogResult<TDialogResultType> Build()
        {
            return _dialogResult;
        }

        
    }
}