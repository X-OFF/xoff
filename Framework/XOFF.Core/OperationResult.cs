using System;

namespace XOFF.Core
{

    /// <summary>
    /// </summary>
    /// <typeparam name="TResult"></typeparam>

    public class OperationResult<TResult>
    {
        private OperationResult()
        {
        }

        public bool Success { get; private set; }
        public TResult Result { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        public static OperationResult<TResult> CreateSuccessResult(TResult result)
        {
            return new OperationResult<TResult> { Success = true, Result = result };
        }

        public static OperationResult<TResult> CreateFailure(string nonSuccessMessage)
        {
            return new OperationResult<TResult> { Success = false, Message = nonSuccessMessage };
        }

        public static OperationResult<TResult> CreateFailure(Exception ex)
        {
            return new OperationResult<TResult>
            {
                Success = false,
                Message = String.Format("{0}{1}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace),
                Exception = ex
            };
        }
    }

    /*
      This class has been borrowed and adapted from https://www.codeproject.com/Articles/1022462/Error-Handling-in-SOLID-Csharp-NET-The-Operation-R
      This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)       
      */

}

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; private set; }
    public Exception Exception { get; private set; }

    public static OperationResult CreateSuccessResult(string message = "")
    {
        return new OperationResult { Success = true, Message = message };
    }

    public static OperationResult CreateFailure(string message)
    {
        return new OperationResult { Success = false, Message = message };
    }
    public static OperationResult CreateFailure(Exception ex)
    {
        return new OperationResult
        {
            Success = false,
            Message = String.Format("{0}{1}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace),
            Exception = ex
        };
    }

    /*
      This class has been borrowed and adapted from https://www.codeproject.com/Articles/1022462/Error-Handling-in-SOLID-Csharp-NET-The-Operation-R
      This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)       
      */
}