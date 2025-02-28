namespace System.Runtime.CompilerServices
{
#if NETFRAMEWORK
    /// <inheritdoc cref="CallerArgumentExpressionAttribute(string)"/>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class CallerArgumentExpressionAttribute : Attribute
    {
        /// <summary> Apply this to an optional string Parameter to capture the Expression string for </summary> 
        /// <param name="parameterName">the name of the other Parameter to capture the Expression string for. </param>
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        /// <summary> Name of the Parameter to capture the Expression string for </summary>
        public string ParameterName { get; }
    }
#endif
}