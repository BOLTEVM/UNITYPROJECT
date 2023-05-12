namespace ChakraCore.API {
  /// <summary>
  ///     Called by the runtime to load the source code of the serialized script.
  /// </summary>
  /// <param name="sourceContext">The context passed to Js[Parse|Run]SerializedScriptCallback</param>
  /// <param name="value">The script returned.</param>
  /// <param name="parseAttributes">Parse Attributes returned</param>
  /// <returns>
  ///     true if the operation succeeded, false otherwise.
  /// </returns>
  /// <remarks>
  ///     <c>This API is experimental and may have breaking change later.</c>
  ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked until the callback completes.
  ///     The callback can be used by hosts to prepare for garbage collection.
  ///     For example, by releasing unnecessary references on Chakra objects.
  /// </remarks>
  public delegate bool JavaScriptSerializedLoadScriptCallback(
    JavaScriptSourceContext sourceContext,
    out JavaScriptValue value,
    out JavaScriptParseScriptAttributes parseAttributes
  );
}
