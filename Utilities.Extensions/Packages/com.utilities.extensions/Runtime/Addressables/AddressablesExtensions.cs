// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_ADDRESSABLES && UTILITIES_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities.Async;
using Utilities.Async.Addressables;

namespace Utilities.Extensions
{
    public static class AddressablesExtensions
    {
        public static void Release<T>(this AsyncOperationHandle<T> handle)
            => Release((AsyncOperationHandle)handle);

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
        /// <param name="key">Path.</param>
        /// <param name="progress">Optional, <see cref="IProgress{T}"/></param>
        public static async Task DownloadAddressableAsync(object key, IProgress<float> progress = null)
        {
            var downloadSizeOp = Addressables.GetDownloadSizeAsync(key);
            var downloadSize = await downloadSizeOp;
            downloadSizeOp.Release();

            if (downloadSize > 0)
            {
                await Addressables.DownloadDependenciesAsync(key).AwaitWithProgress(progress);
            }
        }

        /// <summary>
        /// Wait on the <see cref="AsyncOperationHandle{T}"/> with the provided <see cref="IProgress{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="progress"></param>
        /// <param name="autoRelease"></param>
        public static async Task<T> AwaitWithProgress<T>(this AsyncOperationHandle<T> operation, IProgress<float> progress, bool autoRelease = true)
        {
            Thread backgroundThread = null;

            if (progress != null)
            {
                backgroundThread = new Thread(() => ProgressThread(operation, progress))
                {
                    IsBackground = true
                };
            }

            backgroundThread?.Start();
            var result = await operation.Task;
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

            return result;
        }

        /// <summary>
        /// Wait on the <see cref="AsyncOperationHandle"/> with the provided <see cref="IProgress{T}"/>
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="progress"></param>
        /// <param name="autoRelease"></param>
        public static async Task AwaitWithProgress(this AsyncOperationHandle operation, IProgress<float> progress, bool autoRelease = true)
        {
            Thread backgroundThread = null;

            if (progress != null)
            {
                backgroundThread = new Thread(() => ProgressThread(operation, progress))
                {
                    IsBackground = true
                };
            }

            backgroundThread?.Start();
            await operation.Task;
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

        private static async void ProgressThread(AsyncOperationHandle handle, IProgress<float> progress)
        {
            await Awaiters.UnityMainThread;

            try
            {
                while (handle.IsValid() && !handle.IsDone)
                {
                    if (handle.OperationException != null)
                    {
                        break;
                    }

                    progress.Report(handle.PercentComplete * 100f);
                    await Awaiters.UnityMainThread;
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
