using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IResult<out TData> : IResult<TData, Exception> { }
    internal interface IResultWithoutData<out TError> : IResult<object, TError> { }
    internal interface IResultWithoutData : IResultWithoutData<Exception> { }

    internal interface IResult<out TData, out TError>
    {
        bool IsSuccess { get; }

        TData GetData();

        TError GetError();
    }
}
