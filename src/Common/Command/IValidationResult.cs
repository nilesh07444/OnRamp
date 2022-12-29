namespace Common.Command
{
    public interface IValidationResult
    {
        /// <summary>
        ///     Gets or sets the name of the member.
        /// </summary>
        /// <value>
        ///     The name of the member.  May be null for general validation issues.
        /// </value>
        string MemberName { get; set; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        string Message { get; set; }
    }
}