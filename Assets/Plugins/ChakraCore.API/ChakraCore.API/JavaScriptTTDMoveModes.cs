namespace ChakraCore.API {
  /// <summary>
  ///     TimeTravel move options as bit flag enum.
  /// </summary>
  public enum JavaScriptTTDMoveMode {
    /// <summary>
    ///     Indicates no special actions needed for move.
    /// </summary>
    JsTTDMoveNone = 0x0,

    /// <summary>
    ///     Indicates that we want to move to the first event.
    /// </summary>
    JsTTDMoveFirstEvent = 0x1,

    /// <summary>
    ///     Indicates that we want to move to the last event.
    /// </summary>
    JsTTDMoveLastEvent = 0x2,

    /// <summary>
    ///     Indicates that we want to move to the kth event -- top 32 bits are event count.
    /// </summary>
    JsTTDMoveKthEvent = 0x4,

    /// <summary>
    ///     Indicates if we are doing the scan for a continue operation
    /// </summary>
    JsTTDMoveScanIntervalForContinue = 0x10,

    /// <summary>
    ///     Indicates if we are doing the scan for a continue operation and are in the time-segment where the active breakpoint was
    /// </summary>
    JsTTDMoveScanIntervalForContinueInActiveBreakpointSegment = 0x20,

    /// <summary>
    ///     Indicates if we want to set break on entry or just run and let something else trigger breakpoints.
    /// </summary>
    JsTTDMoveBreakOnEntry = 0x100,
  }
}
