using System;

namespace Utilities.Common
{
    public class ResultWrapper<TResult>
    {
        public bool IsSuccess { get; set; }

        public TResult Result { get; set; }

        public Exception Exception { get; set; }
    }
}
