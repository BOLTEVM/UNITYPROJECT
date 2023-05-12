using System;
using System.Text;

namespace ChakraCore.API {
  /// <summary>
  ///     A reference to an ES module.
  /// </summary>
  /// <remarks>
  ///     A module record represents an ES module.
  /// </remarks>
  public struct JavaScriptModuleRecord {
    /// <summary>
    /// The reference.
    /// </summary>
    private readonly IntPtr reference;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JavaScriptModuleRecord"/> struct.
    /// </summary>
    /// <param name="reference">The reference.</param>
    private JavaScriptModuleRecord(IntPtr reference) {
      this.reference = reference;
    }

    /// <summary>
    ///     Gets an invalid ID.
    /// </summary>
    public static JavaScriptModuleRecord Invalid {
      get { return new JavaScriptModuleRecord(IntPtr.Zero); }
    }

    /// <summary>
    ///     Gets a value indicating whether the record is valid.
    /// </summary>
    public bool IsValid {
      get { return reference != IntPtr.Zero; }
    }


    /// <summary>
    ///     Initialize a ModuleRecord from host
    /// </summary>
    /// <remarks>
    ///     Bootstrap the module loading process by creating a new module record.
    /// </remarks>
    /// <param name="referencingModule">The parent module of the new module - nullptr for a root module.</param>
    /// <param name="normalizedSpecifier">The normalized specifier for the module.</param>
    /// <returns>
    ///     The new module record. The host should not try to call this API twice with the same normalizedSpecifier.
    /// </returns>
    public static JavaScriptModuleRecord Initialize(
      JavaScriptModuleRecord? referencingModule = null,
      JavaScriptValue? normalizedSpecifier = null
    ) {
      JavaScriptValue normalizedSpecifierValue;
      if (normalizedSpecifier.HasValue) {
        normalizedSpecifierValue = normalizedSpecifier.Value;
      } else { // root module has no name, give it a unique name
        normalizedSpecifierValue = JavaScriptValue.FromString(
          Guid.NewGuid().ToString()
        );
      }

      JavaScriptModuleRecord referencingModuleValue;
      if (referencingModule.HasValue) {
        referencingModuleValue = referencingModule.Value;
      } else {
        referencingModuleValue = JavaScriptModuleRecord.Invalid;
      }

      JavaScriptModuleRecord moduleRecord;
      Native.ThrowIfError(
        Native.JsInitializeModuleRecord(
          referencingModuleValue,
          normalizedSpecifierValue,
          out moduleRecord
        )
      );
      return moduleRecord;
    }

    public static JavaScriptModuleRecord Initialize(
      JavaScriptModuleRecord? referencingModule,
      string normalizedSpecifier
    ) {
      if (string.IsNullOrEmpty(normalizedSpecifier)) {
        return Initialize(referencingModule);
      } else {
        return Initialize(referencingModule, JavaScriptValue.FromString(normalizedSpecifier));
      }
    }

    public uint AddRef() {
      uint count;
      Native.ThrowIfError(
        Native.JsAddRef(this, out count)
      );
      return count;
    }


    /// <summary>
    ///     Parse the source for an ES module
    /// </summary>
    /// <remarks>
    ///     This is basically ParseModule operation in ES6 spec. It is slightly different in that:
    ///     a) The ModuleRecord was initialized earlier, and passed in as an argument.
    ///     b) This includes a check to see if the module being Parsed is the last module in the
    /// dependency tree. If it is it automatically triggers Module Instantiation.
    /// </remarks>
    /// <param name="script">The source script to be parsed, but not executed in this code.</param>
    /// <param name="sourceContext">A cookie identifying the script that can be used by debuggable script contexts.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    public void ParseSource(
      string script,
      JavaScriptSourceContext sourceContext
    ) {
      byte[] scriptBuffer = Encoding.UTF8.GetBytes(script);
      uint scriptLength = (uint) scriptBuffer.Length;
      JavaScriptValue exceptionValue;
      JavaScriptErrorCode errorCode = Native.JsParseModuleSource(
        this,
        sourceContext,
        scriptBuffer,
        scriptLength,
        JavaScriptParseModuleSourceFlags.JsParseModuleSourceFlags_DataIsUTF8,
        out exceptionValue
      );
      if (errorCode != JavaScriptErrorCode.NoError) {
        if (exceptionValue.IsValid) {
          JavaScriptContext.SetException(exceptionValue);
        }
        Native.ThrowIfError(errorCode);
      }
    }
    public void ParseSource(string script) {
      ParseSource(script, JavaScriptSourceContext.None);
    }

    /// <summary>
    ///     Execute module code.
    /// </summary>
    /// <remarks>
    ///     This method implements 15.2.1.1.6.5, "ModuleEvaluation" concrete method.
    ///     This method should be called after the engine notifies the host that the module is ready.
    ///     This method only needs to be called on root modules - it will execute all of the dependent modules.
    ///
    ///     One moduleRecord will be executed only once. Additional execution call on the same moduleRecord will fail.
    /// </remarks>
    /// <returns>
    ///     The return value of the module.
    /// </returns>
    public JavaScriptValue Evaluate() {
      JavaScriptValue result;
      Native.ThrowIfError(
        Native.JsModuleEvaluation(
          this,
          out result
        )
      );
      return result;
    }


    /// <summary>
    ///   Set an exception on the module object - only relevant prior to it being Parsed
    /// </summary>
    /// <param name="exception">An exception object - e.g. if the module file cannot be found.</param>
    public JavaScriptValue Exception {
      get {
        JavaScriptValue value;
        Native.ThrowIfError(
          Native.JsGetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_Exception,
            out value
          )
        );
        return value;
      }
      set {
        Native.ThrowIfError(
          Native.JsSetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_Exception,
            value
          )
        );
      }
    }

    /// <summary>
    ///   Set host defined info on a module record - can be anything that you wish to associate with your modules
    /// </summary>
    /// <param name="hostInfo">Host defined info.</param>
    public IntPtr HostDefined {
      get {
        IntPtr value;
        Native.ThrowIfError(
          Native.JsGetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_HostDefined,
            out value
          )
        );
        return value;
      }
      set {
        Native.ThrowIfError(
          Native.JsSetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_HostDefined,
            value
          )
        );
      }
    }

    /// <summary>
    ///     Set user implemented callback to get notification when a module is ready.
    /// </summary>
    /// <remarks>
    ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked until the
    ///     callback completes. This callback should schedule a call to JsEvaluateModule to run the module that has been loaded.
    /// </remarks>
    /// <param name="callback">Callback for receiving notification when module is ready.</param>
    public NotifyModuleReadyCallback NotifyModuleReadyCallback {
      get {
        NotifyModuleReadyCallback value;
        Native.ThrowIfError(
          Native.JsGetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_NotifyModuleReadyCallback,
            out value
          )
        );
        return value;
      }
      set {
        Native.ThrowIfError(
          Native.JsSetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_NotifyModuleReadyCallback,
            value
          )
        );
      }
    }

    /// <summary>
    ///     Set user implemented callback to fetch additional imported modules in ES modules.
    /// </summary>
    /// <param name="callback">Callback for receiving notification to fetch a dependent module.</param>
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
    public FetchImportedModuleCallBack FetchImportedModuleCallBack {
      get {
        FetchImportedModuleCallBack value;
        Native.ThrowIfError(
          Native.JsGetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_FetchImportedModuleCallback,
            out value
          )
        );
        return value;
      }
      set {
        Native.ThrowIfError(
          Native.JsSetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_FetchImportedModuleCallback,
            value
          )
        );
      }
    }

    /// <summary>
    ///     Set user implemented callback to fetch imported modules dynamically in scripts.
    /// </summary>
    /// <param name="callback">Callback for receiving notification for calls to import()</param>
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
    public FetchImportedModuleFromScriptCallBack FetchImportedModuleFromScriptCallBack {
      get {
        FetchImportedModuleFromScriptCallBack value;
        Native.ThrowIfError(
          Native.JsGetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_FetchImportedModuleFromScriptCallback,
            out value
          )
        );
        return value;
      }
      set {
        Native.ThrowIfError(
          Native.JsSetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_FetchImportedModuleFromScriptCallback,
            value
          )
        );
      }
    }

    /// <summary>
    ///   Set a URL for a module to be used for stack traces/debugging
    ///   - note this must be set before calling JsParseModuleSource on the module or it will be ignored
    /// </summary>
    /// <param name="url">URL for use in error stack traces and debugging.</param>
    public string HostUrl {
      get {
        JavaScriptValue result;
        Native.ThrowIfError(
          Native.JsGetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_Url,
            out result
          )
        );
        if (result.IsValid && result.ValueType == JavaScriptValueType.String) {
          return result.ToString();
        } else {
          return null;
        }
      }
      set {
        Native.ThrowIfError(
          Native.JsSetModuleHostInfo(
            this,
            JavascriptModuleHostInfoKind.JsModuleHostInfo_Url,
            JavaScriptValue.FromString(value)
          )
        );
      }
    }


    /// <summary>
    ///     Retrieve the namespace object for a module.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context and that the module has already been evaluated.
    /// </remarks>
    /// <returns>
    ///     The requested namespace object.
    /// </returns>
    public JavaScriptValue Namespace {
      get {
        JavaScriptValue moduleNamespace;
        Native.ThrowIfError(
          Native.JsGetModuleNamespace(
            this,
            out moduleNamespace
          )
        );
        return moduleNamespace;
      }
    }

  }


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
  public delegate JavaScriptErrorCode FetchImportedModuleCallBack(
    JavaScriptModuleRecord referencingModule,
    JavaScriptValue specifier,
    out JavaScriptModuleRecord dependentModuleRecord
  );

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
  /// <param name="sourceContext">The referencing script context that calls import()</param>
  /// <param name="specifier">The specifier provided to the import() call.</param>
  /// <param name="dependentModuleRecord">The ModuleRecord of the dependent module. If the module was requested
  ///                                     before from other source, return the existing ModuleRecord, otherwise
  ///                                     return a newly created ModuleRecord.</param>
  /// <returns>
  ///     Returns <c>JsNoError</c> if the operation succeeded or an error code otherwise.
  /// </returns>
  public delegate JavaScriptErrorCode FetchImportedModuleFromScriptCallBack(
    JavaScriptSourceContext sourceContext,
    JavaScriptValue specifier,
    out JavaScriptModuleRecord dependentModuleRecord
  );

  /// <summary>
  ///     User implemented callback to get notification when the module is ready.
  /// </summary>
  /// <remarks>
  ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked until the
  ///     callback completes. This callback should schedule a call to JsEvaluateModule to run the module that has been loaded.
  /// </remarks>
  /// <param name="referencingModule">The referencing module that has finished running ModuleDeclarationInstantiation step.</param>
  /// <param name="exceptionVar">If nullptr, the module is successfully initialized and host should queue the execution job
  ///                            otherwise it's the exception object.</param>
  /// <returns>
  ///     Returns a JsErrorCode - note, the return value is ignored.
  /// </returns>
  public delegate JavaScriptErrorCode NotifyModuleReadyCallback(
    JavaScriptModuleRecord referencingModule,
    JavaScriptValue exceptionVar
  );
}
