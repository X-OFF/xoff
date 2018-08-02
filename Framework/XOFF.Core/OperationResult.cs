using System;
using System.IO;

namespace XOFF.Core
{

    /// <summary>
    /// </summary>
    /// <typeparam name="TResult"></typeparam>

    public class XOFFOperationResult<TResult>
    {
        private XOFFOperationResult()
        {
        }

        public bool Success { get;  private set; }
        public TResult Result { get;  set; }
        public string Message { get;  set; }
        public Exception Exception { get; private set; }

        public static XOFFOperationResult<TResult> CreateSuccessResult(TResult result)
        {
            return new XOFFOperationResult<TResult> { Success = true, Result = result };
        }

        public static XOFFOperationResult<TResult> CreateFailure(string nonSuccessMessage)
        {
            return new XOFFOperationResult<TResult> { Success = false, Message = nonSuccessMessage };
        }

        public static XOFFOperationResult<TResult> CreateFailure(Exception ex)
        {
            return new XOFFOperationResult<TResult>
            {
                Success = false,
                Message = String.Format("{0}{1}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace),
                Exception = ex
            };
        }

        public XOFFOperationResult ToOperationResult()
        {
            return new XOFFOperationResult()
            {
                Success = this.Success,
                Exception = this.Exception,
                Message = this.Message
            };
        }

        public static XOFFOperationResult<TResult> CreateFailure(string message, Exception exception)
        {
            return new XOFFOperationResult<TResult> { Success = false, Message = message, Exception = exception };
        }
    }

    /*
      This class has been borrowed and adapted from https://www.codeproject.com/Articles/1022462/Error-Handling-in-SOLID-Csharp-NET-The-Operation-R
      This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)       
      */

}

public class XOFFOperationResult
{
    public bool Success { get; set; }
    public string Message { get; internal set; }
    public Exception Exception { get; internal set; }

    public static XOFFOperationResult CreateSuccessResult(string message = "Success")
    {
        return new XOFFOperationResult { Success = true, Message = message };
    }

    public static XOFFOperationResult CreateFailure(string message, Exception exception)
	{
        return new XOFFOperationResult { Success = false, Message = message, Exception = exception};
    }

    public static XOFFOperationResult CreateFailure(string message)
    {
        return new XOFFOperationResult { Success = false, Message = message };
    }
    public static XOFFOperationResult CreateFailure(Exception ex)
    {

        if (ex != null)
        {
            return new XOFFOperationResult
            {
                Success = false,
                Message = String.Format("{0}{1}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace),
                Exception = ex
            };
        }
        return CreateFailure("");

    }

    /*
      This class has been borrowed and adapted from https://www.codeproject.com/Articles/1022462/Error-Handling-in-SOLID-Csharp-NET-The-Operation-R
      This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)       
      */

}
