namespace Utilities.Extensions
{
#if !UNITY_2022_2_OR_NEWER
    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public static class NativeArrayExtensions
    {
        public static unsafe ReadOnlySpan<T> AsSpan<T>(this NativeArray<T> nativeArray) where T : unmanaged
            => new(nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
    }
#endif
}
