using System;

namespace ChakraCore.API {
  /// <summary>
  ///     A weak reference to a JavaScript value.
  /// </summary>
  /// <remarks>
  ///     A value with only weak references is available for garbage-collection. A strong reference
  ///     to the value (<c>JsValueRef</c>) may be obtained from a weak reference if the value happens
  ///     to still be available.
  /// </remarks>
  public struct JavaScriptWeakRef {
    /// <summary>
    /// The reference.
    /// </summary>
    private readonly IntPtr reference;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JavaScriptWeakRef"/> struct.
    /// </summary>
    /// <param name="reference">The reference.</param>
    private JavaScriptWeakRef(IntPtr reference) {
      this.reference = reference;
    }

    /// <summary>
    ///     Gets an invalid ID.
    /// </summary>
    public static JavaScriptWeakRef Invalid {
      get { return new JavaScriptWeakRef(IntPtr.Zero); }
    }

    /// <summary>
    ///     Gets a value indicating whether the reference is valid.
    /// </summary>
    public bool IsValid {
      get { return reference != IntPtr.Zero; }
    }
  }
}
