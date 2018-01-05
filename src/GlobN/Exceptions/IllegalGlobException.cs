using System;

namespace Acklann.GlobN.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a <see cref="Glob"/> pattern is not well-formed.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class IllegalGlobException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalGlobException"/> class.
        /// </summary>
        public IllegalGlobException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalGlobException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IllegalGlobException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalGlobException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public IllegalGlobException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalGlobException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected IllegalGlobException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}