using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IResult<TData> : IResult<TData, string> { }

    internal interface IResult<TData, TError>
    {
        bool IsSuccess { get; }

        TData GetData();

        TError GetError();
    }
}
