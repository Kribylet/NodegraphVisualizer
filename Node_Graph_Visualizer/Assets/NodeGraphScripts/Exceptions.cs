using System;

namespace Nodegraph_Generator
{
    /*
     * Exception for when operations are being preformed on an 
     * invalid plane. 
     */
    public class InvalidPlaneException : Exception
    {
        public InvalidPlaneException()
        {
        }

        public InvalidPlaneException(string message) 
            : base(message)
        {
        }

        public InvalidPlaneException(string message, Exception inner) 
            : base(message, inner)
        {
        }
    }

    /*
     * Exception for when trying make illegal calculations with the Zero vector.
     */
    public class ZeroVectorException : Exception
    {
        public ZeroVectorException() { }

        public ZeroVectorException(string message)
            : base(message) { }

        public ZeroVectorException(string message, Exception inner)
            : base(message, inner) { }
    }

    /*
    * Exception for when trying to add or get an object by index and that index is negative.
    */
    public class NegativeIndexException : Exception
    {
        public NegativeIndexException() { }
        public NegativeIndexException(string message) : base(message) { }
        public NegativeIndexException(string message, Exception inner)
            : base(message, inner) { }
    }

    /*
    * Exception for when trying to create or add a face that is not valid.
    */
    public class InvalidFaceException : Exception
    {
        public InvalidFaceException()
        {
        }

        public InvalidFaceException(string message) : base(message)
        {
        }

        public InvalidFaceException(string message, Exception inner) : base(message, inner)
        {
        }

    }

    /*
    * Exception for when trying to evaluate an empty list.
    */
    public class EmptyListException : Exception
    {
        public EmptyListException()
        {
        }

        public EmptyListException(string message) : base(message)
        {
        }

        public EmptyListException(string message, Exception inner) : base(message, inner)
        {
        }

    }

    /*
    * Exception for when trying to access an NodeGraph node that does not exist.
    */
    public class NullNodeException : Exception
    {
        public NullNodeException()
        {
        }

        public NullNodeException(string message) : base(message)
        {
        }

        public NullNodeException(string message, Exception inner) : base(message, inner)
        {
        }

    }

    /*
    * Exception when failing to invert a transformation matrix.
    */
    public class InvalidTransformException : Exception
    {
        public InvalidTransformException()
        {
        }

        public InvalidTransformException(string message) : base(message)
        {
        }

        public InvalidTransformException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
