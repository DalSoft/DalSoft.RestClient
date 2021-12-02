using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient
{
    public static class Verify
    {
        public static Expression<Func<TResponse, bool>> Expression<TResponse>(Expression<Func<TResponse, bool>> verify) where TResponse : class
        {
            return verify;
        }

        public static Func<dynamic, bool> Expression(Func<dynamic, bool> verify)
        {
            return verify;
        }

        public static string FailedMessage<TResponse>(Expression<Func<TResponse, bool>> verify) where TResponse : class
        {
            return $"{verify} was not verified";
        }

        public static string FailedMessage(Func<dynamic, bool> verify)
        {
            return $"dynamic {verify.Method.GetParameters().FirstOrDefault()?.Name} => ... was not verified";
        }
    }

    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
    public static class RestClientExtensions
    {
        private const string ThrowOnExceptionKey = "DalSoft.RestClient.RestClientExtensions.ThrowOnException";

        public static Task<dynamic> Verify<TResponse>(this Task<dynamic> request, Expression<Func<TResponse, bool>> verify) where TResponse : class 
            => Verify(request, verify, CancellationToken.None);
        
        public static Task<dynamic> Verify<TResponse>(this Task<dynamic> request, 
            Expression<Func<TResponse, bool>> verify, 
            CancellationToken cancellationToken = default, 
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null) where TResponse : class
        {
            var result = request.AsyncState ?? request.Result; // In the case of a faulted task and use the first to verify the result

            dynamic ContinuationFunction(Task<dynamic> task, dynamic state)
            {
                var response = typeof(TResponse) == typeof(string) ? state?.ToString() : (TResponse) state;

                if (verify == null || verify.Compile()(response) == true)
                {
                    if (task.IsFaulted)
                        throw task.Exception.ToFlatAggregateException();

                    return state;
                }
                
                var message = DalSoft.RestClient.Verify.FailedMessage(verify);
                var verifiedFailed = new VerifiedFailed(message);

                throw task.Exception.ToFlatAggregateException(verifiedFailed);
            }

            return request.ContinueWith
            (
                continuationFunction:(Func<Task<dynamic>, object, object>)((task, state) => ContinuationFunction(task, state)) ,
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler:scheduler ?? TaskScheduler.Default,
                state: result
            );
        }

        public static Task<dynamic> Verify(this Task<dynamic> request, Func<dynamic, bool> verify) 
            => Verify(request, verify, CancellationToken.None);

        public static Task<dynamic> Verify(this Task<dynamic> request,
            Func<dynamic, bool> verify,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null)
        {
            var result = request.AsyncState ?? request.Result; // In the case of a faulted task and use the first to verify the result

            dynamic ContinuationFunction(Task<dynamic> task, dynamic state)
            {
                if (verify == null || verify(state) == true)
                {
                    if (task.IsFaulted)
                        throw task.Exception.ToFlatAggregateException();

                    return state;
                }

                var message = DalSoft.RestClient.Verify.FailedMessage(verify);
                var verifiedFailed = new VerifiedFailed(message);

                throw task.Exception.ToFlatAggregateException(verifiedFailed);
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>) ((task, state) => ContinuationFunction(task, state)) ,
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: result
            );
        }

        /// <summary>Add to the end of the continuation chain</summary>
        public static Task<dynamic> OnException<TResponse>(this Task<dynamic> request, Action<AggregateException, TResponse> onException) where TResponse : class
            => OnException(request, onException, false, CancellationToken.None);

        /// <summary>Add to the end of the continuation chain, pass throwException true to throw the exception or to use multiple OnException continuations</summary>
        public static Task<dynamic> OnException<TResponse>(this Task<dynamic> request, Action<AggregateException, TResponse> onException, bool throwException) where TResponse : class
            => OnException(request, onException, throwException, CancellationToken.None);

        public static Task<dynamic> OnException<TResponse>(this Task<dynamic> request,
            Action<AggregateException, TResponse> onException,
            bool throwException = false,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null) where TResponse : class
        {
            var result = request.AsyncState ?? request.Result; // In the case of a faulted task and use the first to verify the result

            dynamic ContinuationFunction(Task<dynamic> task, dynamic state)
            {
                var flatAggregateException = task.Exception.ToFlatAggregateException(throwOnException:throwException);

                if (onException != null && task.IsFaulted)
                {
                    var response = typeof(TResponse) == typeof(string) ? state?.ToString() : (TResponse)state;

                    onException(flatAggregateException, response);
                }

                if (throwException)
                    throw flatAggregateException;
                
                return state;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>) ((task, state) => ContinuationFunction(task, state)) ,
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: result
            );
        }

        /// <summary>Add to the end of the continuation chain</summary>
        public static Task<dynamic> OnException(this Task<dynamic> request, Action<AggregateException, dynamic> onException)
            => OnException(request, onException, false, CancellationToken.None);

        /// <summary>Add to the end of the continuation chain, pass throwException true to throw the exception or to use multiple OnException continuations</summary>
        public static Task<dynamic> OnException(this Task<dynamic> request, Action<AggregateException, dynamic> onException, bool throwException)
            => OnException(request, onException, throwException, CancellationToken.None);

        public static Task<dynamic> OnException(this Task<dynamic> request,
            Action<AggregateException, dynamic> onException,
            bool throwException = false,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null)
        {
            var result = request.AsyncState ?? request.Result; // In the case of a faulted task and use the first to verify the result

            dynamic ContinuationFunction(Task<dynamic> task, object state)
            {
                var flatAggregateException = task.Exception.ToFlatAggregateException(throwOnException:throwException);

                if (onException != null && task.IsFaulted)
                    onException(flatAggregateException, result);
                
                if (throwException)
                    throw flatAggregateException;

                return state;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>) ((task, state) =>  ContinuationFunction(task, state)) ,
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: result
            );
        }

        public static Task<T> As<T>(this Task<dynamic> request,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null) where T : class
        {

            T ContinuationFunction(Task<dynamic> task)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                return (T)task.Result; 
            }

            return request.ContinueWith
            (
                continuationFunction: task => ContinuationFunction(task),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default
            );
        }

        public static Task<TTo> Map<TFrom, TTo>(this Task<dynamic> request, 
            Func<TFrom, TTo> map,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null) where TFrom : class where TTo : class
        {
            TTo ContinuationFunction(Task<dynamic> task)
            {
                if (map == null) throw new ArgumentNullException(nameof(map));

                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                TFrom from = task.Result;

                return map(from);
            }

            return request.ContinueWith
            (
                continuationFunction: task => ContinuationFunction(task),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default
            );
        }

        public static Task<TTo> Map<TTo>(this Task<dynamic> request, 
            Func<dynamic, TTo> map,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null) where TTo : class
        {
            TTo ContinuationFunction(Task<dynamic> task)
            {
                if (map == null) throw new ArgumentNullException(nameof(map));

                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                TTo to = map(task.Result);

                return to;
            }

            return request.ContinueWith
            (
                continuationFunction: task => ContinuationFunction(task),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default
            );
        }

        public static Task<dynamic> Act<TResponse>(this Task<dynamic> request,
          Action<TResponse> act,
          CancellationToken cancellationToken = default,
          TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
          TaskScheduler scheduler = null
          ) where TResponse : class
        {
            var result = request.AsyncState ?? request.Result; // In the case of a faulted task and use the first to verify the result

            dynamic ContinuationFunction(Task<dynamic> task, object state)
            {
                if (act == null) throw new ArgumentNullException(nameof(act));

                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                TResponse response = task.Result;
                act(response);

                return state;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>)((task, state) => ContinuationFunction(task, state)),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: result
            );
        }

        public static Task<dynamic> Act(this Task<dynamic> request,
            Action<dynamic> act,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null)
        {
            var result = request.AsyncState ?? request.Result; // In the case of a faulted task and use the first to verify the result

            dynamic ContinuationFunction(Task<dynamic> task, object state)
            {
                if (act == null) throw new ArgumentNullException(nameof(act));

                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                act(task.Result);

                return state;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>)((task, state) => ContinuationFunction(task, state)),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: result
            );
        }

        private static AggregateException ToFlatAggregateException(this AggregateException aggregateException, Exception currentException = null, bool throwOnException = false)
        {
            var flatAggregateExceptions = new List<Exception>(new List<Exception>(aggregateException?.Flatten().InnerExceptions.ToList() ?? new List<Exception>()));

            if (currentException != null)
                flatAggregateExceptions.Add(currentException);

            var flatAggregateException = new AggregateException(flatAggregateExceptions);

            if (throwOnException)
                flatAggregateException.Data.Add(ThrowOnExceptionKey, true);

            return flatAggregateException;
        }
    }

    public class VerifiedFailed : Exception
    {
        public VerifiedFailed(string message) : base(message) { }
    }
}