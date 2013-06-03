using System;
using System.Diagnostics.Contracts;

namespace ZMQ.Net
{
    #region Enumerations

    /// <summary>
    /// ØMQ socket options.
    /// </summary>
    public enum SocketOption
    {
        /// <summary>
        /// High water mark. (uint64)
        /// </summary>
        HighWaterMark = 1,          //ZMQ_HWM

        /// <summary>
        /// Disk offload size. (int64)
        /// </summary>
        SwapSize = 3,               //ZMQ_SWAP

        /// <summary>
        /// I/O thread affinity. (uint64)
        /// </summary>
        ThreadAffinity = 4,         //ZMQ_AFFINITY

        /// <summary>
        /// Socket identity. (string/byte[])
        /// </summary>
        SocketIdentity = 5,         //ZMQ_IDENTITY

        /// <summary>
        /// Establish message filter. (string/byte[])
        /// </summary>
        Subscribe = 6,              //ZMQ_SUBSCRIBE

        /// <summary>
        /// Remove message filter. (string/byte[])
        /// </summary>
        Unsubscribe = 7,            //ZMQ_UNSUBSCRIBE

        /// <summary>
        /// Kernel transmit buffer size. (uint64)
        /// </summary>
        SendBuffer = 11,            //ZMQ_SNDBUF

        /// <summary>
        /// Kernel receive buffer size. (uint64)
        /// </summary>
        ReceiveBuffer = 12,         //ZMQ_RCVBUF

        /// <summary>
        /// Value for this option indicates whether the multi-part message currently 
        /// being read from the socket has more message parts to follow. (int64)
        /// </summary>
        MoreToFollow = 13,          //ZMQ_RCVMORE

        /// <summary>
        /// Socket type. (int32)
        /// </summary>
        SocketType = 16,            //ZMQ_TYPE

        /// <summary>
        /// Linger period (in ms) for socket shutdown. (int32)
        /// </summary>
        Linger = 17,                //ZMQ_LINGER

        /// <summary>
        /// Reconnection interval in ms. (int32)
        /// </summary>
        ReconnectInterval = 18,     //ZMQ_RECONNECT_IVL

        /// <summary>
        /// Maximum length of the queue of outstanding connections. (int32)
        /// </summary>
        ConnectionBacklog = 19,               //ZMQ_BACKLOG

        /// <summary>
        /// Maximum reconnection interval in ms. (int32)
        /// </summary>
        ReconnectIntervalMax = 21   //ZMQ_RECONNECT_IVL_MAX

        //ZMQ_FD = 14
        //ZMQ_EVENTS = 15
    }

    /// <summary>
    /// ØMQ multicast socket options.
    /// </summary>
    public enum MulticastOption
    {
        /// <summary>
        /// Multicast data rate. (int64)
        /// </summary>
        DataRate = 8,           //ZMQ_RATE

        /// <summary>
        /// Multicast recovery interval. (int64)
        /// </summary>
        RecoveryInterval = 9,   //ZMQ_RECOVERY_IVL

        /// <summary>
        /// Controls multicast loopback. (int64)
        /// </summary>
        Loopback = 10,          //ZMQ_MCAST_LOOP

        /// <summary>
        /// Multicast recovery interval in milliseconds. (int64)
        /// </summary>
        RecoveryIntervalMS = 20     //ZMQ_RECOVERY_IVL_MSEC = 20
    }

    /// <summary>
    /// ØMQ socket types.
    /// </summary>
    public enum SocketType
    {
        /// <summary>
        /// Exclusive pair. Experimental.
        /// </summary>
        ExclusivePair = 0,  //ZMQ_PAIR

        /// <summary>
        /// Pub-sub pattern publisher.
        /// </summary>
        Publisher = 1,      //ZMQ_PUB

        /// <summary>
        /// Pub-sub pattern subscriber.
        /// </summary>
        Subscriber = 2,     //ZMQ_SUB

        /// <summary>
        /// Request-reply pattern client.
        /// </summary>
        Request = 3,        //ZMQ_REQ

        /// <summary>
        /// Request-reply pattern service.
        /// </summary>
        Reply = 4,          //ZMQ_REP

        /// <summary>
        /// Extended request-reply pattern 'dealer'.
        /// </summary>
        Dealer = 5,         //ZMQ_DEALER

        /// <summary>
        /// Extended request-reply pattern 'router'.
        /// </summary>
        Router = 6,         //ZMQ_ROUTER

        /// <summary>
        /// Pipeline pattern receive-only 'pull' socket.
        /// </summary>
        Pull = 7,           //ZMQ_PULL

        /// <summary>
        /// Pipeline pattern send-only 'push' socket.
        /// </summary>
        Push = 8            //ZMQ_PUSH

        //ZMQ_XPUB = 9
        //ZMQ_XSUB = 10
    }

    /// <summary>
    /// ØMQ receive flags.
    /// </summary>
    [Flags]
    public enum ReceiveFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Non-blocking operation.
        /// </summary>
        NonBlocking = 1,    //ZMQ_NOBLOCK
    }

    /// <summary>
    /// ØMQ send flags.
    /// </summary>
    [Flags]
    public enum SendFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Non-blocking operation.
        /// </summary>
        NonBlocking = 1,    //ZMQ_NOBLOCK

        /// <summary>
        /// Send part of a multipart message with more parts left to follow.
        /// </summary>
        MoreToFollow = 2        //ZMQ_SNDMORE
    }

    //TODO I/O multiplexing
    //ZMQ_POLLIN = 1
    //ZMQ_POLLOUT = 2
    //ZMQ_POLLERR = 4

    /// <summary>
    /// ØMQ built-in devices. Experimental.
    /// </summary>
    public enum Device
    {
        /// <summary>
        /// 
        /// </summary>
        Streamer = 1,       //ZMQ_STREAMER

        /// <summary>
        /// 
        /// </summary>
        Forwarder = 2,      //ZMQ_FORWARDER

        /// <summary>
        /// 
        /// </summary>
        Queue = 3           //ZMQ_QUEUE
    }

    #endregion

    /// <summary>
    /// ØMQ exception base class.
    /// </summary>
    public class ZeroMQException : System.Exception
    {
        /// <summary>
        /// Gets the error number.
        /// </summary>
        public int Errno
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new exception with the creating thread's last ØMQ error message and number.
        /// </summary>
        private ZeroMQException()
            : base( C.zmq_strerror( C.zmq_errno() ) )
        {
            Errno = C.zmq_errno();
        }

        /// <summary>
        /// Returns a new exception with the creating thread's last ØMQ error message and number.
        /// </summary>
        /// <returns></returns>
        public static Exception CurrentError()
        {
            return new ZeroMQException();
        }
    }

    /// <summary>
    /// Contains the version information of the ØMQ assembly this library uses.
    /// </summary>
    public static class Version
    {
        /// <summary>
        /// Major version number.
        /// </summary>
        public static readonly int Major;

        /// <summary>
        /// Minor version number.
        /// </summary>
        public static readonly int Minor;

        /// <summary>
        /// Patch number.
        /// </summary>
        public static readonly int Patch;

        static Version()
        {
            C.zmq_version( out Major, out Minor, out Patch );
        }
    }

    /// <summary>
    /// Wrapper for ØMQ context.
    /// </summary>
    public class Context : IDisposable
    {
        /// <summary>
        /// Default linger period for socket shutdown in ms.
        /// </summary>
        public const int DEFAULT_LINGER = 10000;

        private IntPtr m_ptr = IntPtr.Zero;

        /// <summary>
        /// Creates a ØMQ context without specifying thread pool size, for use with the inproc transport.
        /// </summary>
        public Context()
        {
            Disposed = false;

            m_ptr = C.zmq_init( 0 );

            if( m_ptr == IntPtr.Zero )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Creates a ØMQ context with a specified thread pool size.
        /// </summary>
        /// <param name="io_threads">Size of the ØMQ thread pool to handle I/O operations. Must be at least zero.</param>
        public Context( int io_threads )
        {
            Contract.Requires( io_threads >= 0, "Thread pool size must be at least 0." );

            Disposed = false;

            m_ptr = C.zmq_init( io_threads );

            if( m_ptr == IntPtr.Zero )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        ~Context()
        {
            Dispose( false );
        }

        /// <summary>
        /// Gets whether the object has been disposed.
        /// </summary>
        public bool Disposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a socket within the current context.
        /// </summary>
        /// <param name="type">The socket type.</param>
        /// <param name="lingerTime">Linger period for socket shutdown, in ms, or <see cref="System.Threading.Timeout.Infinite"/> for no linger.</param>
        /// <returns>An unbound socket.</returns>
        public Socket CreateSocket( SocketType type, int lingerTime = DEFAULT_LINGER )
        {
            Contract.Requires( !Disposed );
            Contract.Ensures( Contract.Result<Socket>() != null );

            IntPtr socket = C.zmq_socket( m_ptr, (int)type );

            if( socket == IntPtr.Zero )
            {
                throw ZeroMQException.CurrentError();
            }
            
            Socket sock = new Socket( socket );

            if( lingerTime >= 0 )
            {
                sock.SetOption( SocketOption.Linger, lingerTime );
            }

            return sock;
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            Contract.Ensures( Disposed );

            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        /// <param name="disposing">Whether to dispose of managed resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            Contract.Ensures( Disposed );

            try
            {
                if( m_ptr != IntPtr.Zero )
                {
                    int rc = C.zmq_term( m_ptr );
                    m_ptr = IntPtr.Zero;

                    if( rc != 0 )
                    {
                        throw ZeroMQException.CurrentError();
                    }
                }
            }
            finally
            {
                Disposed = true;
            }
        }
    }
}