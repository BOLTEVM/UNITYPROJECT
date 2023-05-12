namespace ChakraCore.API {
  /// <summary>
  ///     User implemented callback to fetch imported modules dynamically in scripts.
  /// </summary>
  /// <remarks>
  ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked untill
  ///     the callback completes. Notify the host to fetch the dependent module. This is used for the dynamic
  ///     import() syntax.
  ///
  ///     Callback should:
  ///     1. Check if the requested module has been requested before - if yes return the existing module record
  ///     2. If no create and initialize a new module record with JsInitializeModuleRecord to return and
  ///         schedule a call to JsParseModuleSource for the new record.
  /// </remarks>
  /// <param name="dwReferencingSourceContext">The referencing script context that calls import()</param>
  /// <param name="specifier">The specifier provided to the import() call.</param>
  /// <param name="dependentModuleRecord">The ModuleRecord of the dependent module. If the module was requested
  ///                                     before from other source, return the existing ModuleRecord, otherwise
  ///                                     return a newly created ModuleRecord.</param>
  /// <returns>
  ///     Returns <c>JsNoError</c> if the operation succeeded or an error code otherwise.
  /// </returns>
  public delegate JavaScriptErrorCode JavaScriptFetchImportedModuleFromScriptCallBack(
    JavaScriptSourceContext dwReferencingSourceContext,
    JavaScriptValue specifier,
    out JavaScriptModuleRecord dependentModuleRecord
  );
}
