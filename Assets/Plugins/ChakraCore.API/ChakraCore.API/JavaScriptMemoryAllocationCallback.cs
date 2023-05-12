using System;

namespace ChakraCore.API {
  /// <summary>
  ///     User implemented callback routine for memory allocation events
  /// </summary>
  /// <remarks>
  ///     Use <c>JsSetRuntimeMemoryAllocationCallback</c> to register this callback.
  /// </remarks>
  /// <param name="callbackState">
  ///     The state passed to <c>JsSetRuntimeMemoryAllocationCallback</c>.
  /// </param>
  /// <param name="allocationEvent">The type of type allocation event.</param>
  /// <param name="allocationSize">The size of the allocation.</param>
  /// <returns>
  ///     For the <c>JsMemoryAllocate</c> event, returning <c>true</c> allows the runtime to continue
  ///     with the allocation. Returning false indicates the allocation request is rejected. The
  ///     return value is ignored for other allocation events.
  /// </returns>
  public delegate bool JavaScriptMemoryAllocationCallback(
    IntPtr callbackState,
    JavaScriptMemoryEventType allocationEvent,
    UIntPtr allocationSize
  );
}
