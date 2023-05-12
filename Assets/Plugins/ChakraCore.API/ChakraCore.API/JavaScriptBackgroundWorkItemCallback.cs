using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A background work item callback.
  /// </summary>
  /// <remarks>
  ///     This is passed to the host's thread service (if provided) to allow the host to
  ///     invoke the work item callback on the background thread of its choice.
  /// </remarks>
  /// <param name="callbackState">Data argument passed to the thread service.</param>
  public delegate void JavaScriptBackgroundWorkItemCallback(
    IntPtr callbackData
  );
}
