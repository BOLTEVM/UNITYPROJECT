using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A reference to an object owned by the SharedArrayBuffer.
  /// </summary>
  /// <remarks>
  ///     This represents SharedContents which is heap allocated object, it can be passed through
  ///     different runtimes to share the underlying buffer.
  /// </remarks>
  public struct JavaScriptSharedArrayBufferContentHandle {
    /// <summary>
    /// The reference.
    /// </summary>
    private readonly IntPtr reference;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JavaScriptSharedArrayBufferContentHandle"/> struct.
    /// </summary>
    /// <param name="reference">The reference.</param>
    private JavaScriptSharedArrayBufferContentHandle(IntPtr reference) {
      this.reference = reference;
    }

    /// <summary>
    ///     Gets an invalid ID.
    /// </summary>
    public static JavaScriptSharedArrayBufferContentHandle Invalid {
      get { return new JavaScriptSharedArrayBufferContentHandle(IntPtr.Zero); }
    }

    /// <summary>
    ///     Gets a value indicating whether the content handle is valid.
    /// </summary>
    public bool IsValid {
      get { return reference != IntPtr.Zero; }
    }
  }
}
