using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal static class ResultFactory
    {
        public static IResult<TData, TError> SuccessResult<TData, TError>(TData data)
            => new Result<TData, TError>(true, data, default);

        public static IResult<TData, TError> ErrorResult<TData, TError>(TError error)
            => new Result<TData, TError>(false, default, error);

        public static IResultWithoutData<TError> SuccessResultWithoutData<TError>()
            => new ResultWithoutData<TError>(true, default);

        public static IResultWithoutData<TError> ErrorResultWithoutData<TError>(TError error)
            => new ResultWithoutData<TError>(false, error);

        public static IResultWithoutData SuccessResultWithoutData()
            => new ResultWithoutData(true, null);

        public static IResultWithoutData ErrorResultWithoutData(Exception exception)
            => new ResultWithoutData(false, exception);

        public static IResult<TData> SuccessResult<TData>(TData data)
            => new Result<TData>(true, data, null);

        public static IResult<TData> ErrorResult<TData>(Exception exception)
            => new Result<TData>(false, default, exception);

        private record Result<TData, TError>(bool IsSuccess, TData Data, TError Error) : IResult<TData, TError>
        {
            public virtual TData GetData() => this.Data;

            public virtual TError GetError() => this.Error;
        }

        private record Result<TData>(bool IsSuccess, TData Data, Exception Exception)
            : Result<TData, Exception>(IsSuccess, Data, Exception), IResult<TData>;

        private record ResultWithoutData<TError>(bool IsSuccess, TError Error)
            : Result<object, TError>(IsSuccess, null, Error), IResultWithoutData<TError>
        {
            public override object GetData() => throw new NotSupportedException("No data");
        }

        private record ResultWithoutData(bool IsSuccess, Exception Exception)
            : ResultWithoutData<Exception>(IsSuccess, Exception), IResultWithoutData;
    }
}
