using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A callback called before collection.
  /// </summary>
  /// <remarks>
  ///     Use <c>JsSetBeforeCollectCallback</c> to register this callback.
  /// </remarks>
  /// <param name="callbackState">The state passed to <c>JsSetBeforeCollectCallback</c>.</param>
  public delegate void JavaScriptBeforeCollectCallback(
    IntPtr callbackState
  );
}
