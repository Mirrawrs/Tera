namespace Tera
{
    /// <summary>
    ///     Exposes basic information about a system message type.
    /// </summary>
    public interface ISystemMessageTypeInfo
    {
        /// <summary>
        ///     Gets the system message type's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the system message type's formattable text.
        /// </summary>
        string Text { get; }
    }
}