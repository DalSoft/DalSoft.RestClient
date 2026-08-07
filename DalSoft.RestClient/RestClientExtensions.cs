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
            dynamic ContinuationFunction(Task<dynamic> task, dynamic state)
            {
                var responseState = ResolveStateObject(state) ?? ResolveContinuationState(task);
                var response = typeof(TResponse) == typeof(string) ? responseState?.ToString() : (TResponse)responseState;

                if (verify == null || verify.Compile()(response) == true)
                {
                    if (task.IsFaulted)
                        throw task.Exception.ToFlatAggregateException();

                    return responseState;
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
                state: GetContinuationState(request)
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
            dynamic ContinuationFunction(Task<dynamic> task, dynamic state)
            {
                var responseState = ResolveStateObject(state) ?? ResolveContinuationState(task);

                if (verify == null || verify(responseState) == true)
                {
                    if (task.IsFaulted)
                        throw task.Exception.ToFlatAggregateException();

                    return responseState;
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
                state: GetContinuationState(request)
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
            dynamic ContinuationFunction(Task<dynamic> task, dynamic state)
            {
                var responseState = ResolveStateObject(state) ?? ResolveContinuationState(task);
                var flatAggregateException = task.Exception.ToFlatAggregateException(throwOnException:throwException);

                if (onException != null && task.IsFaulted)
                {
                    var response = typeof(TResponse) == typeof(string) ? responseState?.ToString() : (TResponse)responseState;

                    onException(flatAggregateException, response);
                }

                if (throwException)
                    throw flatAggregateException;
                 
                return responseState;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>) ((task, state) => ContinuationFunction(task, state)) ,
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: GetContinuationState(request)
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
            dynamic ContinuationFunction(Task<dynamic> task, object state)
            {
                var responseState = ResolveStateObject(state) ?? ResolveContinuationState(task);
                var flatAggregateException = task.Exception.ToFlatAggregateException(throwOnException:throwException);

                if (onException != null && task.IsFaulted)
                    onException(flatAggregateException, responseState);
                 
                if (throwException)
                    throw flatAggregateException;

                return responseState;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>) ((task, state) =>  ContinuationFunction(task, state)) ,
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: GetContinuationState(request)
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
            dynamic ContinuationFunction(Task<dynamic> task, object state)
            {
                if (act == null) throw new ArgumentNullException(nameof(act));

                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                TResponse response = task.Result;
                act(response);

                return ResolveStateObject(state) ?? ResolveContinuationState(task);
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>)((task, state) => ContinuationFunction(task, state)),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: GetContinuationState(request)
            );
        }

        public static Task<dynamic> Act(this Task<dynamic> request,
            Action<dynamic> act,
            CancellationToken cancellationToken = default,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
            TaskScheduler scheduler = null)
        {
            dynamic ContinuationFunction(Task<dynamic> task, object state)
            {
                if (act == null) throw new ArgumentNullException(nameof(act));

                if (task.IsFaulted || task.IsCanceled)
                {
                    throw task.Exception.ToFlatAggregateException(throwOnException: true);
                }

                var responseState = ResolveStateObject(state) ?? ResolveContinuationState(task);
                act(responseState);

                return responseState;
            }

            return request.ContinueWith
            (
                continuationFunction: (Func<Task<dynamic>, object, object>)((task, state) => ContinuationFunction(task, state)),
                cancellationToken: cancellationToken,
                continuationOptions: continuationOptions,
                scheduler: scheduler ?? TaskScheduler.Default,
                state: GetContinuationState(request)
            );
        }

        private static object GetContinuationState(Task<dynamic> request)
        {
            return request?.AsyncState ?? (object)request;
        }

        private static dynamic ResolveContinuationState(Task<dynamic> task)
        {
            if (task == null)
                return null;

            if (task.AsyncState != null)
                return ResolveStateObject(task.AsyncState);

            if (task.Status == TaskStatus.RanToCompletion)
                return task.Result;

            return null;
        }

        private static object ResolveStateObject(object state)
        {
            if (!(state is Task stateTask))
                return state;

            if (stateTask.AsyncState != null)
                return ResolveStateObject(stateTask.AsyncState);

            if (stateTask.Status == TaskStatus.RanToCompletion)
            {
                var resultProperty = stateTask.GetType().GetProperty("Result");
                var taskResult = resultProperty?.GetValue(stateTask);
                return ResolveStateObject(taskResult);
            }

            return null;
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
