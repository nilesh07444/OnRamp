namespace Common.Command
{
    /// <summary>
    ///     Describes the result of a validation of a potential change through a business service.
    /// </summary>
    public class ValidationResult : IValidationResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationResult" /> class.
        /// </summary>
        public ValidationResult()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationResult" /> class.
        /// </summary>
        /// <param name="memeberName">Name of the memeber.</param>
        /// <param name="message">The message.</param>
        public ValidationResult(string memeberName, string message)
        {
            MemberName = memeberName;
            Message = message;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationResult" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ValidationResult(string message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets or sets the name of the member.
        /// </summary>
        /// <value>
        ///     The name of the member.  May be null for general validation issues.
        /// </value>
        public string MemberName { get; set; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{MemberName} : {Message}";
        }
    }
}