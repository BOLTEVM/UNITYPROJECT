namespace ChakraCore.API {
  /// <summary>
  ///     User implemented callback to fetch additional imported modules in ES modules.
  /// </summary>
  /// <remarks>
  ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked until
  ///     the callback completes. Notify the host to fetch the dependent module. This is the "import" part
  ///     before HostResolveImportedModule in ES6 spec. This notifies the host that the referencing module has
  ///     the specified module dependency, and the host needs to retrieve the module back.
  ///
  ///     Callback should:
  ///     1. Check if the requested module has been requested before - if yes return the existing
  ///         module record
  ///     2. If no create and initialize a new module record with JsInitializeModuleRecord to
  ///         return and schedule a call to JsParseModuleSource for the new record.
  /// </remarks>
  /// <param name="referencingModule">The referencing module that is requesting the dependent module.</param>
  /// <param name="specifier">The specifier coming from the module source code.</param>
  /// <param name="dependentModuleRecord">The ModuleRecord of the dependent module. If the module was requested
  ///                                     before from other source, return the existing ModuleRecord, otherwise
  ///                                     return a newly created ModuleRecord.</param>
  /// <returns>
  ///     Returns a <c>JsNoError</c> if the operation succeeded an error code otherwise.
  /// </returns>
  public delegate JavaScriptErrorCode JavaScriptFetchImportedModuleCallBack(
    JavaScriptModuleRecord referencingModule,
    JavaScriptValue specifier,
    out JavaScriptModuleRecord dependentModuleRecord
  );
}
