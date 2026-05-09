// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_ADDRESSABLES && UTILITIES_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities.Async;

namespace Utilities.Extensions
{
    /// <summary>
    /// Extension methods and helpers for working with Unity Addressables operations.
    /// </summary>
    public static class AddressablesExtensions
    {
        /// <summary>
        /// Releases a typed async operation handle when valid.
        /// </summary>
        /// <typeparam name="T">The result type of the operation handle.</typeparam>
        /// <param name="handle">The handle to release.</param>
        public static void Release<T>(this AsyncOperationHandle<T> handle)
            => Release((AsyncOperationHandle)handle);

        /// <summary>
        /// Releases an async operation handle when valid.
        /// </summary>
        /// <param name="handle">The handle to release.</param>
        public static void Release(this AsyncOperationHandle handle)
        {
            if (handle.IsValid())
            {
                try
                {
                    Addressables.Release(handle);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>
        /// Checks cache, then downloads addressable if needed.
        /// </summary>
        /// <param name="key">The key or address identifying the addressable content.</param>
        /// <param name="progress">Optional progress reporter receiving percentage values from <c>0</c> to <c>100</c>.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that completes when any required download has finished.</returns>
        public static async Task DownloadAddressableAsync(object key, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var downloadSizeOp = Addressables.GetDownloadSizeAsync(key);
            long downloadSize;

            try
            {
                downloadSize = await downloadSizeOp.Task.WithCancellation(cancellationToken);
            }
            finally
            {
                downloadSizeOp.Release();
            }

            if (downloadSize > 0)
            {
                await Addressables.DownloadDependenciesAsync(key).AwaitWithProgress(progress, true, cancellationToken);
            }
        }

        /// <summary>
        /// Wait on the <see cref="AsyncOperationHandle{T}"/> with the provided <see cref="IProgress{T}"/>
        /// </summary>
        /// <typeparam name="T">The result type of the operation.</typeparam>
        /// <param name="operation"><see cref="AsyncOperationHandle{T}"/> to await.</param>
        /// <param name="progress">Optional progress reporter receiving percentage values from <c>0</c> to <c>100</c>.</param>
        /// <param name="autoRelease">Should the <see cref="AsyncOperationHandle{T}"/> be automatically released? Defaults to true.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The operation result.</returns>
        public static async Task<T> AwaitWithProgress<T>(this AsyncOperationHandle<T> operation, IProgress<float> progress, bool autoRelease = true, CancellationToken cancellationToken = default)
        {
            Thread backgroundThread = null;

            if (progress != null)
            {
                backgroundThread = new Thread(() => ProgressThread(operation, progress, cancellationToken))
                {
                    IsBackground = true
                };
            }

            T result;

            try
            {
                backgroundThread?.Start();
                result = await operation.Task.WithCancellation(cancellationToken);
            }
            finally
            {
                backgroundThread?.Join();
                progress?.Report(100f);

                var opException = operation.OperationException;

                if (autoRelease)
                {
                    operation.Release();
                }
                if (opException != null)
                {
                    throw opException;
                }
            }

            return result;
        }

        /// <summary>
        /// Wait on the <see cref="AsyncOperationHandle"/> with the provided <see cref="IProgress{T}"/>
        /// </summary>
        /// <param name="operation"><see cref="AsyncOperationHandle"/> to await.</param>
        /// <param name="progress">Optional progress reporter receiving percentage values from <c>0</c> to <c>100</c>.</param>
        /// <param name="autoRelease">Should the <see cref="AsyncOperationHandle"/> be automatically released? Defaults to true.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that completes when the operation finishes.</returns>
        public static async Task AwaitWithProgress(this AsyncOperationHandle operation, IProgress<float> progress, bool autoRelease = true, CancellationToken cancellationToken = default)
        {
            Thread backgroundThread = null;

            if (progress != null)
            {
                backgroundThread = new Thread(() => ProgressThread(operation, progress, cancellationToken))
                {
                    IsBackground = true
                };
            }

            try
            {
                backgroundThread?.Start();
                await operation.Task.WithCancellation(cancellationToken);
            }
            finally
            {
                backgroundThread?.Join();
                var opException = operation.OperationException;

                if (autoRelease)
                {
                    operation.Release();
                }

                if (opException != null)
                {
                    throw opException;
                }

                progress?.Report(100f);
            }
        }

        private static async void ProgressThread(AsyncOperationHandle handle, IProgress<float> progress, CancellationToken cancellationToken)
        {
            try
            {
                // ensure we're on main thread.
                await Awaiters.UnityMainThread;

                while (handle.IsValid() && !handle.IsDone && !cancellationToken.IsCancellationRequested)
                {
                    if (handle.OperationException != null)
                    {
                        break;
                    }

                    progress.Report(handle.PercentComplete * 100f);
                    await Task.Yield();
                }
            }
            catch (Exception)
            {
                // throw away
            }
        }
    }
}

#endif // UNITY_ADDRESSABLES && UTILITIES_ASYNC
