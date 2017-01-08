using System;
using System.Diagnostics;
using static Interop.Libsodium;

namespace NSec.Cryptography
{
    [DebuggerDisplay("Size = {Size}")]
    public sealed class SharedSecret : IDisposable
    {
        private readonly SecureMemoryHandle _handle;

        internal SharedSecret(
            SecureMemoryHandle handle)
        {
            Debug.Assert(handle != null);

            handle.MakeReadOnly();

            _handle = handle;
        }

        public int Size => _handle.Length;

        internal SecureMemoryHandle Handle => _handle;

        public static SharedSecret Import(
            ReadOnlySpan<byte> sharedSecret)
        {
            if (sharedSecret.Length > 128)
                throw new ArgumentException(Error.ArgumentExceptionMessage, nameof(sharedSecret));

            Sodium.Initialize();

            SecureMemoryHandle sharedSecretHandle = null;
            bool success = false;

            try
            {
                SecureMemoryHandle.Alloc(sharedSecret.Length, out sharedSecretHandle);
                sharedSecretHandle.Import(sharedSecret);
                success = true;
            }
            finally
            {
                if (!success && sharedSecretHandle != null)
                {
                    sharedSecretHandle.Dispose();
                }
            }

            return new SharedSecret(sharedSecretHandle);
        }

        public void Dispose()
        {
            _handle.Dispose();
        }
    }
}
