using System;
using System.Runtime.InteropServices;

namespace ZMQ.Net
{
    /// <summary>
    /// Imports of ØMQ functions.
    /// </summary>
    internal class C
    {
        #region Version

        /// <summary>
        /// 
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern void zmq_version( out int major, out int minor, out int patch );

        #endregion

        #region Error handling

        public const int EAGAIN = 11;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_errno();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errnum"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl )]
        public static extern string zmq_strerror( int errnum );

        #endregion

        #region Message manipulation

        /// <summary>
        /// 'Very small message' size in bytes. A message at or below this size will be embedded in the zmq_msg_t structure.
        /// </summary>
        public const int ZMQ_MAX_VSM_SIZE = 30;

        /// <summary>
        /// Size of zmq_msg_t structure in bytes. It is equal to (size of pointer) + 2 unsigned chars + (VSM size).
        /// </summary>
        public static readonly int ZMQ_MSG_T_SIZE = IntPtr.Size + 2 + ZMQ_MAX_VSM_SIZE;

        /// <summary>
        /// Callback for deallocation of buffers used in zero-copy.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hint"></param>
        public delegate void zmq_free_fn( IntPtr data, IntPtr hint );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_init( IntPtr msg );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_init_size( IntPtr msg, int size );

        /// <summary>
        /// Initialize a ØMQ message from an existing buffer.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="ffn"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_init_data( IntPtr msg, IntPtr data, int size, zmq_free_fn ffn, IntPtr hint );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_close( IntPtr msg );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dstMsg"></param>
        /// <param name="srcMsg"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_move( IntPtr dstMsg, IntPtr srcMsg );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dstMsg"></param>
        /// <param name="srcMsg"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_copy( IntPtr dstMsg, IntPtr srcMsg );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr zmq_msg_data( IntPtr msg );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_msg_size( IntPtr msg );

        #endregion

        #region Context

        /// <summary>
        /// 
        /// </summary>
        /// <param name="io_threads"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr zmq_init( int io_threads );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_term( IntPtr context );

        #endregion

        #region Sockets

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr zmq_socket( IntPtr context, int type );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_close( IntPtr socket );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_setsockopt( IntPtr socket, int option, IntPtr optval, size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_setsockopt( IntPtr socket, int option, [In]ref int optval, size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_setsockopt( IntPtr socket, int option, [In]ref long optval, size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_setsockopt( IntPtr socket, int option, [In]ref ulong optval, size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_getsockopt( IntPtr socket, int option, IntPtr optval, ref size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_getsockopt( IntPtr socket, int option, out int optval, [In]ref size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_getsockopt( IntPtr socket, int option, out long optval, [In]ref size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="option"></param>
        /// <param name="optval"></param>
        /// <param name="optvallen"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_getsockopt( IntPtr socket, int option, out ulong optval, [In]ref size_t optvallen );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_bind( IntPtr socket, string addr );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_connect( IntPtr socket, string addr );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="msg"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_recv( IntPtr socket, IntPtr msg, int flags );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="msg"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_send( IntPtr socket, IntPtr msg, int flags );

        #endregion

        #region Polling

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="nItems"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_poll( IntPtr items, int nItems, long timeout );

        #endregion

        #region Forwarding devices

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inSocket"></param>
        /// <param name="outSocket"></param>
        /// <returns></returns>
        [DllImport( "libzmq", CallingConvention = CallingConvention.Cdecl )]
        public static extern int zmq_device( int device, IntPtr inSocket, IntPtr outSocket );

        #endregion

        #region size_t

        public struct size_t
        {
            private UIntPtr ptr;

            public size_t( int value )
            {
#if x86
                ptr = new UIntPtr( unchecked( (uint)value ) );
#elif x64
                ptr = new UIntPtr( unchecked( (ulong)value ) );
#endif
            }

            public size_t( long value )
            {
#if x86
                ptr = new UIntPtr( unchecked( (uint)value ) );
#elif x64
                ptr = new UIntPtr( unchecked( (ulong)value ) );
#endif
            }

            public uint ToUInt32()
            {
                return ptr.ToUInt32();
            }

            public ulong ToUInt64()
            {
                return ptr.ToUInt64();
            }

            public int ToInt32()
            {
                return unchecked( (int)ptr.ToUInt32() );
            }

            public long ToInt64()
            {
                return unchecked( (long)ptr.ToUInt64() );
            }
        }

        #endregion
    }
}