using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A finalizer callback.
  /// </summary>
  /// <param name="data">
  ///     The external data that was passed in when creating the object being finalized.
  /// </param>
  public delegate void JavaScriptFinalizeCallback(
    IntPtr data
  );
}
