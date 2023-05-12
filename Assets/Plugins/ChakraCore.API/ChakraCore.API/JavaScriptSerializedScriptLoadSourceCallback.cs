namespace ChakraCore.API {
  /// <summary>
  ///     Called by the runtime to load the source code of the serialized script.
  ///     The caller must keep the script buffer valid until the JsSerializedScriptUnloadCallback.
  ///     This callback is only supported by the Win32 version of the API
  /// </summary>
  /// <param name="sourceContext">The context passed to Js[Parse|Run]SerializedScriptWithCallback</param>
  /// <param name="scriptBuffer">The script returned.</param>
  /// <returns>
  ///     true if the operation succeeded, false otherwise.
  /// </returns>
  // typedef bool (CHAKRA_CALLBACK * JsSerializedScriptLoadSourceCallback)(_In_ JsSourceContext sourceContext, _Outptr_result_z_ const WCHAR** scriptBuffer);
  public delegate bool JavaScriptSerializedScriptLoadSourceCallback(
    JavaScriptSourceContext sourceContext,
    out string scriptBuffer
  );
}
