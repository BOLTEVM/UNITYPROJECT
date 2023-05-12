using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ChakraCore.API {
  /// <summary>
  ///     Native interfaces.
  /// </summary>
  public static class Native {
    /// <summary>
    /// Throws if a native method returns an error code.
    /// </summary>
    /// <param name="error">The error.</param>
    public static void ThrowIfError(JavaScriptErrorCode error, bool ignoreScriptError = false) {
      if (error != JavaScriptErrorCode.NoError) {
        switch (error) {
          case JavaScriptErrorCode.InvalidArgument:
            throw new JavaScriptUsageException(error, "Invalid argument.");

          case JavaScriptErrorCode.NullArgument:
            throw new JavaScriptUsageException(error, "Null argument.");

          case JavaScriptErrorCode.NoCurrentContext:
            throw new JavaScriptUsageException(error, "No current context.");

          case JavaScriptErrorCode.InExceptionState:
            throw new JavaScriptUsageException(error, "Runtime is in exception state.");

          case JavaScriptErrorCode.NotImplemented:
            throw new JavaScriptUsageException(error, "Method is not implemented.");

          case JavaScriptErrorCode.WrongThread:
            throw new JavaScriptUsageException(error, "Runtime is active on another thread.");

          case JavaScriptErrorCode.RuntimeInUse:
            throw new JavaScriptUsageException(error, "Runtime is in use.");

          case JavaScriptErrorCode.BadSerializedScript:
            throw new JavaScriptUsageException(error, "Bad serialized script.");

          case JavaScriptErrorCode.InDisabledState:
            throw new JavaScriptUsageException(error, "Runtime is disabled.");

          case JavaScriptErrorCode.CannotDisableExecution:
            throw new JavaScriptUsageException(error, "Cannot disable execution.");

          case JavaScriptErrorCode.AlreadyDebuggingContext:
            throw new JavaScriptUsageException(error, "Context is already in debug mode.");

          case JavaScriptErrorCode.HeapEnumInProgress:
            throw new JavaScriptUsageException(error, "Heap enumeration is in progress.");

          case JavaScriptErrorCode.ArgumentNotObject:
            throw new JavaScriptUsageException(error, "Argument is not an object.");

          case JavaScriptErrorCode.InProfileCallback:
            throw new JavaScriptUsageException(error, "In a profile callback.");

          case JavaScriptErrorCode.InThreadServiceCallback:
            throw new JavaScriptUsageException(error, "In a thread service callback.");

          case JavaScriptErrorCode.CannotSerializeDebugScript:
            throw new JavaScriptUsageException(error, "Cannot serialize a debug script.");

          case JavaScriptErrorCode.AlreadyProfilingContext:
            throw new JavaScriptUsageException(error, "Already profiling this context.");

          case JavaScriptErrorCode.IdleNotEnabled:
            throw new JavaScriptUsageException(error, "Idle is not enabled.");

          case JavaScriptErrorCode.OutOfMemory:
            throw new JavaScriptEngineException(error, "Out of memory.");

          case JavaScriptErrorCode.ScriptException:
            {
              if (!ignoreScriptError) {
                JavaScriptValue errorObject;
                string msg = getErrorMessageAndObject(out errorObject);
                throw new JavaScriptScriptException(error, errorObject, $"Script threw an exception. {msg}");
              }
              break;
            }
          case JavaScriptErrorCode.ScriptCompile:
            {
              JavaScriptValue errorObject;
              string msg = getErrorMessageAndObject(out errorObject);
              throw new JavaScriptScriptException(error, errorObject, $"Compile error. {msg}");
            }

          case JavaScriptErrorCode.ScriptTerminated:
            throw new JavaScriptScriptException(error, JavaScriptValue.Invalid, "Script was terminated.");

          case JavaScriptErrorCode.ScriptEvalDisabled:
            throw new JavaScriptScriptException(error, JavaScriptValue.Invalid, "Eval of strings is disabled in this runtime.");

          case JavaScriptErrorCode.Fatal:
            throw new JavaScriptFatalException(error);

          default:
            throw new JavaScriptFatalException(error);
        }
      }
    }

    private static string getErrorMessageAndObject(out JavaScriptValue errorObject) {
      JavaScriptValue metadata;
      JavaScriptErrorCode result = JsGetAndClearExceptionWithMetadata(out metadata); // exception line column length source url
      if (result != JavaScriptErrorCode.NoError) {
        errorObject = JavaScriptValue.Invalid;
        return "Failed to get js Error! JsGetAndClearExceptionWithMetadata failed: " + result;
      }

      errorObject = metadata.GetProperty("exception");


      string source = metadata.GetProperty("source").ToString(); // can be string "undefined"
      string url = metadata.GetProperty("url").ToString();
      int line = metadata.GetProperty("line").ToInt32(); // 0 based
      int column = metadata.GetProperty("column").ToInt32(); // 0 based

      JavaScriptValueType valueType = errorObject.ValueType;

      string message;
      bool hasStack = false;
      if (valueType == JavaScriptValueType.Error) {
        // can be undefined sometimes (Compile error/syntax)
        JavaScriptValue stackProp = errorObject.GetProperty("stack");

        if (stackProp.ValueType == JavaScriptValueType.String) {
          // stack includes message, if it exists
          hasStack = true;
          message = stackProp.ToString();
        } else {
          message = errorObject.GetProperty("message").ToString();
        }
      } else { // something that isn't Error
        string toString = errorObject.ConvertToString().ToString();
        message = $"{toString} ({valueType})";
      }

      if (!hasStack && (url != "undefined" || line != 0 || column != 0)) {
        message = $"{message}\n   at {url}:{line + 1}:{column + 1}";
      }


      message = $"\n{message}\n";

      if (source != "undefined") {
        if (source.Length < column) { // something weird happened and we couldn't obtain the full source line
          return message;
        }

        const int charsOnEachSide = 30;

        int index = Math.Min(
          Math.Max(0, column - charsOnEachSide),
          source.Length
        );

        source = source.Substring(
          index,
          Math.Min(charsOnEachSide * 2 + 1, source.Length - index)
        );

        string arrow = new String(' ', (column - index) + (line + 1).ToString().Length + 2) + "^"; // repeat space char
        return message + $"{line + 1}| {source}\n{arrow}\n";
      } else {
        return message;
      }
    }


    const string DllName = "ChakraCore";

    #region ChakraCommon.h

    /// <summary>
    ///     Creates a new runtime.
    /// </summary>
    /// <param name="attributes">The attributes of the runtime to be created.</param>
    /// <param name="threadService">The thread service for the runtime. Can be null.</param>
    /// <param name="runtime">The runtime created.</param>
    /// <remarks>In the edge-mode binary, chakra.dll, this function lacks the <c>runtimeVersion</c>
    /// parameter (compare to jsrt9.h).</remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateRuntime(JavaScriptRuntimeAttributes attributes, JavaScriptThreadServiceCallback threadService, out JavaScriptRuntime runtime);

    /// <summary>
    ///     Performs a full garbage collection.
    /// </summary>
    /// <param name="runtime">The runtime in which the garbage collection will be performed.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCollectGarbage(JavaScriptRuntime handle);

    /// <summary>
    ///     Disposes a runtime.
    /// </summary>
    /// <remarks>
    ///     Once a runtime has been disposed, all resources owned by it are invalid and cannot be used.
    ///     If the runtime is active (i.e. it is set to be current on a particular thread), it cannot
    ///     be disposed.
    /// </remarks>
    /// <param name="runtime">The runtime to dispose.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDisposeRuntime(JavaScriptRuntime handle);

    /// <summary>
    ///     Gets the current memory usage for a runtime.
    /// </summary>
    /// <remarks>
    ///     Memory usage can be always be retrieved, regardless of whether or not the runtime is active
    ///     on another thread.
    /// </remarks>
    /// <param name="runtime">The runtime whose memory usage is to be retrieved.</param>
    /// <param name="memoryUsage">The runtime's current memory usage, in bytes.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetRuntimeMemoryUsage(JavaScriptRuntime runtime, out UIntPtr memoryUsage);

    /// <summary>
    ///     Gets the current memory limit for a runtime.
    /// </summary>
    /// <remarks>
    ///     The memory limit of a runtime can be always be retrieved, regardless of whether or not the
    ///     runtime is active on another thread.
    /// </remarks>
    /// <param name="runtime">The runtime whose memory limit is to be retrieved.</param>
    /// <param name="memoryLimit">
    ///     The runtime's current memory limit, in bytes, or -1 if no limit has been set.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetRuntimeMemoryLimit(JavaScriptRuntime runtime, out UIntPtr memoryLimit);

    /// <summary>
    ///     Sets the current memory limit for a runtime.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     A memory limit will cause any operation which exceeds the limit to fail with an "out of
    ///     memory" error. Setting a runtime's memory limit to -1 means that the runtime has no memory
    ///     limit. New runtimes  default to having no memory limit. If the new memory limit exceeds
    ///     current usage, the call will succeed and any future allocations in this runtime will fail
    ///     until the runtime's memory usage drops below the limit.
    ///     </para>
    ///     <para>
    ///     A runtime's memory limit can be always be set, regardless of whether or not the runtime is
    ///     active on another thread.
    ///     </para>
    /// </remarks>
    /// <param name="runtime">The runtime whose memory limit is to be set.</param>
    /// <param name="memoryLimit">
    ///     The new runtime memory limit, in bytes, or -1 for no memory limit.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetRuntimeMemoryLimit(JavaScriptRuntime runtime, UIntPtr memoryLimit);

    /// <summary>
    ///     Sets a memory allocation callback for specified runtime
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Registering a memory allocation callback will cause the runtime to call back to the host
    ///     whenever it acquires memory from, or releases memory to, the OS. The callback routine is
    ///     called before the runtime memory manager allocates a block of memory. The allocation will
    ///     be rejected if the callback returns false. The runtime memory manager will also invoke the
    ///     callback routine after freeing a block of memory, as well as after allocation failures.
    ///     </para>
    ///     <para>
    ///     The callback is invoked on the current runtime execution thread, therefore execution is
    ///     blocked until the callback completes.
    ///     </para>
    ///     <para>
    ///     The return value of the callback is not stored; previously rejected allocations will not
    ///     prevent the runtime from invoking the callback again later for new memory allocations.
    ///     </para>
    /// </remarks>
    /// <param name="runtime">The runtime for which to register the allocation callback.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="allocationCallback">
    ///     Memory allocation callback to be called for memory allocation events.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetRuntimeMemoryAllocationCallback(JavaScriptRuntime runtime, IntPtr callbackState, JavaScriptMemoryAllocationCallback allocationCallback);

    /// <summary>
    ///     Sets a callback function that is called by the runtime before garbage collection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The callback is invoked on the current runtime execution thread, therefore execution is
    ///     blocked until the callback completes.
    ///     </para>
    ///     <para>
    ///     The callback can be used by hosts to prepare for garbage collection. For example, by
    ///     releasing unnecessary references on Chakra objects.
    ///     </para>
    /// </remarks>
    /// <param name="runtime">The runtime for which to register the allocation callback.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="beforeCollectCallback">The callback function being set.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetRuntimeBeforeCollectCallback(JavaScriptRuntime runtime, IntPtr callbackState, JavaScriptBeforeCollectCallback beforeCollectCallback);

    /// <summary>
    ///     Adds a reference to a garbage collected object.
    /// </summary>
    /// <remarks>
    ///     This only needs to be called on <c>JsRef</c> handles that are not going to be stored
    ///     somewhere on the stack. Calling <c>JsAddRef</c> ensures that the object the <c>JsRef</c>
    ///     refers to will not be freed until <c>JsRelease</c> is called.
    /// </remarks>
    /// <param name="reference">The object to add a reference to.</param>
    /// <param name="count">The object's new reference count (can pass in null).</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsAddRef(JavaScriptValue reference, out uint count);

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsAddRef(JavaScriptContext reference, out uint count);

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsAddRef(JavaScriptModuleRecord reference, out uint count);

    /// <summary>
    ///     Releases a reference to a garbage collected object.
    /// </summary>
    /// <remarks>
    ///     Removes a reference to a <c>JsRef</c> handle that was created by <c>JsAddRef</c>.
    /// </remarks>
    /// <param name="reference">The object to add a reference to.</param>
    /// <param name="count">The object's new reference count (can pass in null).</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsRelease(JavaScriptValue reference, out uint count);

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsRelease(JavaScriptContext reference, out uint count);

    /// <summary>
    ///     Sets a callback function that is called by the runtime before garbage collection of
    ///     an object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The callback is invoked on the current runtime execution thread, therefore execution is
    ///     blocked until the callback completes.
    ///     </para>
    /// </remarks>
    /// <param name="reference">The object for which to register the callback.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="objectBeforeCollectCallback">The callback function being set. Use null to clear
    ///     previously registered callback.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetObjectBeforeCollectCallback(JavaScriptValue reference, IntPtr callbackState, JavaScriptObjectBeforeCollectCallback objectBeforeCollectCallback);

    /// <summary>
    ///     Creates a script context for running scripts.
    /// </summary>
    /// <remarks>
    ///     Each script context has its own global object that is isolated from all other script
    ///     contexts.
    /// </remarks>
    /// <param name="runtime">The runtime the script context is being created in.</param>
    /// <param name="newContext">The created script context.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateContext(JavaScriptRuntime runtime, out JavaScriptContext newContext);

    /// <summary>
    ///     Gets the current script context on the thread.
    /// </summary>
    /// <param name="currentContext">
    ///     The current script context on the thread, null if there is no current script context.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetCurrentContext(out JavaScriptContext currentContext);

    /// <summary>
    ///     Sets the current script context on the thread.
    /// </summary>
    /// <param name="context">The script context to make current.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetCurrentContext(JavaScriptContext context);

    /// <summary>
    ///     Gets the script context that the object belongs to.
    /// </summary>
    /// <param name="obj">The object to get the context from.</param>
    /// <param name="context">The context the object belongs to.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetContextOfObject(JavaScriptValue obj, out JavaScriptContext context);

    /// <summary>
    ///     Gets the internal data set on JsrtContext.
    /// </summary>
    /// <param name="context">The context to get the data from.</param>
    /// <param name="data">The pointer to the data where data will be returned.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetContextData(JavaScriptContext context, out IntPtr data);

    /// <summary>
    ///     Sets the internal data of JsrtContext.
    /// </summary>
    /// <param name="context">The context to set the data to.</param>
    /// <param name="data">The pointer to the data to be set.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetContextData(JavaScriptContext context, IntPtr data);

    /// <summary>
    ///     Gets the runtime that the context belongs to.
    /// </summary>
    /// <param name="context">The context to get the runtime from.</param>
    /// <param name="runtime">The runtime the context belongs to.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetRuntime(JavaScriptContext context, out JavaScriptRuntime runtime);

    /// <summary>
    ///     Tells the runtime to do any idle processing it need to do.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If idle processing has been enabled for the current runtime, calling <c>JsIdle</c> will
    ///     inform the current runtime that the host is idle and that the runtime can perform
    ///     memory cleanup tasks.
    ///     </para>
    ///     <para>
    ///     <c>JsIdle</c> can also return the number of system ticks until there will be more idle work
    ///     for the runtime to do. Calling <c>JsIdle</c> before this number of ticks has passed will do
    ///     no work.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="nextIdleTick">
    ///     The next system tick when there will be more idle work to do. Can be null. Returns the
    ///     maximum number of ticks if there no upcoming idle work to do.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsIdle(out uint nextIdleTick);

    /// <summary>
    ///     Gets the symbol associated with the property ID.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="propertyId">The property ID to get the symbol of.</param>
    /// <param name="symbol">The symbol associated with the property ID.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetSymbolFromPropertyId(JavaScriptPropertyId propertyId, out JavaScriptValue symbol);

    /// <summary>
    ///     Gets the type of property
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="propertyId">The property ID to get the type of.</param>
    /// <param name="propertyIdType">The JsPropertyIdType of the given property ID</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetPropertyIdType(JavaScriptPropertyId propertyId, out JavaScriptPropertyIdType propertyIdType);

    /// <summary>
    ///     Gets the property ID associated with the symbol.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Property IDs are specific to a context and cannot be used across contexts.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="symbol">
    ///     The symbol whose property ID is being retrieved.
    /// </param>
    /// <param name="propertyId">The property ID for the given symbol.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetPropertyIdFromSymbol(JavaScriptValue symbol, out JavaScriptPropertyId propertyId);

    /// <summary>
    ///     Creates a Javascript symbol.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="description">The string description of the symbol. Can be null.</param>
    /// <param name="result">The new symbol.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateSymbol(JavaScriptValue description, out JavaScriptValue symbol);

    /// <summary>
    ///     Gets the list of all symbol properties on the object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object from which to get the property symbols.</param>
    /// <param name="propertySymbols">An array of property symbols.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetOwnPropertySymbols(JavaScriptValue obj, out JavaScriptValue propertySymbols);

    /// <summary>
    ///     Gets the value of <c>undefined</c> in the current script context.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="undefinedValue">The <c>undefined</c> value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetUndefinedValue(out JavaScriptValue undefinedValue);

    /// <summary>
    ///     Gets the value of <c>null</c> in the current script context.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="nullValue">The <c>null</c> value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetNullValue(out JavaScriptValue nullValue);

    /// <summary>
    ///     Gets the value of <c>true</c> in the current script context.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="trueValue">The <c>true</c> value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetTrueValue(out JavaScriptValue trueValue);

    /// <summary>
    ///     Gets the value of <c>false</c> in the current script context.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="falseValue">The <c>false</c> value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetFalseValue(out JavaScriptValue falseValue);

    /// <summary>
    ///     Creates a Boolean value from a <c>bool</c> value.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="value">The value to be converted.</param>
    /// <param name="booleanValue">The converted value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsBoolToBoolean(bool value, out JavaScriptValue booleanValue);

    /// <summary>
    ///     Retrieves the <c>bool</c> value of a Boolean value.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="boolValue">The converted value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsBooleanToBool(JavaScriptValue booleanValue, out bool boolValue);

    /// <summary>
    ///     Converts the value to Boolean using standard JavaScript semantics.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="value">The value to be converted.</param>
    /// <param name="booleanValue">The converted value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsConvertValueToBoolean(JavaScriptValue value, out JavaScriptValue booleanValue);

    /// <summary>
    ///     Gets the JavaScript type of a JsValueRef.
    /// </summary>
    /// <param name="value">The value whose type is to be returned.</param>
    /// <param name="type">The type of the value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetValueType(JavaScriptValue value, out JavaScriptValueType type);

    /// <summary>
    ///     Creates a number value from a <c>double</c> value.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="doubleValue">The <c>double</c> to convert to a number value.</param>
    /// <param name="value">The new number value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDoubleToNumber(double doubleValue, out JavaScriptValue value);

    /// <summary>
    ///     Creates a number value from an <c>int</c> value.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="intValue">The <c>int</c> to convert to a number value.</param>
    /// <param name="value">The new number value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsIntToNumber(int intValue, out JavaScriptValue value);

    /// <summary>
    ///     Retrieves the <c>double</c> value of a number value.
    /// </summary>
    /// <remarks>
    ///     This function retrieves the value of a number value. It will fail with
    ///     <c>JsErrorInvalidArgument</c> if the type of the value is not number.
    /// </remarks>
    /// <param name="value">The number value to convert to a <c>double</c> value.</param>
    /// <param name="doubleValue">The <c>double</c> value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsNumberToDouble(JavaScriptValue value, out double doubleValue);

    /// <summary>
    ///     Retrieves the <c>int</c> value of a number value.
    /// </summary>
    /// <remarks>
    ///     This function retrieves the value of a number value and converts to an <c>int</c> value.
    ///     It will fail with <c>JsErrorInvalidArgument</c> if the type of the value is not number.
    /// </remarks>
    /// <param name="value">The number value to convert to an <c>int</c> value.</param>
    /// <param name="intValue">The <c>int</c> value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsNumberToInt(JavaScriptValue value, out int intValue);

    /// <summary>
    ///     Converts the value to number using standard JavaScript semantics.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="value">The value to be converted.</param>
    /// <param name="numberValue">The converted value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsConvertValueToNumber(JavaScriptValue value, out JavaScriptValue numberValue);

    /// <summary>
    ///     Gets the length of a string value.
    /// </summary>
    /// <param name="stringValue">The string value to get the length of.</param>
    /// <param name="length">The length of the string.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetStringLength(JavaScriptValue sringValue, out int length);

    /// <summary>
    ///     Converts the value to string using standard JavaScript semantics.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="value">The value to be converted.</param>
    /// <param name="stringValue">The converted value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsConvertValueToString(JavaScriptValue value, out JavaScriptValue stringValue);

    /// <summary>
    ///     Gets the global object in the current script context.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="globalObject">The global object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetGlobalObject(out JavaScriptValue globalObject);

    /// <summary>
    ///     Creates a new object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The new object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateObject(out JavaScriptValue obj);

    /// <summary>
    ///     Creates a new object that stores some external data.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="data">External data that the object will represent. May be null.</param>
    /// <param name="finalizeCallback">
    ///     A callback for when the object is finalized. May be null.
    /// </param>
    /// <param name="obj">The new object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateExternalObject(IntPtr data, JavaScriptFinalizeCallback finalizeCallback, out JavaScriptValue obj);

    /// <summary>
    ///     Converts the value to object using standard JavaScript semantics.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="value">The value to be converted.</param>
    /// <param name="obj">The converted value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsConvertValueToObject(JavaScriptValue value, out JavaScriptValue obj);

    /// <summary>
    ///     Returns the prototype of an object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object whose prototype is to be returned.</param>
    /// <param name="prototypeObject">The object's prototype.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetPrototype(JavaScriptValue obj, out JavaScriptValue prototypeObject);

    /// <summary>
    ///     Sets the prototype of an object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object whose prototype is to be changed.</param>
    /// <param name="prototypeObject">The object's new prototype.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetPrototype(JavaScriptValue obj, JavaScriptValue prototypeObject);

    /// <summary>
    ///     Performs JavaScript "instanceof" operator test.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to test.</param>
    /// <param name="constructor">The constructor function to test against.</param>
    /// <param name="result">Whether "object instanceof constructor" is true.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsInstanceOf(JavaScriptValue obj, JavaScriptValue constructor, out bool result);

    /// <summary>
    ///     Returns a value that indicates whether an object is extensible or not.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to test.</param>
    /// <param name="value">Whether the object is extensible or not.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetExtensionAllowed(JavaScriptValue obj, out bool value);

    /// <summary>
    ///     Makes an object non-extensible.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to make non-extensible.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsPreventExtension(JavaScriptValue obj);

    /// <summary>
    ///     Gets an object's property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that contains the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, out JavaScriptValue value);

    /// <summary>
    ///     Gets a property descriptor for an object's own property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that has the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="propertyDescriptor">The property descriptor.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetOwnPropertyDescriptor(JavaScriptValue obj, JavaScriptPropertyId propertyId, out JavaScriptValue propertyDescriptor);

    /// <summary>
    ///     Gets the list of all properties on the object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object from which to get the property names.</param>
    /// <param name="propertyNames">An array of property names.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetOwnPropertyNames(JavaScriptValue obj, out JavaScriptValue propertyNames);

    /// <summary>
    ///     Puts an object's property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that contains the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="value">The new value of the property.</param>
    /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, JavaScriptValue value, bool useStrictRules);

    /// <summary>
    ///     Determines whether an object has a property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that may contain the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="hasProperty">Whether the object (or a prototype) has the property.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsHasProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, out bool hasProperty);

    /// <summary>
    ///     Deletes an object's property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that contains the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
    /// <param name="result">Whether the property was deleted.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDeleteProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, bool useStrictRules, out JavaScriptValue result);

    /// <summary>
    ///     Defines a new object's own property from a property descriptor.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that has the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="propertyDescriptor">The property descriptor.</param>
    /// <param name="result">Whether the property was defined.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDefineProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, JavaScriptValue propertyDescriptor, out bool result);

    /// <summary>
    ///     Tests whether an object has a value at the specified index.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to operate on.</param>
    /// <param name="index">The index to test.</param>
    /// <param name="result">Whether the object has a value at the specified index.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsHasIndexedProperty(JavaScriptValue obj, JavaScriptValue index, out bool result);

    /// <summary>
    ///     Retrieve the value at the specified index of an object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to operate on.</param>
    /// <param name="index">The index to retrieve.</param>
    /// <param name="result">The retrieved value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetIndexedProperty(JavaScriptValue obj, JavaScriptValue index, out JavaScriptValue result);

    /// <summary>
    ///     Set the value at the specified index of an object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to operate on.</param>
    /// <param name="index">The index to set.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetIndexedProperty(JavaScriptValue obj, JavaScriptValue index, JavaScriptValue value);

    /// <summary>
    ///     Delete the value at the specified index of an object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to operate on.</param>
    /// <param name="index">The index to delete.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDeleteIndexedProperty(JavaScriptValue obj, JavaScriptValue index);

    /// <summary>
    ///     Determines whether an object has its indexed properties in external data.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="value">Whether the object has its indexed properties in external data.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsHasIndexedPropertiesExternalData(JavaScriptValue obj, out bool value);

    /// <summary>
    ///     Retrieves an object's indexed properties external data information.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="data">The external data back store for the object's indexed properties.</param>
    /// <param name="arrayType">The array element type in external data.</param>
    /// <param name="elementLength">The number of array elements in external data.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetIndexedPropertiesExternalData(JavaScriptValue obj, IntPtr data, out JavaScriptTypedArrayType arrayType, out uint elementLength);

    /// <summary>
    ///     Sets an object's indexed properties to external data. The external data will be used as back
    ///     store for the object's indexed properties and accessed like a typed array.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object to operate on.</param>
    /// <param name="data">The external data to be used as back store for the object's indexed properties.</param>
    /// <param name="arrayType">The array element type in external data.</param>
    /// <param name="elementLength">The number of array elements in external data.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetIndexedPropertiesToExternalData(JavaScriptValue obj, IntPtr data, JavaScriptTypedArrayType arrayType, uint elementLength);

    /// <summary>
    ///     Compare two JavaScript values for equality.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This function is equivalent to the <c>==</c> operator in Javascript.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="object1">The first object to compare.</param>
    /// <param name="object2">The second object to compare.</param>
    /// <param name="result">Whether the values are equal.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsEquals(JavaScriptValue obj1, JavaScriptValue obj2, out bool result);

    /// <summary>
    ///     Compare two JavaScript values for strict equality.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This function is equivalent to the <c>===</c> operator in Javascript.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="object1">The first object to compare.</param>
    /// <param name="object2">The second object to compare.</param>
    /// <param name="result">Whether the values are strictly equal.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsStrictEquals(JavaScriptValue obj1, JavaScriptValue obj2, out bool result);

    /// <summary>
    ///     Determines whether an object is an external object.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="value">Whether the object is an external object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsHasExternalData(JavaScriptValue obj, out bool value);

    /// <summary>
    ///     Retrieves the data from an external object.
    /// </summary>
    /// <param name="obj">The external object.</param>
    /// <param name="externalData">
    ///     The external data stored in the object. Can be null if no external data is stored in the
    ///     object.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetExternalData(JavaScriptValue obj, out IntPtr externalData);

    /// <summary>
    ///     Sets the external data on an external object.
    /// </summary>
    /// <param name="obj">The external object.</param>
    /// <param name="externalData">
    ///     The external data to be stored in the object. Can be null if no external data is
    ///     to be stored in the object.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetExternalData(JavaScriptValue obj, IntPtr externalData);

    /// <summary>
    ///     Creates a Javascript array object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="length">The initial length of the array.</param>
    /// <param name="result">The new array object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateArray(uint length, out JavaScriptValue result);

    /// <summary>
    ///     Creates a Javascript ArrayBuffer object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="byteLength">
    ///     The number of bytes in the ArrayBuffer.
    /// </param>
    /// <param name="result">The new ArrayBuffer object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateArrayBuffer(uint byteLength, out JavaScriptValue result);

    /// <summary>
    ///     Creates a Javascript ArrayBuffer object to access external memory.
    /// </summary>
    /// <remarks>Requires an active script context.</remarks>
    /// <param name="data">A pointer to the external memory.</param>
    /// <param name="byteLength">The number of bytes in the external memory.</param>
    /// <param name="finalizeCallback">A callback for when the object is finalized. May be null.</param>
    /// <param name="callbackState">User provided state that will be passed back to finalizeCallback.</param>
    /// <param name="result">The new ArrayBuffer object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateExternalArrayBuffer(IntPtr data, uint byteLength, JavaScriptFinalizeCallback finalizeCallback, IntPtr callbackState, out JavaScriptValue obj);

    /// <summary>
    ///     Creates a Javascript typed array object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The <c>baseArray</c> can be an <c>ArrayBuffer</c>, another typed array, or a JavaScript
    ///     <c>Array</c>. The returned typed array will use the baseArray if it is an ArrayBuffer, or
    ///     otherwise create and use a copy of the underlying source array.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="arrayType">The type of the array to create.</param>
    /// <param name="baseArray">
    ///     The base array of the new array. Use <c>JS_INVALID_REFERENCE</c> if no base array.
    /// </param>
    /// <param name="byteOffset">
    ///     The offset in bytes from the start of baseArray (ArrayBuffer) for result typed array to reference.
    ///     Only applicable when baseArray is an ArrayBuffer object. Must be 0 otherwise.
    /// </param>
    /// <param name="elementLength">
    ///     The number of elements in the array. Only applicable when creating a new typed array without
    ///     baseArray (baseArray is <c>JS_INVALID_REFERENCE</c>) or when baseArray is an ArrayBuffer object.
    ///     Must be 0 otherwise.
    /// </param>
    /// <param name="result">The new typed array object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateTypedArray(
      JavaScriptTypedArrayType arrayType,
      JavaScriptValue arrayBuffer,
      uint byteOffset,
      uint elementLength,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Creates a Javascript DataView object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="arrayBuffer">
    ///     An existing ArrayBuffer object to use as the storage for the result DataView object.
    /// </param>
    /// <param name="byteOffset">
    ///     The offset in bytes from the start of arrayBuffer for result DataView to reference.
    /// </param>
    /// <param name="byteLength">
    ///     The number of bytes in the ArrayBuffer for result DataView to reference.
    /// </param>
    /// <param name="result">The new DataView object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateDataView(JavaScriptValue arrayBuffer, uint byteOffset, uint byteOffsetLength, out JavaScriptValue result);

    /// <summary>
    ///     Obtains frequently used properties of a typed array.
    /// </summary>
    /// <param name="typedArray">The typed array instance.</param>
    /// <param name="arrayType">The type of the array.</param>
    /// <param name="arrayBuffer">The ArrayBuffer backstore of the array.</param>
    /// <param name="byteOffset">The offset in bytes from the start of arrayBuffer referenced by the array.</param>
    /// <param name="byteLength">The number of bytes in the array.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetTypedArrayInfo(JavaScriptValue typedArray, out JavaScriptTypedArrayType arrayType, out JavaScriptValue arrayBuffer, out uint byteOffset, out uint byteLength);

    /// <summary>
    ///     Obtains the underlying memory storage used by an <c>ArrayBuffer</c>.
    /// </summary>
    /// <param name="arrayBuffer">The ArrayBuffer instance.</param>
    /// <param name="buffer">
    ///     The ArrayBuffer's buffer. The lifetime of the buffer returned is the same as the lifetime of the
    ///     the ArrayBuffer. The buffer pointer does not count as a reference to the ArrayBuffer for the purpose
    ///     of garbage collection.
    /// </param>
    /// <param name="bufferLength">The number of bytes in the buffer.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetArrayBufferStorage(JavaScriptValue arrayBuffer, out IntPtr data, out uint bufferLength);

    /// <summary>
    ///     Obtains the underlying memory storage used by a typed array.
    /// </summary>
    /// <param name="typedArray">The typed array instance.</param>
    /// <param name="buffer">
    ///     The array's buffer. The lifetime of the buffer returned is the same as the lifetime of the
    ///     the array. The buffer pointer does not count as a reference to the array for the purpose
    ///     of garbage collection.
    /// </param>
    /// <param name="bufferLength">The number of bytes in the buffer.</param>
    /// <param name="arrayType">The type of the array.</param>
    /// <param name="elementSize">
    ///     The size of an element of the array.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetTypedArrayStorage(
      JavaScriptValue typedArray,
      out IntPtr data,
      out uint bufferLength,
      out JavaScriptTypedArrayType arrayType,
      out int elementSize
    );

    /// <summary>
    ///     Obtains the underlying memory storage used by a DataView.
    /// </summary>
    /// <param name="dataView">The DataView instance.</param>
    /// <param name="buffer">
    ///     The DataView's buffer. The lifetime of the buffer returned is the same as the lifetime of the
    ///     the DataView. The buffer pointer does not count as a reference to the DataView for the purpose
    ///     of garbage collection.
    /// </param>
    /// <param name="bufferLength">The number of bytes in the buffer.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetDataViewStorage(JavaScriptValue dataView, out IntPtr data, out uint bufferLength);

    /// <summary>
    ///     Invokes a function.
    /// </summary>
    /// <remarks>
    ///     Requires thisArg as first argument of arguments.
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="function">The function to invoke.</param>
    /// <param name="arguments">The arguments to the call.</param>
    /// <param name="argumentCount">The number of arguments being passed in to the function.</param>
    /// <param name="result">The value returned from the function invocation, if any.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCallFunction(
      JavaScriptValue function,
      JavaScriptValue[] arguments,
      ushort argumentCount,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Invokes a function as a constructor.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="function">The function to invoke as a constructor.</param>
    /// <param name="arguments">The arguments to the call.</param>
    /// <param name="argumentCount">The number of arguments being passed in to the function.</param>
    /// <param name="result">The value returned from the function invocation.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsConstructObject(
      JavaScriptValue function,
      JavaScriptValue[] arguments,
      ushort argumentCount,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Creates a new JavaScript function.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="nativeFunction">The method to call when the function is invoked.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="function">The new function object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateFunction(
      JavaScriptNativeFunction nativeFunction,
      IntPtr externalData,
      out JavaScriptValue function
    );

    /// <summary>
    ///     Creates a new JavaScript function with name.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="name">The name of this function that will be used for diagnostics and stringification purposes.</param>
    /// <param name="nativeFunction">The method to call when the function is invoked.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="function">The new function object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateNamedFunction(
      JavaScriptValue name,
      JavaScriptNativeFunction nativeFunction,
      IntPtr callbackState,
      out JavaScriptValue function
    );

    /// <summary>
    ///     Creates a new JavaScript error object
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="message">Message for the error object.</param>
    /// <param name="error">The new error object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateError(JavaScriptValue message, out JavaScriptValue error);

    /// <summary>
    ///     Creates a new JavaScript RangeError error object
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="message">Message for the error object.</param>
    /// <param name="error">The new error object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateRangeError(JavaScriptValue message, out JavaScriptValue error);

    /// <summary>
    ///     Creates a new JavaScript ReferenceError error object
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="message">Message for the error object.</param>
    /// <param name="error">The new error object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateReferenceError(JavaScriptValue message, out JavaScriptValue error);

    /// <summary>
    ///     Creates a new JavaScript SyntaxError error object
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="message">Message for the error object.</param>
    /// <param name="error">The new error object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateSyntaxError(JavaScriptValue message, out JavaScriptValue error);

    /// <summary>
    ///     Creates a new JavaScript TypeError error object
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="message">Message for the error object.</param>
    /// <param name="error">The new error object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateTypeError(JavaScriptValue message, out JavaScriptValue error);

    /// <summary>
    ///     Creates a new JavaScript URIError error object
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="message">Message for the error object.</param>
    /// <param name="error">The new error object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateURIError(JavaScriptValue message, out JavaScriptValue error);

    /// <summary>
    ///     Determines whether the runtime of the current context is in an exception state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If a call into the runtime results in an exception (either as the result of running a
    ///     script or due to something like a conversion failure), the runtime is placed into an
    ///     "exception state." All calls into any context created by the runtime (except for the
    ///     exception APIs) will fail with <c>JsErrorInExceptionState</c> until the exception is
    ///     cleared.
    ///     </para>
    ///     <para>
    ///     If the runtime of the current context is in the exception state when a callback returns
    ///     into the engine, the engine will automatically rethrow the exception.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="hasException">
    ///     Whether the runtime of the current context is in the exception state.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsHasException(out bool hasException);

    /// <summary>
    ///     Returns the exception that caused the runtime of the current context to be in the
    ///     exception state and resets the exception state for that runtime.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If the runtime of the current context is not in an exception state, this API will return
    ///     <c>JsErrorInvalidArgument</c>. If the runtime is disabled, this will return an exception
    ///     indicating that the script was terminated, but it will not clear the exception (the
    ///     exception will be cleared if the runtime is re-enabled using
    ///     <c>JsEnableRuntimeExecution</c>).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="exception">The exception for the runtime of the current context.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetAndClearException(out JavaScriptValue exception);

    /// <summary>
    ///     Sets the runtime of the current context to an exception state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If the runtime of the current context is already in an exception state, this API will
    ///     return <c>JsErrorInExceptionState</c>.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="exception">
    ///     The JavaScript exception to set for the runtime of the current context.
    /// </param>
    /// <returns>
    ///     JsNoError if the engine was set into an exception state, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetException(JavaScriptValue exception);

    /// <summary>
    ///     Suspends script execution and terminates any running scripts in a runtime.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Calls to a suspended runtime will fail until <c>JsEnableRuntimeExecution</c> is called.
    ///     </para>
    ///     <para>
    ///     This API does not have to be called on the thread the runtime is active on. Although the
    ///     runtime will be set into a suspended state, an executing script may not be suspended
    ///     immediately; a running script will be terminated with an uncatchable exception as soon as
    ///     possible.
    ///     </para>
    ///     <para>
    ///     Suspending execution in a runtime that is already suspended is a no-op.
    ///     </para>
    /// </remarks>
    /// <param name="runtime">The runtime to be suspended.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDisableRuntimeExecution(JavaScriptRuntime runtime);

    /// <summary>
    ///     Enables script execution in a runtime.
    /// </summary>
    /// <remarks>
    ///     Enabling script execution in a runtime that already has script execution enabled is a
    ///     no-op.
    /// </remarks>
    /// <param name="runtime">The runtime to be enabled.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsEnableRuntimeExecution(JavaScriptRuntime runtime);

    /// <summary>
    ///     Returns a value that indicates whether script execution is disabled in the runtime.
    /// </summary>
    /// <param name="runtime">Specifies the runtime to check if execution is disabled.</param>
    /// <param name="isDisabled">If execution is disabled, <c>true</c>, <c>false</c> otherwise.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsIsRuntimeExecutionDisabled(JavaScriptRuntime runtime, out bool isDisabled);

    /// <summary>
    ///     Sets a promise continuation callback function that is called by the context when a task
    ///     needs to be queued for future execution
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="promiseContinuationCallback">The callback function being set.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetPromiseContinuationCallback(JavaScriptPromiseContinuationCallback promiseContinuationCallback, IntPtr callbackState);

    #endregion // ChakraCommon.h


    #region ChakraCore.h

    /// <summary>
    ///     Creates a new enhanced JavaScript function.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="nativeFunction">The method to call when the function is invoked.</param>
    /// <param name="metadata">If this is not <c>JS_INVALID_REFERENCE</c>, it is converted to a string and used as the name of the function.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="function">The new function object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateEnhancedFunction(
      JavaScriptEnhancedNativeFunction nativeFunction,
      JavaScriptValue metadata,
      IntPtr callbackState,
      out JavaScriptValue function
    );

    /// <summary>
    ///     Initialize a ModuleRecord from host
    /// </summary>
    /// <remarks>
    ///     Bootstrap the module loading process by creating a new module record.
    /// </remarks>
    /// <param name="referencingModule">The parent module of the new module - nullptr for a root module.</param>
    /// <param name="normalizedSpecifier">The normalized specifier for the module.</param>
    /// <param name="moduleRecord">The new module record. The host should not try to call this API twice
    ///                            with the same normalizedSpecifier.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsInitializeModuleRecord(
      JavaScriptModuleRecord referencingModule,
      JavaScriptValue normalizedSpecifier,
      out JavaScriptModuleRecord moduleRecord
    );

    /// <summary>
    ///     Parse the source for an ES module
    /// </summary>
    /// <remarks>
    ///     This is basically ParseModule operation in ES6 spec. It is slightly different in that:
    ///     a) The ModuleRecord was initialized earlier, and passed in as an argument.
    ///     b) This includes a check to see if the module being Parsed is the last module in the
    /// dependency tree. If it is it automatically triggers Module Instantiation.
    /// </remarks>
    /// <param name="requestModule">The ModuleRecord being parsed.</param>
    /// <param name="sourceContext">A cookie identifying the script that can be used by debuggable script contexts.</param>
    /// <param name="script">The source script to be parsed, but not executed in this code.</param>
    /// <param name="scriptLength">The length of sourceText in bytes. As the input might contain a embedded null.</param>
    /// <param name="sourceFlag">The type of the source code passed in. It could be utf16 or utf8 at this time.</param>
    /// <param name="exceptionValueRef">The error object if there is parse error.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsParseModuleSource(
      JavaScriptModuleRecord requestModule,
      JavaScriptSourceContext sourceContext,
      byte[] script,
      uint scriptLength,
      JavaScriptParseModuleSourceFlags sourceFlag,
      out JavaScriptValue exceptionValueRef
    );

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
    /// <param name="requestModule">The ModuleRecord being executed.</param>
    /// <param name="result">The return value of the module.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsModuleEvaluation(
      JavaScriptModuleRecord requestModule,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Set host info for the specified module.
    /// </summary>
    /// <remarks>
    ///     This is used for four things:
    ///     1. Setting up the callbacks for module loading - note these are actually
    ///         set on the current Context not the module so only have to be set for
    ///         the first root module in any given context.
    ///     2. Setting host defined info on a module record - can be anything that
    ///         you wish to associate with your modules.
    ///     3. Setting a URL for a module to be used for stack traces/debugging -
    ///         note this must be set before calling JsParseModuleSource on the module
    ///         or it will be ignored.
    ///     4. Setting an exception on the module object - only relevant prior to it being Parsed.
    /// </remarks>
    /// <param name="requestModule">The request module.</param>
    /// <param name="moduleHostInfo">The type of host info to be set.</param>
    /// <param name="hostInfo">The host info to be set.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      IntPtr hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      JavaScriptValue hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      NotifyModuleReadyCallback hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      FetchImportedModuleCallBack hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      FetchImportedModuleFromScriptCallBack hostInfo
    );

    /// <summary>
    ///     Retrieve the host info for the specified module.
    /// </summary>
    /// <remarks>
    ///     This can used to retrieve info previously set with JsSetModuleHostInfo.
    /// </remarks>
    /// <param name="requestModule">The request module.</param>
    /// <param name="moduleHostInfo">The type of host info to be retrieved.</param>
    /// <param name="hostInfo">The retrieved host info for the module.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      out IntPtr hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      out JavaScriptValue hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      out NotifyModuleReadyCallback hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      out FetchImportedModuleCallBack hostInfo
    );

    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetModuleHostInfo(
      JavaScriptModuleRecord requestModule,
      JavascriptModuleHostInfoKind moduleHostInfo,
      out FetchImportedModuleFromScriptCallBack hostInfo
    );

    /// <summary>
    ///     Returns metadata relating to the exception that caused the runtime of the current context
    ///     to be in the exception state and resets the exception state for that runtime. The metadata
    ///     includes a reference to the exception itself.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If the runtime of the current context is not in an exception state, this API will return
    ///     <c>JsErrorInvalidArgument</c>. If the runtime is disabled, this will return an exception
    ///     indicating that the script was terminated, but it will not clear the exception (the
    ///     exception will be cleared if the runtime is re-enabled using
    ///     <c>JsEnableRuntimeExecution</c>).
    ///     </para>
    ///     <para>
    ///     The metadata value is a javascript object with the following properties: <c>exception</c>, the
    ///     thrown exception object; <c>line</c>, the 0 indexed line number where the exception was thrown;
    ///     <c>column</c>, the 0 indexed column number where the exception was thrown; <c>length</c>, the
    ///     source-length of the cause of the exception; <c>source</c>, a string containing the line of
    ///     source code where the exception was thrown; and <c>url</c>, a string containing the name of
    ///     the script file containing the code that threw the exception.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="metadata">The exception metadata for the runtime of the current context.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetAndClearExceptionWithMetadata(
      out JavaScriptValue metadata
    );

    /// <summary>
    ///     Create JavascriptString variable from ASCII or Utf8 string
    /// </summary>
    /// <remarks>
    ///     <para>
    ///        Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Input string can be either ASCII or Utf8
    ///     </para>
    /// </remarks>
    /// <param name="content">Pointer to string memory.</param>
    /// <param name="length">Number of bytes within the string</param>
    /// <param name="value">JsValueRef representing the JavascriptString</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateString(
      string content,
      UIntPtr length,
      out JavaScriptValue value
    );

    /// <summary>
    ///     Create JavascriptString variable from Utf16 string
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Expects Utf16 string
    ///     </para>
    /// </remarks>
    /// <param name="content">Pointer to string memory.</param>
    /// <param name="length">Number of characters within the string</param>
    /// <param name="value">JsValueRef representing the JavascriptString</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsCreateStringUtf16(
      string content,
      UIntPtr length,
      out JavaScriptValue value
    );

    /// <summary>
    ///     Write JavascriptString value into C string buffer (Utf8)
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When size of the `buffer` is unknown,
    ///         `buffer` argument can be nullptr.
    ///         In that case, `length` argument will return the length needed.
    ///     </para>
    /// </remarks>
    /// <param name="value">JavascriptString value</param>
    /// <param name="buffer">Pointer to buffer</param>
    /// <param name="bufferSize">Buffer size</param>
    /// <param name="length">Total number of characters needed or written</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCopyString(
      JavaScriptValue value,
      StringBuilder buffer, // _Out_opt_ char* buffer
      UIntPtr bufferSize, // _In_ size_t bufferSize
      out UIntPtr length // _Out_opt_ size_t* length
    );

    /// <summary>
    ///     Write string value into Utf16 string buffer
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When size of the `buffer` is unknown,
    ///         `buffer` argument can be nullptr.
    ///         In that case, `written` argument will return the length needed.
    ///     </para>
    ///     <para>
    ///         when start is out of range or &lt; 0, returns JsErrorInvalidArgument
    ///         and `written` will be equal to 0.
    ///         If calculated length is 0 (It can be due to string length or `start`
    ///         and length combination), then `written` will be equal to 0 and call
    ///         returns JsNoError
    ///     </para>
    /// </remarks>
    /// <param name="value">JavascriptString value</param>
    /// <param name="start">start offset of buffer</param>
    /// <param name="length">length to be written</param>
    /// <param name="buffer">Pointer to buffer</param>
    /// <param name="written">Total number of characters written</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsCopyStringUtf16(
      JavaScriptValue value,
      int start,
      int length,
      StringBuilder buffer, // _Out_opt_ uint16_t* buffer
      out UIntPtr written // _Out_opt_ size_t* written
    );

    /// <summary>
    ///     Parses a script and returns a function representing the script.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Script source can be either JavascriptString or JavascriptExternalArrayBuffer.
    ///         In case it is an ExternalArrayBuffer, and the encoding of the buffer is Utf16,
    ///         JsParseScriptAttributeArrayBufferIsUtf16Encoded is expected on parseAttributes.
    ///     </para>
    ///     <para>
    ///         Use JavascriptExternalArrayBuffer with Utf8/ASCII script source
    ///         for better performance and smaller memory footprint.
    ///     </para>
    /// </remarks>
    /// <param name="script">The script to run.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="parseAttributes">Attribute mask for parsing the script</param>
    /// <param name="result">The result of the compiled script.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsParse(
      JavaScriptValue script,
      JavaScriptSourceContext sourceContext,
      JavaScriptValue sourceUrl,
      JavaScriptParseScriptAttributes parseAttributes,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Executes a script.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Script source can be either JavascriptString or JavascriptExternalArrayBuffer.
    ///         In case it is an ExternalArrayBuffer, and the encoding of the buffer is Utf16,
    ///         JsParseScriptAttributeArrayBufferIsUtf16Encoded is expected on parseAttributes.
    ///     </para>
    ///     <para>
    ///         Use JavascriptExternalArrayBuffer with Utf8/ASCII script source
    ///         for better performance and smaller memory footprint.
    ///     </para>
    /// </remarks>
    /// <param name="script">The script to run.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from</param>
    /// <param name="parseAttributes">Attribute mask for parsing the script</param>
    /// <param name="result">The result of the script, if any. This parameter can be null.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsRun(
      JavaScriptValue script,
      JavaScriptSourceContext sourceContext,
      JavaScriptValue sourceUrl,
      JavaScriptParseScriptAttributes parseAttributes,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Creates the property ID associated with the name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Property IDs are specific to a context and cannot be used across contexts.
    ///     </para>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="name">
    ///     The name of the property ID to get or create. The name may consist of only digits.
    ///     The string is expected to be ASCII / utf8 encoded.
    /// </param>
    /// <param name="length">length of the name in bytes</param>
    /// <param name="propertyId">The property ID in this runtime for the given name.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreatePropertyId(
      string name,
      UIntPtr length,
      out JavaScriptPropertyId propertyId
    );

    /// <summary>
    ///     Copies the name associated with the property ID into a buffer.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    ///     <para>
    ///         When size of the `buffer` is unknown,
    ///         `buffer` argument can be nullptr.
    ///         `length` argument will return the size needed.
    ///     </para>
    /// </remarks>
    /// <param name="propertyId">The property ID to get the name of.</param>
    /// <param name="buffer">The buffer holding the name associated with the property ID, encoded as utf8</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="written">Total number of characters written or to be written</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCopyPropertyId(
      JavaScriptPropertyId propertyId,
      StringBuilder buffer, // _Out_ char* buffer
      UIntPtr bufferSize,
      out UIntPtr written
    );

    /// <summary>
    ///     Serializes a parsed script to a buffer than can be reused.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>JsSerializeScript</c> parses a script and then stores the parsed form of the script in a
    ///     runtime-independent format. The serialized script then can be deserialized in any
    ///     runtime without requiring the script to be re-parsed.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Script source can be either JavascriptString or JavascriptExternalArrayBuffer.
    ///         In case it is an ExternalArrayBuffer, and the encoding of the buffer is Utf16,
    ///         JsParseScriptAttributeArrayBufferIsUtf16Encoded is expected on parseAttributes.
    ///     </para>
    ///     <para>
    ///         Use JavascriptExternalArrayBuffer with Utf8/ASCII script source
    ///         for better performance and smaller memory footprint.
    ///     </para>
    /// </remarks>
    /// <param name="script">The script to serialize</param>
    /// <param name="buffer">ArrayBuffer</param>
    /// <param name="parseAttributes">Encoding for the script.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSerialize(
      JavaScriptValue script,
      out JavaScriptValue buffer,
      JavaScriptParseScriptAttributes parseAttributes
    );

    /// <summary>
    ///     Parses a serialized script and returns a function representing the script.
    ///     Provides the ability to lazy load the script source only if/when it is needed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="buffer">The serialized script as an ArrayBuffer (preferably ExternalArrayBuffer).</param>
    /// <param name="scriptLoadCallback">
    ///     Callback called when the source code of the script needs to be loaded.
    ///     This is an optional parameter, set to null if not needed.
    /// </param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    ///     This context will passed into scriptLoadCallback.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">A function representing the script code.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsParseSerialized(
      JavaScriptValue buffer,
      JavaScriptSerializedLoadScriptCallback scriptLoadCallback,
      JavaScriptSourceContext sourceContext,
      JavaScriptValue sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Runs a serialized script.
    ///     Provides the ability to lazy load the script source only if/when it is needed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///     The runtime will detach the data from the buffer and hold on to it until all
    ///     instances of any functions created from the buffer are garbage collected.
    ///     </para>
    /// </remarks>
    /// <param name="buffer">The serialized script as an ArrayBuffer (preferably ExternalArrayBuffer).</param>
    /// <param name="scriptLoadCallback">Callback called when the source code of the script needs to be loaded.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    ///     This context will passed into scriptLoadCallback.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">
    ///     The result of running the script, if any. This parameter can be null.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsRunSerialized(
      JavaScriptValue buffer,
      JavaScriptSerializedLoadScriptCallback scriptLoadCallback,
      JavaScriptSourceContext sourceContext,
      JavaScriptValue sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Gets the state of a given Promise object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="promise">The Promise object.</param>
    /// <param name="state">The current state of the Promise.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetPromiseState(JavaScriptValue promise, out JavaScriptPromiseState state);

    /// <summary>
    ///     Gets the result of a given Promise object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="promise">The Promise object.</param>
    /// <param name="result">The result of the Promise.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetPromiseResult(JavaScriptValue promise, out JavaScriptValue result);

    /// <summary>
    ///     Creates a new JavaScript Promise object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="promise">The new Promise object.</param>
    /// <param name="resolveFunction">The function called to resolve the created Promise object.</param>
    /// <param name="rejectFunction">The function called to reject the created Promise object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreatePromise(
      out JavaScriptValue promise,
      out JavaScriptValue resolveFunction,
      out JavaScriptValue rejectFunction
    );

    /// <summary>
    ///     Creates a weak reference to a value.
    /// </summary>
    /// <param name="value">The value to be referenced.</param>
    /// <param name="weakRef">Weak reference to the value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateWeakReference(JavaScriptValue value, out JavaScriptWeakRef weakRef);

    /// <summary>
    ///     Gets a strong reference to the value referred to by a weak reference.
    /// </summary>
    /// <param name="weakRef">A weak reference.</param>
    /// <param name="value">Reference to the value, or <c>JS_INVALID_REFERENCE</c> if the value is
    ///     no longer available.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetWeakReferenceValue(JavaScriptWeakRef weakRef, out JavaScriptValue value);

    /// <summary>
    ///     Creates a Javascript SharedArrayBuffer object with shared content get from JsGetSharedArrayBufferContent.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="sharedContents">
    ///     The storage object of a SharedArrayBuffer which can be shared between multiple thread.
    /// </param>
    /// <param name="result">The new SharedArrayBuffer object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateSharedArrayBufferWithSharedContent(JavaScriptSharedArrayBufferContentHandle sharedContents, out JavaScriptValue result);

    /// <summary>
    ///     Get the storage object from a SharedArrayBuffer.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="sharedArrayBuffer">The SharedArrayBuffer object.</param>
    /// <param name="sharedContents">
    ///     The storage object of a SharedArrayBuffer which can be shared between multiple thread.
    ///     User should call JsReleaseSharedArrayBufferContentHandle after finished using it.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetSharedArrayBufferContent(JavaScriptValue sharedArrayBuffer, out JavaScriptSharedArrayBufferContentHandle sharedContents);

    /// <summary>
    ///     Decrease the reference count on a SharedArrayBuffer storage object.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="sharedContents">
    ///     The storage object of a SharedArrayBuffer which can be shared between multiple thread.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsReleaseSharedArrayBufferContentHandle(JavaScriptSharedArrayBufferContentHandle sharedContents);

    /// <summary>
    ///     Determines whether an object has a non-inherited property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that may contain the property.</param>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="hasOwnProperty">Whether the object has the non-inherited property.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsHasOwnProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, out bool hasOwnProperty);

    /// <summary>
    ///     Write JS string value into char string buffer without a null terminator
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When size of the `buffer` is unknown,
    ///         `buffer` argument can be nullptr.
    ///         In that case, `written` argument will return the length needed.
    ///     </para>
    ///     <para>
    ///         When start is out of range or &lt; 0, returns JsErrorInvalidArgument
    ///         and `written` will be equal to 0.
    ///         If calculated length is 0 (It can be due to string length or `start`
    ///         and length combination), then `written` will be equal to 0 and call
    ///         returns JsNoError
    ///     </para>
    ///     <para>
    ///         The JS string `value` will be converted one utf16 code point at a time,
    ///         and if it has code points that do not fit in one byte, those values
    ///         will be truncated.
    ///     </para>
    /// </remarks>
    /// <param name="value">JavascriptString value</param>
    /// <param name="start">Start offset of buffer</param>
    /// <param name="length">Number of characters to be written</param>
    /// <param name="buffer">Pointer to buffer</param>
    /// <param name="written">Total number of characters written</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCopyStringOneByte(
      JavaScriptValue value,
      int start,
      int length,
      StringBuilder buffer, // _Out_ char* buffer
      out UIntPtr written
    );

    /// <summary>
    ///     Obtains frequently used properties of a data view.
    /// </summary>
    /// <param name="dataView">The data view instance.</param>
    /// <param name="arrayBuffer">The ArrayBuffer backstore of the view.</param>
    /// <param name="byteOffset">The offset in bytes from the start of arrayBuffer referenced by the array.</param>
    /// <param name="byteLength">The number of bytes in the array.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetDataViewInfo(
      JavaScriptValue dataView,
      out JavaScriptValue arrayBuffer,
      out uint byteOffset,
      out uint byteLength
    );

    /// <summary>
    ///     Determine if one JavaScript value is less than another JavaScript value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This function is equivalent to the <c>&lt;</c> operator in Javascript.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="object1">The first object to compare.</param>
    /// <param name="object2">The second object to compare.</param>
    /// <param name="result">Whether object1 is less than object2.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsLessThan(JavaScriptValue object1, JavaScriptValue object2, out bool result);

    /// <summary>
    ///     Determine if one JavaScript value is less than or equal to another JavaScript value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This function is equivalent to the <c>&lt;=</c> operator in Javascript.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="object1">The first object to compare.</param>
    /// <param name="object2">The second object to compare.</param>
    /// <param name="result">Whether object1 is less than or equal to object2.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsLessThanOrEqual(JavaScriptValue object1, JavaScriptValue object2, out bool result);

    /// <summary>
    ///     Creates a new object (with prototype) that stores some external data.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="data">External data that the object will represent. May be null.</param>
    /// <param name="finalizeCallback">
    ///     A callback for when the object is finalized. May be null.
    /// </param>
    /// <param name="prototype">Prototype object or nullptr.</param>
    /// <param name="obj">The new object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsCreateExternalObjectWithPrototype(
      IntPtr data,
      JavaScriptFinalizeCallback finalizeCallback,
      JavaScriptValue prototype,
      out JavaScriptValue obj
    );

    /// <summary>
    ///     Gets an object's property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that contains the property.</param>
    /// <param name="key">The key (JavascriptString or JavascriptSymbol) to the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsObjectGetProperty(
      JavaScriptValue obj,
      JavaScriptValue key,
      out JavaScriptValue value
    );

    /// <summary>
    ///     Puts an object's property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that contains the property.</param>
    /// <param name="key">The key (JavascriptString or JavascriptSymbol) to the property.</param>
    /// <param name="value">The new value of the property.</param>
    /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsObjectSetProperty(
      JavaScriptValue obj,
      JavaScriptValue key,
      JavaScriptValue value,
      bool useStrictRules
    );

    /// <summary>
    ///     Determines whether an object has a property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that may contain the property.</param>
    /// <param name="key">The key (JavascriptString or JavascriptSymbol) to the property.</param>
    /// <param name="hasProperty">Whether the object (or a prototype) has the property.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsObjectHasProperty(
      JavaScriptValue obj,
      JavaScriptValue key,
      out bool hasProperty
    );

    /// <summary>
    ///     Defines a new object's own property from a property descriptor.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that has the property.</param>
    /// <param name="key">The key (JavascriptString or JavascriptSymbol) to the property.</param>
    /// <param name="propertyDescriptor">The property descriptor.</param>
    /// <param name="result">Whether the property was defined.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsObjectDefineProperty(
      JavaScriptValue obj,
      JavaScriptValue key,
      JavaScriptValue propertyDescriptor,
      out bool result
    );

    /// <summary>
    ///     Deletes an object's property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that contains the property.</param>
    /// <param name="key">The key (JavascriptString or JavascriptSymbol) to the property.</param>
    /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
    /// <param name="result">Whether the property was deleted.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsObjectDeleteProperty(
      JavaScriptValue obj,
      JavaScriptValue key,
      bool useStrictRules,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Determines whether an object has a non-inherited property.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="obj">The object that may contain the property.</param>
    /// <param name="key">The key (JavascriptString or JavascriptSymbol) to the property.</param>
    /// <param name="hasOwnProperty">Whether the object has the non-inherited property.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsObjectHasOwnProperty(
      JavaScriptValue obj,
      JavaScriptValue key,
      out bool hasOwnProperty
    );

    /// <summary>
    ///     Sets whether any action should be taken when a promise is rejected with no reactions
    ///     or a reaction is added to a promise that was rejected before it had reactions.
    ///     By default in either of these cases nothing occurs.
    ///     This function allows you to specify if something should occur and provide a callback
    ///     to implement whatever should occur.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="promiseRejectionTrackerCallback">The callback function being set.</param>
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSetHostPromiseRejectionTracker(
      JavaScriptHostPromiseRejectionTrackerCallback promiseRejectionTrackerCallback,
      IntPtr callbackState
    );

    /// <summary>
    ///     Retrieve the namespace object for a module.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context and that the module has already been evaluated.
    /// </remarks>
    /// <param name="requestModule">The JsModuleRecord for which the namespace is being requested.</param>
    /// <param name="moduleNamespace">A JsValueRef - the requested namespace object.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetModuleNamespace(
      JavaScriptModuleRecord requestModule,
      out JavaScriptValue moduleNamespace
    );

    /// <summary>
    ///     Determines if a provided object is a JavscriptProxy Object and
    ///     provides references to a Proxy's target and handler.
    /// </summary>
    /// <remarks>
    ///     Requires an active script context.
    ///     If object is not a Proxy object the target and handler parameters are not touched.
    ///     If nullptr is supplied for target or handler the function returns after
    ///     setting the isProxy value.
    ///     If the object is a revoked Proxy target and handler are set to JS_INVALID_REFERENCE.
    ///     If it is a Proxy object that has not been revoked target and handler are set to the
    ///     the object's target and handler.
    /// </remarks>
    /// <param name="obj">The object that may be a Proxy.</param>
    /// <param name="isProxy">Pointer to a Boolean - is the object a proxy?</param>
    /// <param name="target">Pointer to a JsValueRef - the object's target.</param>
    /// <param name="handler">Pointer to a JsValueRef - the object's handler.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsGetProxyProperties(
      JavaScriptValue obj,
      out bool isProxy,
      out JavaScriptValue target,
      out JavaScriptValue handler
    );

    /// <summary>
    ///     Parses a script and stores the generated parser state cache into a buffer which can be reused.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>JsSerializeParserState</c> parses a script and then stores a cache of the parser state
    ///     in a runtime-independent format. The parser state may be deserialized in any runtime along
    ///     with the same script to skip the initial parse phase.
    ///     </para>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Script source can be either JavascriptString or JavascriptExternalArrayBuffer.
    ///         In case it is an ExternalArrayBuffer, and the encoding of the buffer is Utf16,
    ///         JsParseScriptAttributeArrayBufferIsUtf16Encoded is expected on parseAttributes.
    ///     </para>
    ///     <para>
    ///         Use JavascriptExternalArrayBuffer with Utf8/ASCII script source
    ///         for better performance and smaller memory footprint.
    ///     </para>
    /// </remarks>
    /// <param name="scriptVal">The script to parse.</param>
    /// <param name="bufferVal">The buffer to put the serialized parser state cache into.</param>
    /// <param name="parseAttributes">Encoding for the script.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsSerializeParserState(
      JavaScriptValue scriptVal,
      out JavaScriptValue bufferVal,
      JavaScriptParseScriptAttributes parseAttributes
    );

    /// <summary>
    ///     Deserializes the cache of initial parser state and (along with the same
    ///     script source) executes the script and returns the result.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Requires an active script context.
    ///     </para>
    ///     <para>
    ///         Script source can be either JavascriptString or JavascriptExternalArrayBuffer.
    ///         In case it is an ExternalArrayBuffer, and the encoding of the buffer is Utf16,
    ///         JsParseScriptAttributeArrayBufferIsUtf16Encoded is expected on parseAttributes.
    ///     </para>
    ///     <para>
    ///         Use JavascriptExternalArrayBuffer with Utf8/ASCII script source
    ///         for better performance and smaller memory footprint.
    ///     </para>
    /// </remarks>
    /// <param name="script">The script to run.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from</param>
    /// <param name="parseAttributes">Attribute mask for parsing the script</param>
    /// <param name="parserState">
    ///     A buffer containing a cache of the parser state generated by <c>JsSerializeParserState</c>.
    /// </param>
    /// <param name="result">The result of the script, if any. This parameter can be null.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsRunScriptWithParserState(
      JavaScriptValue script,
      JavaScriptSourceContext sourceContext,
      JavaScriptValue sourceUrl,
      JavaScriptParseScriptAttributes parseAttributes,
      JavaScriptValue parserState,
      out JavaScriptValue result
    );

    #endregion // ChakraCore.h


    #region ChakraCommonWindows.h

    /// <summary>
    ///     Parses a script and returns a function representing the script.
    /// </summary>
    /// <remarks>
    ///     ### This API is Windows-only (see JsParse for cross-platform equivalent).
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="script">The script to parse.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">A function representing the script code.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsParseScript(
      string script,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Parses a script and returns a function representing the script.
    /// </summary>
    /// <remarks>
    ///     ### This API is Windows-only (see JsParse for cross-platform equivalent).
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="script">The script to parse.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="parseAttributes">Attribute mask for parsing the script</param>
    /// <param name="result">A function representing the script code.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsParseScriptWithAttributes(
      string script,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      JavaScriptParseScriptAttributes parseAttributes,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Executes a script.
    /// </summary>
    /// <remarks>
    ///     ### This API is Windows-only (see JsRun for cross-platform equivalent).
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="script">The script to run.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">The result of the script, if any. This parameter can be null.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsRunScript(
      string script,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Executes a module.
    /// </summary>
    /// <remarks>
    ///     ### This API is Windows-only.
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="script">The module script to parse and execute.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the module script came from.</param>
    /// <param name="result">The result of executing the module script, if any. This parameter can be null.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsExperimentalApiRunModule(
      string script,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Serializes a parsed script to a buffer than can be reused.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsSerialize for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     <c>JsSerializeScript</c> parses a script and then stores the parsed form of the script in a
    ///     runtime-independent format. The serialized script then can be deserialized in any
    ///     runtime without requiring the script to be re-parsed.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="script">The script to serialize.</param>
    /// <param name="buffer">The buffer to put the serialized script into. Can be null.</param>
    /// <param name="bufferSize">
    ///     On entry, the size of the buffer, in bytes; on exit, the size of the buffer, in bytes,
    ///     required to hold the serialized script.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsSerializeScript(
      string script,
      byte[] buffer,
      ref ulong bufferSize // _Inout_ unsigned int *bufferSize
    );

    /// <summary>
    ///     Parses a serialized script and returns a function representing the script.
    ///     Provides the ability to lazy load the script source only if/when it is needed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsParseSerialized for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///     The runtime will hold on to the buffer until all instances of any functions created from
    ///     the buffer are garbage collected.  It will then call scriptUnloadCallback to inform the
    ///     caller it is safe to release.
    ///     </para>
    /// </remarks>
    /// <param name="scriptLoadCallback">Callback called when the source code of the script needs to be loaded.</param>
    /// <param name="scriptUnloadCallback">Callback called when the serialized script and source code are no longer needed.</param>
    /// <param name="buffer">The serialized script.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    ///     This context will passed into scriptLoadCallback and scriptUnloadCallback.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">A function representing the script code.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsParseSerializedScriptWithCallback(
      JavaScriptSerializedScriptLoadSourceCallback scriptLoadCallback,
      JavaScriptSerializedScriptUnloadCallback scriptUnloadCallback,
      byte[] buffer,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Runs a serialized script.
    ///     Provides the ability to lazy load the script source only if/when it is needed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsRunSerialized for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///     The runtime will hold on to the buffer until all instances of any functions created from
    ///     the buffer are garbage collected.  It will then call scriptUnloadCallback to inform the
    ///     caller it is safe to release.
    ///     </para>
    /// </remarks>
    /// <param name="scriptLoadCallback">Callback called when the source code of the script needs to be loaded.</param>
    /// <param name="scriptUnloadCallback">Callback called when the serialized script and source code are no longer needed.</param>
    /// <param name="buffer">The serialized script.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    ///     This context will passed into scriptLoadCallback and scriptUnloadCallback.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">
    ///     The result of running the script, if any. This parameter can be null.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsRunSerializedScriptWithCallback(
      JavaScriptSerializedScriptLoadSourceCallback scriptLoadCallback,
      JavaScriptSerializedScriptUnloadCallback scriptUnloadCallback,
      byte[] buffer,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Parses a serialized script and returns a function representing the script.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsParseSerialized for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///     The runtime will hold on to the buffer until all instances of any functions created from
    ///     the buffer are garbage collected.
    ///     </para>
    /// </remarks>
    /// <param name="script">The script to parse.</param>
    /// <param name="buffer">The serialized script.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">A function representing the script code.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsParseSerializedScript(
      string script,
      byte[] buffer,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Runs a serialized script.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsParseSerialized for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///     The runtime will hold on to the buffer until all instances of any functions created from
    ///     the buffer are garbage collected.
    ///     </para>
    /// </remarks>
    /// <param name="script">The source code of the serialized script.</param>
    /// <param name="buffer">The serialized script.</param>
    /// <param name="sourceContext">
    ///     A cookie identifying the script that can be used by debuggable script contexts.
    /// </param>
    /// <param name="sourceUrl">The location the script came from.</param>
    /// <param name="result">
    ///     The result of running the script, if any. This parameter can be null.
    /// </param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsRunSerializedScript(
      string script,
      byte[] buffer,
      JavaScriptSourceContext sourceContext,
      string sourceUrl,
      out JavaScriptValue result
    );

    /// <summary>
    ///     Gets the property ID associated with the name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsCreatePropertyId for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     Property IDs are specific to a context and cannot be used across contexts.
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="name">
    ///     The name of the property ID to get or create. The name may consist of only digits.
    /// </param>
    /// <param name="propertyId">The property ID in this runtime for the given name.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsGetPropertyIdFromName(
      string name,
      out JavaScriptPropertyId propertyId
    );

    /// <summary>
    ///     Gets the name associated with the property ID.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsCopyPropertyId for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    ///     <para>
    ///     The returned buffer is valid as long as the runtime is alive and cannot be used
    ///     once the runtime has been disposed.
    ///     </para>
    /// </remarks>
    /// <param name="propertyId">The property ID to get the name of.</param>
    /// <param name="name">The name associated with the property ID.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsGetPropertyNameFromId(
      JavaScriptPropertyId propertyId,
      out string name
    );

    /// <summary>
    ///     Creates a string value from a string pointer.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsCreateString/JsCreateStringUtf16 for cross-platform equivalent).
    ///     </para>
    ///     Requires an active script context.
    /// </remarks>
    /// <param name="stringValue">The string pointer to convert to a string value.</param>
    /// <param name="stringLength">The length of the string to convert.</param>
    /// <param name="value">The new string value.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName, CharSet = CharSet.Unicode)]
    public static extern JavaScriptErrorCode JsPointerToString(
      string value,
      UIntPtr stringLength,
      out JavaScriptValue stringValue
    );

    /// <summary>
    ///     Retrieves the string pointer of a string value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ### This API is Windows-only (see JsCopyString/JsCopyStringUtf16 for cross-platform equivalent).
    ///     </para>
    ///     <para>
    ///     This function retrieves the string pointer of a string value. It will fail with
    ///     <c>JsErrorInvalidArgument</c> if the type of the value is not string. The lifetime
    ///     of the string returned will be the same as the lifetime of the value it came from, however
    ///     the string pointer is not considered a reference to the value (and so will not keep it
    ///     from being collected).
    ///     </para>
    ///     <para>
    ///     Requires an active script context.
    ///     </para>
    /// </remarks>
    /// <param name="value">The string value to convert to a string pointer.</param>
    /// <param name="stringValue">The string pointer.</param>
    /// <param name="stringLength">The length of the string.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsStringToPointer(
      JavaScriptValue value,
      out IntPtr stringValue,
      out UIntPtr stringLength
    );

    #endregion // ChakraCommonWindows.h


    #region ChakraDebug.h

    /// <summary>
    ///     Starts debugging in the given runtime.
    /// </summary>
    /// <param name="runtimeHandle">Runtime to put into debug mode.</param>
    /// <param name="debugEventCallback">Registers a callback to be called on every JsDiagDebugEvent.</param>
    /// <param name="callbackState">User provided state that will be passed back to the callback.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The runtime should be active on the current thread and should not be in debug state.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagStartDebugging(
      JavaScriptRuntime runtimeHandle,
      JsDiagDebugEventCallback debugEventCallback,
      IntPtr callbackState
    );

    /// <summary>
    ///     Stops debugging in the given runtime.
    /// </summary>
    /// <param name="runtimeHandle">Runtime to stop debugging.</param>
    /// <param name="callbackState">User provided state that was passed in JsDiagStartDebugging.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The runtime should be active on the current thread and in debug state.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagStopDebugging(JavaScriptRuntime runtimeHandle, out IntPtr callbackState);

    /// <summary>
    ///     Request the runtime to break on next JavaScript statement.
    /// </summary>
    /// <param name="runtimeHandle">Runtime to request break.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The runtime should be in debug state. This API can be called from another runtime.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagRequestAsyncBreak(JavaScriptRuntime runtimeHandle);

    /// <summary>
    ///     List all breakpoints in the current runtime.
    /// </summary>
    /// <param name="breakpoints">Array of breakpoints.</param>
    /// <remarks>
    ///     <para>
    ///     [{
    ///         "breakpointId" : 1,
    ///         "scriptId" : 1,
    ///         "line" : 0,
    ///         "column" : 62
    ///     }]
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can be called when runtime is at a break or running.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetBreakpoints(out JavaScriptValue breakpoints);


    /// <summary>
    ///     Sets breakpoint in the specified script at give location.
    /// </summary>
    /// <param name="scriptId">Id of script from JsDiagGetScripts or JsDiagGetSource to put breakpoint.</param>
    /// <param name="lineNumber">0 based line number to put breakpoint.</param>
    /// <param name="columnNumber">0 based column number to put breakpoint.</param>
    /// <param name="breakpoint">Breakpoint object with id, line and column if success.</param>
    /// <remarks>
    ///     <para>
    ///     {
    ///         "breakpointId" : 1,
    ///         "line" : 2,
    ///         "column" : 4
    ///     }
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can be called when runtime is at a break or running.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagSetBreakpoint(uint scriptId, uint lineNumber, uint columnNumber, out JavaScriptValue breakpoint);


    /// <summary>
    ///     Remove a breakpoint.
    /// </summary>
    /// <param name="breakpointId">Breakpoint id returned from JsDiagSetBreakpoint.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can be called when runtime is at a break or running.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagRemoveBreakpoint(uint breakpointId);

    /// <summary>
    ///     Sets break on exception handling.
    /// </summary>
    /// <param name="runtimeHandle">Runtime to set break on exception attributes.</param>
    /// <param name="exceptionAttributes">Mask of JsDiagBreakOnExceptionAttributes to set.</param>
    /// <remarks>
    ///     <para>
    ///         If this API is not called the default value is set to JsDiagBreakOnExceptionAttributeUncaught in the runtime.
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The runtime should be in debug state. This API can be called from another runtime.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagSetBreakOnException(JavaScriptRuntime runtimeHandle, JavaScriptDiagBreakOnExceptionAttributes exceptionAttributes);

    /// <summary>
    ///     Gets break on exception setting.
    /// </summary>
    /// <param name="runtimeHandle">Runtime from which to get break on exception attributes, should be in debug mode.</param>
    /// <param name="exceptionAttributes">Mask of JsDiagBreakOnExceptionAttributes.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The runtime should be in debug state. This API can be called from another runtime.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetBreakOnException(JavaScriptRuntime runtimeHandle, out JavaScriptDiagBreakOnExceptionAttributes exceptionAttributes);

    /// <summary>
    ///     Sets the step type in the runtime after a debug break.
    /// </summary>
    /// <remarks>
    ///     Requires to be at a debug break.
    /// </remarks>
    /// <param name="resumeType">Type of JsDiagStepType.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can only be called when runtime is at a break.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagSetStepType(JavaScriptDiagStepType stepType);

    /// <summary>
    ///     Gets list of scripts.
    /// </summary>
    /// <param name="scriptsArray">Array of script objects.</param>
    /// <remarks>
    ///     <para>
    ///     [{
    ///         "scriptId" : 2,
    ///         "fileName" : "c:\\Test\\Test.js",
    ///         "lineCount" : 4,
    ///         "sourceLength" : 111
    ///       }, {
    ///         "scriptId" : 3,
    ///         "parentScriptId" : 2,
    ///         "scriptType" : "eval code",
    ///         "lineCount" : 1,
    ///         "sourceLength" : 12
    ///     }]
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can be called when runtime is at a break or running.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetScripts(out JavaScriptValue scriptsArray);

    /// <summary>
    ///     Gets source for a specific script identified by scriptId from JsDiagGetScripts.
    /// </summary>
    /// <param name="scriptId">Id of the script.</param>
    /// <param name="source">Source object.</param>
    /// <remarks>
    ///     <para>
    ///     {
    ///         "scriptId" : 1,
    ///         "fileName" : "c:\\Test\\Test.js",
    ///         "lineCount" : 12,
    ///         "sourceLength" : 15154,
    ///         "source" : "var x = 1;"
    ///     }
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can be called when runtime is at a break or running.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetSource(uint scriptId, out JavaScriptValue source);

    /// <summary>
    ///     Gets the source information for a function object.
    /// </summary>
    /// <param name="function">JavaScript function.</param>
    /// <param name="functionPosition">Function position - scriptId, start line, start column, line number of first statement, column number of first statement.</param>
    /// <remarks>
    ///     <para>
    ///     {
    ///         "scriptId" : 1,
    ///         "fileName" : "c:\\Test\\Test.js",
    ///         "line" : 1,
    ///         "column" : 2,
    ///         "firstStatementLine" : 6,
    ///         "firstStatementColumn" : 0
    ///     }
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     This API can be called when runtime is at a break or running.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetFunctionPosition(JavaScriptValue function, out JavaScriptValue functionPosition);

    /// <summary>
    ///     Gets the stack trace information.
    /// </summary>
    /// <param name="stackTrace">Stack trace information.</param>
    /// <remarks>
    ///     <para>
    ///     [{
    ///         "index" : 0,
    ///         "scriptId" : 2,
    ///         "line" : 3,
    ///         "column" : 0,
    ///         "sourceLength" : 9,
    ///         "sourceText" : "var x = 1",
    ///         "functionHandle" : 1
    ///     }]
    ///    </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can only be called when runtime is at a break.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetStackTrace(out JavaScriptValue stackTrace);

    /// <summary>
    ///     Gets the list of properties corresponding to the frame.
    /// </summary>
    /// <param name="stackFrameIndex">Index of stack frame from JsDiagGetStackTrace.</param>
    /// <param name="properties">Object of properties array (properties, scopes and globals).</param>
    /// <remarks>
    ///     <para>
    ///     propertyAttributes is a bit mask of
    ///         NONE = 0x1,
    ///         HAVE_CHILDRENS = 0x2,
    ///         READ_ONLY_VALUE = 0x4,
    ///         IN_TDZ = 0x8,
    ///     </para>
    ///     <para>
    ///     {
    ///         "thisObject": {
    ///             "name": "this",
    ///             "type" : "object",
    ///             "className" : "Object",
    ///             "display" : "{...}",
    ///             "propertyAttributes" : 1,
    ///             "handle" : 306
    ///         },
    ///         "exception" : {
    ///             "name" : "{exception}",
    ///             "type" : "object",
    ///             "display" : "'a' is undefined",
    ///             "className" : "Error",
    ///             "propertyAttributes" : 1,
    ///             "handle" : 307
    ///         }
    ///         "arguments" : {
    ///             "name" : "arguments",
    ///             "type" : "object",
    ///             "display" : "{...}",
    ///             "className" : "Object",
    ///             "propertyAttributes" : 1,
    ///             "handle" : 190
    ///         },
    ///         "returnValue" : {
    ///             "name" : "[Return value]",
    ///             "type" : "undefined",
    ///             "propertyAttributes" : 0,
    ///             "handle" : 192
    ///         },
    ///         "functionCallsReturn" : [{
    ///                 "name" : "[foo1 returned]",
    ///                 "type" : "number",
    ///                 "value" : 1,
    ///                 "propertyAttributes" : 2,
    ///                 "handle" : 191
    ///             }
    ///         ],
    ///         "locals" : [],
    ///         "scopes" : [{
    ///                 "index" : 0,
    ///                 "handle" : 193
    ///             }
    ///         ],
    ///         "globals" : {
    ///             "handle" : 194
    ///         }
    ///     }
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can only be called when runtime is at a break.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetStackProperties(uint stackFrameIndex, out JavaScriptValue properties);

    /// <summary>
    ///     Gets the list of children of a handle.
    /// </summary>
    /// <param name="objectHandle">Handle of object.</param>
    /// <param name="fromCount">0-based from count of properties, usually 0.</param>
    /// <param name="totalCount">Number of properties to return.</param>
    /// <param name="propertiesObject">Array of properties.</param>
    /// <remarks>Handle should be from objects returned from call to JsDiagGetStackProperties.</remarks>
    /// <remarks>For scenarios where object have large number of properties totalCount can be used to control how many properties are given.</remarks>
    /// <remarks>
    ///     <para>
    ///     {
    ///         "totalPropertiesOfObject": 10,
    ///         "properties" : [{
    ///                 "name" : "__proto__",
    ///                 "type" : "object",
    ///                 "display" : "{...}",
    ///                 "className" : "Object",
    ///                 "propertyAttributes" : 1,
    ///                 "handle" : 156
    ///             }
    ///         ],
    ///         "debuggerOnlyProperties" : [{
    ///                 "name" : "[Map]",
    ///                 "type" : "string",
    ///                 "value" : "size = 0",
    ///                 "propertyAttributes" : 2,
    ///                 "handle" : 157
    ///             }
    ///         ]
    ///     }
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can only be called when runtime is at a break.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetProperties(uint objectHandle, uint fromCount, uint totalCount, out JavaScriptValue propertiesObject);

    /// <summary>
    ///     Gets the object corresponding to handle.
    /// </summary>
    /// <param name="objectHandle">Handle of object.</param>
    /// <param name="handleObject">Object corresponding to the handle.</param>
    /// <remarks>
    ///     <para>
    ///     {
    ///         "scriptId" : 24,
    ///          "line" : 1,
    ///          "column" : 63,
    ///          "name" : "foo",
    ///          "type" : "function",
    ///          "handle" : 2
    ///     }
    ///    </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can only be called when runtime is at a break.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagGetObjectFromHandle(uint objectHandle, out JavaScriptValue handleObject);

    /// <summary>
    ///     Evaluates an expression on given frame.
    /// </summary>
    /// <param name="expression">
    ///     Javascript String or ArrayBuffer (incl. ExternalArrayBuffer).
    /// </param>
    /// <param name="stackFrameIndex">Index of stack frame on which to evaluate the expression.</param>
    /// <param name="parseAttributes">
    ///     Defines how `expression` (JsValueRef) should be parsed.
    ///     - `JsParseScriptAttributeNone` when `expression` is a Utf8 encoded ArrayBuffer and/or a Javascript String (encoding independent)
    ///     - `JsParseScriptAttributeArrayBufferIsUtf16Encoded` when `expression` is Utf16 Encoded ArrayBuffer
    ///     - `JsParseScriptAttributeLibraryCode` has no use for this function and has similar effect with `JsParseScriptAttributeNone`
    /// </param>
    /// <param name="forceSetValueProp">Forces the result to contain the raw value of the expression result.</param>
    /// <param name="evalResult">Result of evaluation.</param>
    /// <remarks>
    ///     <para>
    ///     evalResult when evaluating 'this' and return is JsNoError
    ///     {
    ///         "name" : "this",
    ///         "type" : "object",
    ///         "className" : "Object",
    ///         "display" : "{...}",
    ///         "propertyAttributes" : 1,
    ///         "handle" : 18
    ///     }
    ///
    ///     evalResult when evaluating a script which throws JavaScript error and return is JsErrorScriptException
    ///     {
    ///         "name" : "a.b.c",
    ///         "type" : "object",
    ///         "className" : "Error",
    ///         "display" : "'a' is undefined",
    ///         "propertyAttributes" : 1,
    ///         "handle" : 18
    ///     }
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, evalResult will contain the result
    ///     The code <c>JsErrorScriptException</c> if evaluate generated a JavaScript exception, evalResult will contain the error details
    ///     Other error code for invalid parameters or API was not called at break
    /// </returns>
    /// <remarks>
    ///     The current runtime should be in debug state. This API can only be called when runtime is at a break.
    /// </remarks>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsDiagEvaluate(
      JavaScriptValue expression,
      uint stackFrameIndex,
      JavaScriptParseScriptAttributes parseAttributes,
      bool forceSetValueProp,
      out JavaScriptValue evalResult
    );


    #region Time Travel Debugging

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Creates a new runtime in Record Mode.
    /// </summary>
    /// <param name="attributes">The attributes of the runtime to be created.</param>
    /// <param name="enableDebugging">A flag to enable debugging during record.</param>
    /// <param name="snapInterval">The interval to wait between snapshots (measured in millis).</param>
    /// <param name="snapHistoryLength">The amount of history to maintain before discarding -- measured in number of snapshots and controls how far back in time a trace can be reversed.</param>
    /// <param name="openResourceStream">The <c>TTDOpenResourceStreamCallback</c> function for generating a JsTTDStreamHandle to read/write serialized data.</param>
    /// <param name="writeBytesToStream">The <c>JsTTDWriteBytesToStreamCallback</c> function for writing bytes to a JsTTDStreamHandle.</param>
    /// <param name="flushAndCloseStream">The <c>JsTTDFlushAndCloseStreamCallback</c> function for flushing and closing a JsTTDStreamHandle as needed.</param>
    /// <param name="threadService">The thread service for the runtime. Can be null.</param>
    /// <param name="runtime">The runtime created.</param>
    /// <remarks>
    ///     <para>See <c>JsCreateRuntime</c> for additional information.</para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDCreateRecordRuntime(
      JavaScriptRuntimeAttributes attributes,
      bool enableDebugging,
      UIntPtr snapInterval,
      UIntPtr snapHistoryLength,
      TTDOpenResourceStreamCallback openResourceStream,
      JavaScriptTTDWriteBytesToStreamCallback writeBytesToStream,
      JavaScriptTTDFlushAndCloseStreamCallback flushAndCloseStream,
      JavaScriptThreadServiceCallback threadService,
      out JavaScriptRuntime runtime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Creates a new runtime in Debug Mode.
    /// </summary>
    /// <param name="attributes">The attributes of the runtime to be created.</param>
    /// <param name="infoUri">The uri where the recorded Time-Travel data should be loaded from.</param>
    /// <param name="enableDebugging">A flag to enable additional debugging operation support during replay.</param>
    /// <param name="openResourceStream">The <c>TTDOpenResourceStreamCallback</c> function for generating a JsTTDStreamHandle to read/write serialized data.</param>
    /// <param name="readBytesFromStream">The <c>JsTTDReadBytesFromStreamCallback</c> function for reading bytes from a JsTTDStreamHandle.</param>
    /// <param name="flushAndCloseStream">The <c>JsTTDFlushAndCloseStreamCallback</c> function for flushing and closing a JsTTDStreamHandle as needed.</param>
    /// <param name="threadService">The thread service for the runtime. Can be null.</param>
    /// <param name="runtime">The runtime created.</param>
    /// <remarks>
    ///     <para>See <c>JsCreateRuntime</c> for additional information.</para>
    /// </remarks>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDCreateReplayRuntime(
      JavaScriptRuntimeAttributes attributes,
      string infoUri,
      UIntPtr infoUriCount,
      bool enableDebugging,
      TTDOpenResourceStreamCallback openResourceStream,
      JavaScriptTTDReadBytesFromStreamCallback readBytesFromStream,
      JavaScriptTTDFlushAndCloseStreamCallback flushAndCloseStream,
      JavaScriptThreadServiceCallback threadService,
      out JavaScriptRuntime runtime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Creates a script context that takes the TTD mode from the log or explicitly is not in TTD mode (regular takes mode from currently active script).
    /// </summary>
    /// <param name="runtime">The runtime the script context is being created in.</param>
    /// <param name="useRuntimeTTDMode">Set to true to use runtime TTD mode false to explicitly be non-TTD context.</param>
    /// <param name="newContext">The created script context.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDCreateContext(
      JavaScriptRuntime runtimeHandle,
      bool useRuntimeTTDMode,
      out JavaScriptContext newContext
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the time-travel system that a context has been identified as dead by the gc (and is being de-allocated).
    /// </summary>
    /// <param name="context">The script context that is now dead.</param>
    /// <returns>
    ///     The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDNotifyContextDestroy(
      JavaScriptContext context
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Start Time-Travel record or replay at next turn of event loop.
    /// </summary>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDStart();

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Stop Time-Travel record or replay.
    /// </summary>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDStop();

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Pause Time-Travel recording before executing code on behalf of debugger or other diagnostic/telemetry.
    /// </summary>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDPauseTimeTravelBeforeRuntimeOperation();

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     ReStart Time-Travel recording after executing code on behalf of debugger or other diagnostic/telemetry.
    /// </summary>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDReStartTimeTravelAfterRuntimeOperation();

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the Js runtime we are at a safe yield point in the event loop (i.e. no locals on the stack and we can process as desired).
    /// </summary>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDNotifyYield();

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the TTD runtime that we are doing a weak add on a reference (we may use this in external API calls and the release will happen in a GC callback).
    /// </summary>
    /// <param name="value">The value we are adding the ref to.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDNotifyLongLivedReferenceAdd(JavaScriptValue value);

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the Js runtime the host is aborting the process and what the status code is.
    /// </summary>
    /// <param name="statusCode">The exit status code.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDHostExit(int statusCode);

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the event log that the contents of one buffer have been copied to a second buffer.
    /// </summary>
    /// <param name="dst">The buffer that was written into.</param>
    /// <param name="dstIndex">The first index modified.</param>
    /// <param name="src">The buffer that was copied from.</param>
    /// <param name="srcIndex">The first index copied.</param>
    /// <param name="count">The number of bytes copied.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDRawBufferCopySyncIndirect(
      JavaScriptValue dst,
      UIntPtr dstIndex,
      JavaScriptValue src,
      UIntPtr srcIndex,
      UIntPtr count
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the event log that the contents of a naked byte* buffer passed to the host have been modified synchronously.
    /// </summary>
    /// <param name="buffer">The buffer that was modified.</param>
    /// <param name="index">The first index modified.</param>
    /// <param name="count">The number of bytes written.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDRawBufferModifySyncIndirect(
      JavaScriptValue buffer,
      UIntPtr index,
      UIntPtr count
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Get info for notifying the TTD system that a raw buffer it shares with the host has been modified.
    /// </summary>
    /// <param name="instance">The array buffer we want to monitor for contents modification.</param>
    /// <param name="initialModPos">The first position in the buffer that may be modified.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDRawBufferAsyncModificationRegister(
      JavaScriptValue instance,
      byte[] initialModPos
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Notify the event log that the contents of a naked byte* buffer passed to the host have been modified asynchronously.
    /// </summary>
    /// <param name="finalModPos">One past the last modified position in the buffer.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDRawBufferAsyncModifyComplete(
      byte[] finalModPos
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     A check for unimplemented TTD actions in the host.
    ///     This API is a TEMPORARY API while we complete the implementation of TTD support in the Node host and will be deleted once that is complete.
    /// </summary>
    /// <param name="msg">The message to print if we should be catching this as a TTD operation.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDCheckAndAssertIfTTDRunning(
      string msg
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Before calling JsTTDMoveToTopLevelEvent (which inflates a snapshot and replays) check to see if we want to reset the script context.
    ///     We reset the script context if the move will require inflating from a different snapshot that the last one.
    /// </summary>
    /// <param name="runtimeHandle">The runtime handle that the script is executing in.</param>
    /// <param name="moveMode">Flags controlling the way the move it performed and how other parameters are interpreted.</param>
    /// <param name="kthEvent">When <c>moveMode == JsTTDMoveKthEvent</c> indicates which event, otherwise this parameter is ignored.</param>
    /// <param name="targetEventTime">The event time we want to move to or -1 if not relevant.</param>
    /// <param name="targetStartSnapTime">Out parameter with the event time of the snapshot that we should inflate from.</param>
    /// <param name="targetEndSnapTime">Optional Out parameter with the snapshot time following the event.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDGetSnapTimeTopLevelEventMove(
      JavaScriptRuntime runtimeHandle,
      JavaScriptTTDMoveMode moveMode,
      uint kthEvent,
      ref long targetEventTime,
      out long targetStartSnapTime,
      out long targetEndSnapTime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Get the snapshot interval that bounds the target event time.
    /// </summary>
    /// <param name="runtimeHandle">The runtime handle that the script is executing in.</param>
    /// <param name="targetEventTime">The event time we want to get the interval for.</param>
    /// <param name="startSnapTime">The snapshot time that comes before the desired event.</param>
    /// <param name="endSnapTime">The snapshot time that comes after the desired event (-1 if the leg ends before a snapshot appears).</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDGetSnapShotBoundInterval(
      JavaScriptRuntime runtimeHandle,
      long targetEventTime,
      out long startSnapTime,
      out long endSnapTime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Get the snapshot interval that precedes the one given by currentSnapStartTime (or -1 if there is no such interval).
    /// </summary>
    /// <param name="runtimeHandle">The runtime handle that the script is executing in.</param>
    /// <param name="currentSnapStartTime">The current snapshot interval start time.</param>
    /// <param name="previousSnapTime">The resulting previous snapshot interval start time or -1 if no such time.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDGetPreviousSnapshotInterval(
      JavaScriptRuntime runtimeHandle,
      long currentSnapStartTime,
      out long previousSnapTime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     During debug operations some additional information is populated during replay. This runs the code between the given
    ///     snapshots to populate this information which may be needed by the debugger to determine time-travel jump targets.
    /// </summary>
    /// <param name="runtimeHandle">The runtime handle that the script is executing in.</param>
    ///<param name = "startSnapTime">The snapshot time that we will start executing from.< / param>
    ///<param name = "endSnapTime">The snapshot time that we will stop at (or -1 if we want to run to the end).< / param>
    /// <param name="moveMode">Additional flags for controling how the move is done.</param>
    /// <param name="newTargetEventTime">The updated target event time set according to the moveMode (-1 if not found).</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDPreExecuteSnapShotInterval(
      JavaScriptRuntime runtimeHandle,
      long startSnapTime,
      long endSnapTime,
      JavaScriptTTDMoveMode moveMode,
      out long newTargetEventTime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Move to the given top-level call event time (assuming JsTTDPrepContextsForTopLevelEventMove) was called previously to reset any script contexts.
    ///     This also computes the ready-to-run snapshot if needed.
    /// </summary>
    /// <param name="runtimeHandle">The runtime handle that the script is executing in.</param>
    /// <param name="moveMode">Additional flags for controling how the move is done.</param>
    /// <param name="snapshotTime">The event time that we will start executing from to move to the given target time.</param>
    /// <param name="eventTime">The event that we want to move to.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDMoveToTopLevelEvent(
      JavaScriptRuntime runtimeHandle,
      JavaScriptTTDMoveMode moveMode,
      long snapshotTime,
      long eventTime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Execute from the current point in the log to the end returning the error code.
    /// </summary>
    /// <param name="moveMode">Additional flags for controling how the move is done.</param>
    /// <param name="rootEventTime">The event time that we should move to next or notification (-1) that replay has ended.</param>
    /// <returns>
    ///     If the debugger requested an abort the code is JsNoError -- rootEventTime is the target event time we need to move to and re - execute from.
    ///     If we aborted at the end of the replay log the code is JsNoError -- rootEventTime is -1.
    ///     If there was an unhandled script exception the code is JsErrorCategoryScript.
    /// </returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDReplayExecution(
      ref JavaScriptTTDMoveMode moveMode,
      out long rootEventTime
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Enable or disable autotrace ability from JsRT.
    /// </summary>
    /// <param name="status">True to enable autotracing false to disable it.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDDiagSetAutoTraceStatus(
      bool status
    );

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     A way for the debugger to programatically write a trace when it is at a breakpoint.
    /// </summary>
    /// <param name="uri">The URI that the log should be written into.</param>
    /// <param name="uriLength">The length of the uri array that the host passed in for storing log info.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    [DllImport(DllName)]
    public static extern JavaScriptErrorCode JsTTDDiagWriteLog(
      string uri,
      UIntPtr uriLength
    );

    #endregion // Time Travel Debugging

    #endregion // ChakraDebug.h
  }
}
