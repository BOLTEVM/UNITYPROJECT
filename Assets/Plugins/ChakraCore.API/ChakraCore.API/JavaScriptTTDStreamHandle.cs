using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A handle for URI's that TTD information is written to/read from.
  /// </summary>
  public struct JavaScriptTTDStreamHandle {
    /// <summary>
    /// The reference.
    /// </summary>
    private readonly IntPtr reference;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JavaScriptTTDStreamHandle"/> struct.
    /// </summary>
    /// <param name="reference">The reference.</param>
    private JavaScriptTTDStreamHandle(IntPtr reference) {
      this.reference = reference;
    }

    /// <summary>
    ///     Gets an invalid ID.
    /// </summary>
    public static JavaScriptTTDStreamHandle Invalid {
      get { return new JavaScriptTTDStreamHandle(IntPtr.Zero); }
    }

    /// <summary>
    ///     Gets a value indicating whether the stream handle is valid.
    /// </summary>
    public bool IsValid {
      get { return reference != IntPtr.Zero; }
    }
  }


  /// <summary>
  ///     TTD API -- may change in future versions:
  ///     Construct a JsTTDStreamHandle that will be used to read/write the event log portion of the TTD data based on the uri
  ///     provided by JsTTDInitializeUriCallback.
  /// </summary>
  /// <remarks>
  ///     <para>Exactly one of read or write will be set to true.</para>
  /// </remarks>
  /// <param name="uriLength">The length of the uri array that the host passed in for storing log info.</param>
  /// <param name="uri">The URI that the host passed in for storing log info.</param>
  /// <param name="asciiNameLength">The length of the ascii name array that the host passed in for storing log info.</param>
  /// <param name="asciiResourceName">An optional ascii string giving a unique name to the resource that the JsTTDStreamHandle will be created for.</param>
  /// <param name="read">If the handle should be opened for reading.</param>
  /// <param name="write">If the handle should be opened for writing.</param>
  /// <returns>A JsTTDStreamHandle opened in read/write mode as specified.</returns>
  // typedef JsTTDStreamHandle (CHAKRA_CALLBACK *TTDOpenResourceStreamCallback)(_In_ size_t uriLength, _In_reads_(uriLength) const char* uri, _In_ size_t asciiNameLength, _In_reads_(asciiNameLength) const char* asciiResourceName, _In_ bool read, _In_ bool write);
  public delegate JavaScriptTTDStreamHandle TTDOpenResourceStreamCallback(
    UIntPtr uriLength,
    string uri,
    UIntPtr asciiNameLength,
    string asciiResourceName,
    bool read,
    bool write
  );

  /// <summary>
  ///     TTD API -- may change in future versions:
  ///     A callback for reading data from a handle.
  /// </summary>
  /// <param name="handle">The JsTTDStreamHandle to read the data from.</param>
  /// <param name="buff">The buffer to place the data into.</param>
  /// <param name="size">The max number of bytes that should be read.</param>
  /// <param name="readCount">The actual number of bytes read and placed in the buffer.</param>
  /// <returns>true if the read was successful false otherwise.</returns>
  // typedef bool (CHAKRA_CALLBACK *JsTTDReadBytesFromStreamCallback)(_In_ JsTTDStreamHandle handle, _Out_writes_(size) byte* buff, _In_ size_t size, _Out_ size_t* readCount);
  public delegate bool JavaScriptTTDReadBytesFromStreamCallback(
    JavaScriptTTDStreamHandle handle,
    out byte[] buff, // ????
    UIntPtr size,
    out UIntPtr readCount
  );

  /// <summary>
  ///     TTD API -- may change in future versions:
  ///     A callback for writing data to a handle.
  /// </summary>
  /// <param name="handle">The JsTTDStreamHandle to write the data to.</param>
  /// <param name="buff">The buffer to copy the data from.</param>
  /// <param name="size">The max number of bytes that should be written.</param>
  /// <param name="readCount">The actual number of bytes written to the HANDLE.</param>
  /// <returns>true if the write was successful false otherwise.</returns>
  // typedef bool (CHAKRA_CALLBACK *JsTTDWriteBytesToStreamCallback)(_In_ JsTTDStreamHandle handle, _In_reads_(size) const byte* buff, _In_ size_t size, _Out_ size_t* writtenCount);
  public delegate bool JavaScriptTTDWriteBytesToStreamCallback(
    JavaScriptTTDStreamHandle handle,
    byte[] buff, // ????
    UIntPtr size,
    out UIntPtr writtenCount
  );

  /// <summary>
  ///     TTD API -- may change in future versions:
  ///     Flush and close the stream represented by the HANDLE as needed.
  /// </summary>
  /// <remarks>
  ///     <para>Exactly one of read or write will be set to true.</para>
  /// </remarks>
  /// <param name="handle">The JsTTDStreamHandle to close.</param>
  /// <param name="read">If the handle was opened for reading.</param>
  /// <param name="write">If the handle was opened for writing.</param>
  // typedef void (CHAKRA_CALLBACK *JsTTDFlushAndCloseStreamCallback)(_In_ JsTTDStreamHandle handle, _In_ bool read, _In_ bool write);
  public delegate void JavaScriptTTDFlushAndCloseStreamCallback(
    JavaScriptTTDStreamHandle handle,
    bool read,
    bool write
  );
}
