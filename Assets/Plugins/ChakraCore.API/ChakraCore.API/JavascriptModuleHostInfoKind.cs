namespace ChakraCore.API {
  /// <summary>
  ///     The types of host info that can be set on a module record with JsSetModuleHostInfo.
  /// </summary>
  /// <remarks>
  ///     For more information see JsSetModuleHostInfo.
  /// </remarks>
  public enum JavascriptModuleHostInfoKind {
    /// <summary>
    ///     An exception object - e.g. if the module file cannot be found.
    /// </summary>
    JsModuleHostInfo_Exception = 0x01,

    /// <summary>
    ///     Host defined info.
    /// </summary>
    JsModuleHostInfo_HostDefined = 0x02,

    /// <summary>
    ///     Callback for receiving notification when module is ready.
    /// </summary>
    JsModuleHostInfo_NotifyModuleReadyCallback = 0x3,

    /// <summary>
    ///     Callback for receiving notification to fetch a dependent module.
    /// </summary>
    JsModuleHostInfo_FetchImportedModuleCallback = 0x4,

    /// <summary>
    ///     Callback for receiving notification for calls to ```import()```
    /// </summary>
    JsModuleHostInfo_FetchImportedModuleFromScriptCallback = 0x5,

    /// <summary>
    ///     URL for use in error stack traces and debugging.
    /// </summary>
    JsModuleHostInfo_Url = 0x6,
  }
}
