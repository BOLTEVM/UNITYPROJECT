namespace ChakraCore.API {
  /// <summary>
  ///     The possible states for a Promise object.
  /// </summary>
  public enum JavaScriptPromiseState {
    JsPromiseStatePending = 0x0,
    JsPromiseStateFulfilled = 0x1,
    JsPromiseStateRejected = 0x2,
  }
}
