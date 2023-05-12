using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A Chakra runtime.
  /// </summary>
  /// <remarks>
  ///     <para>
  ///     Each Chakra runtime has its own independent execution engine, JIT compiler, and garbage
  ///     collected heap. As such, each runtime is completely isolated from other runtimes.
  ///     </para>
  ///     <para>
  ///     Runtimes can be used on any thread, but only one thread can call into a runtime at any
  ///     time.
  ///     </para>
  ///     <para>
  ///     NOTE: A JavaScriptRuntime, unlike other objects in the Chakra hosting API, is not
  ///     garbage collected since it contains the garbage collected heap itself. A runtime will
  ///     continue to exist until Dispose is called.
  ///     </para>
  /// </remarks>
  public struct JavaScriptRuntime : IDisposable {
    /// <summary>
    /// The handle.
    /// </summary>
    private IntPtr handle;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JavaScriptRuntime"/> struct.
    /// </summary>
    /// <param name="handle">The handle.</param>
    internal JavaScriptRuntime(IntPtr handle) {
      this.handle = handle;
    }

    /// <summary>
    ///     Gets an invalid runtime.
    /// </summary>
    public static JavaScriptRuntime Invalid {
      get { return new JavaScriptRuntime(IntPtr.Zero); }
    }

    /// <summary>
    ///     Gets a value indicating whether the runtime is valid.
    /// </summary>
    public bool IsValid {
      get { return handle != IntPtr.Zero; }
    }

    /// <summary>
    ///     Creates a new runtime.
    /// </summary>
    /// <param name="attributes">The attributes of the runtime to be created.</param>
    /// <param name="threadService">The thread service for the runtime. Can be null.</param>
    /// <remarks>In the edge-mode binary, chakra.dll, this function lacks the <c>runtimeVersion</c>
    /// parameter (compare to jsrt9.h).</remarks>
    /// <returns>
    ///     The runtime created.
    /// </returns>
    public static JavaScriptRuntime Create(
      JavaScriptRuntimeAttributes attributes = JavaScriptRuntimeAttributes.None,
      JavaScriptThreadServiceCallback threadServiceCallback = null
    ) {
      JavaScriptRuntime handle;
      Native.ThrowIfError(Native.JsCreateRuntime(attributes, threadServiceCallback, out handle));
      return handle;
    }

    /// <summary>
    ///     Performs a full garbage collection.
    /// </summary>
    public void CollectGarbage() {
      Native.ThrowIfError(Native.JsCollectGarbage(this));
    }

    /// <summary>
    ///     Disposes a runtime.
    /// </summary>
    /// <remarks>
    ///     Once a runtime has been disposed, all resources owned by it are invalid and cannot be used.
    ///     If the runtime is active (i.e. it is set to be current on a particular thread), it cannot
    ///     be disposed.
    /// </remarks>
    public void Dispose() {
      if (IsValid) {
        Native.ThrowIfError(Native.JsSetCurrentContext(JavaScriptContext.Invalid));
        Native.ThrowIfError(Native.JsDisposeRuntime(this));
      }

      handle = IntPtr.Zero;
    }

    /// <summary>
    ///     Gets the current memory usage for a runtime.
    /// </summary>
    /// <remarks>
    ///     Memory usage can be always be retrieved, regardless of whether or not the runtime is active
    ///     on another thread.
    /// </remarks>
    /// <returns>
    ///     The runtime's current memory usage, in bytes.
    /// </returns>
    public UIntPtr MemoryUsage {
      get {
        UIntPtr memoryUsage;
        Native.ThrowIfError(Native.JsGetRuntimeMemoryUsage(this, out memoryUsage));
        return memoryUsage;
      }
    }

    /// <summary>
    ///     Gets or sets the current memory limit for a runtime.
    /// </summary>
    /// <remarks>
    ///     The memory limit of a runtime can be always be retrieved, regardless of whether or not the
    ///     runtime is active on another thread.
    /// </remarks>
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
    /// <param name="memoryLimit">
    ///     The new runtime memory limit, in bytes, or -1 for no memory limit.
    /// </param>
    /// <returns>
    ///     The runtime's current memory limit, in bytes, or -1 if no limit has been set.
    /// </returns>
    public UIntPtr MemoryLimit {
      get {
        UIntPtr memoryLimit;
        Native.ThrowIfError(Native.JsGetRuntimeMemoryLimit(this, out memoryLimit));
        return memoryLimit;
      }

      set {
        Native.ThrowIfError(Native.JsSetRuntimeMemoryLimit(this, value));
      }
    }

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
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="allocationCallback">
    ///     Memory allocation callback to be called for memory allocation events.
    /// </param>
    public void SetMemoryAllocationCallback(IntPtr callbackState, JavaScriptMemoryAllocationCallback allocationCallback) {
      Native.ThrowIfError(Native.JsSetRuntimeMemoryAllocationCallback(this, callbackState, allocationCallback));
    }

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
    /// <param name="callbackState">
    ///     User provided state that will be passed back to the callback.
    /// </param>
    /// <param name="beforeCollectCallback">The callback function being set.</param>
    public void SetBeforeCollectCallback(IntPtr callbackState, JavaScriptBeforeCollectCallback beforeCollectCallback) {
      Native.ThrowIfError(Native.JsSetRuntimeBeforeCollectCallback(this, callbackState, beforeCollectCallback));
    }


    /// <summary>
    ///     Creates a script context for running scripts.
    /// </summary>
    /// <remarks>
    ///     Each script context has its own global object that is isolated from all other script
    ///     contexts.
    /// </remarks>
    /// <returns>
    ///     The created script context.
    /// </returns>
    public JavaScriptContext CreateContext() {
      JavaScriptContext reference;
      Native.ThrowIfError(Native.JsCreateContext(this, out reference));
      return reference;
    }

    /// <summary>
    ///     #### Returns a value that indicates whether script execution is disabled in the runtime.
    ///     #### Enables script execution in a runtime.
    ///     #### Suspends script execution and terminates any running scripts in a runtime.
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
    /// <remarks>
    ///     Enabling script execution in a runtime that already has script execution enabled is a
    ///     no-op.
    /// </remarks>
    /// <returns>
    ///     If execution is disabled, <c>true</c>, <c>false</c> otherwise.
    /// </returns>
    public bool Disabled {
      get {
        bool isDisabled;
        Native.ThrowIfError(Native.JsIsRuntimeExecutionDisabled(this, out isDisabled));
        return isDisabled;
      }

      set {
        Native.ThrowIfError(
          value ? Native.JsDisableRuntimeExecution(this) :
          Native.JsEnableRuntimeExecution(this)
        );
      }
    }


    // Debug Functions

    /// <summary>
    ///     Starts debugging in the given runtime.
    /// </summary>
    /// <param name="debugEventCallback">Registers a callback to be called on every JsDiagDebugEvent.</param>
    /// <param name="callbackState">User provided state that will be passed back to the callback.</param>
    /// <remarks>
    ///     The runtime should be active on the current thread and should not be in debug state.
    /// </remarks>
    public void DiagStartDebugging(JsDiagDebugEventCallback debugEventCallback, IntPtr callbackState) {
      Native.ThrowIfError(Native.JsDiagStartDebugging(this, debugEventCallback, callbackState));
    }

    /// <summary>
    ///     Stops debugging in the given runtime.
    /// </summary>
    /// <returns>
    ///     User provided state that was passed in JsDiagStartDebugging.
    /// </returns>
    /// <remarks>
    ///     The runtime should be active on the current thread and in debug state.
    /// </remarks>
    public IntPtr DiagStopDebugging() {
      IntPtr callbackState;
      Native.ThrowIfError(Native.JsDiagStopDebugging(this, out callbackState));
      return callbackState;
    }

    /// <summary>
    ///     Request the runtime to break on next JavaScript statement.
    /// </summary>
    /// <remarks>
    ///     The runtime should be in debug state. This API can be called from another runtime.
    /// </remarks>
    public void DiagRequestAsyncBreak(JsDiagDebugEventCallback debugEventCallback, IntPtr callbackState) {
      Native.ThrowIfError(Native.JsDiagRequestAsyncBreak(this));
    }


    /// <summary>
    ///     Sets break on exception handling.
    /// </summary>
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
    public void JsDiagSetBreakOnException(JavaScriptDiagBreakOnExceptionAttributes exceptionAttributes) {
      Native.ThrowIfError(Native.JsDiagSetBreakOnException(this, exceptionAttributes));
    }

    /// <summary>
    ///     Gets break on exception setting.
    /// </summary>
    /// <returns>
    ///     Mask of JsDiagBreakOnExceptionAttributes.
    /// </returns>
    /// <remarks>
    ///     The runtime should be in debug state. This API can be called from another runtime.
    /// </remarks>
    public JavaScriptDiagBreakOnExceptionAttributes DiagGetBreakOnException() {
      JavaScriptDiagBreakOnExceptionAttributes exceptionAttributes;
      Native.ThrowIfError(Native.JsDiagGetBreakOnException(this, out exceptionAttributes));
      return exceptionAttributes;
    }


    // Time Travel Debugging

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
    /// <remarks>
    ///     <para>See <c>JsCreateRuntime</c> for additional information.</para>
    /// </remarks>
    /// <returns>
    ///     The runtime created.
    /// </returns>
    public static JavaScriptRuntime TTDCreateRecordRuntime(
      JavaScriptRuntimeAttributes attributes,
      bool enableDebugging,
      UIntPtr snapInterval,
      UIntPtr snapHistoryLength,
      TTDOpenResourceStreamCallback openResourceStream,
      JavaScriptTTDWriteBytesToStreamCallback writeBytesToStream,
      JavaScriptTTDFlushAndCloseStreamCallback flushAndCloseStream,
      JavaScriptThreadServiceCallback threadService
    ) {
      JavaScriptRuntime runtime;
      Native.ThrowIfError(
        Native.JsTTDCreateRecordRuntime(
          attributes,
          enableDebugging,
          snapInterval,
          snapHistoryLength,
          openResourceStream,
          writeBytesToStream,
          flushAndCloseStream,
          threadService,
          out runtime
        )
      );
      return runtime;
    }

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
    /// <remarks>
    ///     <para>See <c>JsCreateRuntime</c> for additional information.</para>
    /// </remarks>
    /// <returns>
    ///     The runtime created.
    /// </returns>
    public static JavaScriptRuntime TTDCreateReplayRuntime(
      JavaScriptRuntimeAttributes attributes,
      string infoUri,
      UIntPtr infoUriCount,
      bool enableDebugging,
      TTDOpenResourceStreamCallback openResourceStream,
      JavaScriptTTDReadBytesFromStreamCallback readBytesFromStream,
      JavaScriptTTDFlushAndCloseStreamCallback flushAndCloseStream,
      JavaScriptThreadServiceCallback threadService
    ) {
      JavaScriptRuntime runtime;
      Native.ThrowIfError(
        Native.JsTTDCreateReplayRuntime(
          attributes,
          infoUri,
          infoUriCount,
          enableDebugging,
          openResourceStream,
          readBytesFromStream,
          flushAndCloseStream,
          threadService,
          out runtime
        )
      );
      return runtime;
    }

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Creates a script context that takes the TTD mode from the log or explicitly is not in TTD mode (regular takes mode from currently active script).
    /// </summary>
    /// <param name="useRuntimeTTDMode">Set to true to use runtime TTD mode false to explicitly be non-TTD context.</param>
    /// <returns>
    ///     The created script context.
    /// </returns>
    public JavaScriptContext TTDCreateContext(bool useRuntimeTTDMode) {
      JavaScriptContext newContext;
      Native.ThrowIfError(Native.JsTTDCreateContext(this, useRuntimeTTDMode, out newContext));
      return newContext;
    }


    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Before calling JsTTDMoveToTopLevelEvent (which inflates a snapshot and replays) check to see if we want to reset the script context.
    ///     We reset the script context if the move will require inflating from a different snapshot that the last one.
    /// </summary>
    /// <param name="moveMode">Flags controlling the way the move it performed and how other parameters are interpreted.</param>
    /// <param name="kthEvent">When <c>moveMode == JsTTDMoveKthEvent</c> indicates which event, otherwise this parameter is ignored.</param>
    /// <param name="targetEventTime">The event time we want to move to or -1 if not relevant.</param>
    /// <returns>( Out parameter with the event time of the snapshot that we should inflate from, Optional Out parameter with the snapshot time following the event )</returns>
    public Tuple<long, long> TTDGetSnapTimeTopLevelEventMove(
      JavaScriptTTDMoveMode moveMode,
      uint kthEvent,
      ref long targetEventTime
    ) {
      long targetStartSnapTime;
      long targetEndSnapTime;
      Native.ThrowIfError(
        Native.JsTTDGetSnapTimeTopLevelEventMove(
          this,
          moveMode,
          kthEvent,
          ref targetEventTime,
          out targetStartSnapTime,
          out targetEndSnapTime
        )
      );
      return Tuple.Create(targetStartSnapTime, targetEndSnapTime);
    }

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Get the snapshot interval that bounds the target event time.
    /// </summary>
    /// <param name="targetEventTime">The event time we want to get the interval for.</param>
    /// <returns>( The snapshot time that comes before the desired event, The snapshot time that comes after the desired event (-1 if the leg ends before a snapshot appears) )</returns>
    public Tuple<long, long> TTDGetSnapShotBoundInterval(long targetEventTime) {
      long startSnapTime;
      long endSnapTime;
      Native.ThrowIfError(
        Native.JsTTDGetSnapShotBoundInterval(
          this,
          targetEventTime,
          out startSnapTime,
          out endSnapTime
        )
      );
      return Tuple.Create(startSnapTime, endSnapTime);
    }

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Get the snapshot interval that precedes the one given by currentSnapStartTime (or -1 if there is no such interval).
    /// </summary>
    /// <param name="currentSnapStartTime">The current snapshot interval start time.</param>
    /// <returns>The resulting previous snapshot interval start time or -1 if no such time.</returns>
    public long TTDGetPreviousSnapshotInterval(long currentSnapStartTime) {
      long previousSnapTime;
      Native.ThrowIfError(Native.JsTTDGetPreviousSnapshotInterval(this, currentSnapStartTime, out previousSnapTime));
      return previousSnapTime;
    }

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     During debug operations some additional information is populated during replay. This runs the code between the given
    ///     snapshots to populate this information which may be needed by the debugger to determine time-travel jump targets.
    /// </summary>
    /// <param name="startSnapTime">The snapshot time that we will start executing from.</param>
    /// <param name="endSnapTime">The snapshot time that we will stop at (or -1 if we want to run to the end).</param>
    /// <param name="moveMode">Additional flags for controling how the move is done.</param>
    /// <returns>The updated target event time set according to the moveMode (-1 if not found).</returns>
    public long TTDPreExecuteSnapShotInterval(
      long startSnapTime,
      long endSnapTime,
      JavaScriptTTDMoveMode moveMode
    ) {
      long newTargetEventTime;
      Native.ThrowIfError(
        Native.JsTTDPreExecuteSnapShotInterval(
          this,
          startSnapTime,
          endSnapTime,
          moveMode,
          out newTargetEventTime
        )
      );
      return newTargetEventTime;
    }

    /// <summary>
    ///     TTD API -- may change in future versions:
    ///     Move to the given top-level call event time (assuming JsTTDPrepContextsForTopLevelEventMove) was called previously to reset any script contexts.
    ///     This also computes the ready-to-run snapshot if needed.
    /// </summary>
    /// <param name="moveMode">Additional flags for controling how the move is done.</param>
    /// <param name="snapshotTime">The event time that we will start executing from to move to the given target time.</param>
    /// <param name="eventTime">The event that we want to move to.</param>
    /// <returns>The code <c>JsNoError</c> if the operation succeeded, a failure code otherwise.</returns>
    public void TTDMoveToTopLevelEvent(
      JavaScriptTTDMoveMode moveMode,
      long snapshotTime,
      long eventTime
    ) {
      Native.ThrowIfError(Native.JsTTDMoveToTopLevelEvent(this, moveMode, snapshotTime, eventTime));
    }


  }
}
