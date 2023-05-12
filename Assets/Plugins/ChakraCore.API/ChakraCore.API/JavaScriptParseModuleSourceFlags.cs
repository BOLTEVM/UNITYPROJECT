namespace ChakraCore.API {
  /// <summary>
  ///     Flags for parsing a module.
  /// </summary>
  public enum JavaScriptParseModuleSourceFlags {
    /// <summary>
    ///     Module source is UTF16.
    /// </summary>
    JsParseModuleSourceFlags_DataIsUTF16LE = 0x00000000,

    /// <summary>
    ///     Module source is UTF8.
    /// </summary>
    JsParseModuleSourceFlags_DataIsUTF8 = 0x00000001
  }
}
