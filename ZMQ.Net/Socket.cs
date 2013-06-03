using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZMQ.Net
{
    /// <summary>
    /// Wrapper for ØMQ socket. Not explicitly thread-safe, but that is not neccesary anyway: a socket may only be used on the creating thread.
    /// </summary>
    public class Socket : IDisposable
    {
        private IntPtr m_sock = IntPtr.Zero;
        private IntPtr m_msg = IntPtr.Zero;

        /// <summary>
        /// Creates a socket object for the specified 0MQ handle.
        /// </summary>
        /// <param name="socketHandle"></param>
        internal Socket( IntPtr socketHandle )
        {
            Disposed = false;

            m_sock = socketHandle;
            m_msg = Marshal.AllocHGlobal( C.ZMQ_MSG_T_SIZE );
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        ~Socket()
        {
            Dispose( false );
        }

        #region Public properties

        /// <summary>
        /// Gets whether the object has been disposed.
        /// </summary>
        public bool Disposed
        {
            get;
            private set;
        }

        #endregion

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
                if( m_msg != IntPtr.Zero )
                {
                    Marshal.FreeHGlobal( m_msg );
                    m_msg = IntPtr.Zero;
                }

                if( m_sock != IntPtr.Zero )
                {
                    int rc = C.zmq_close( m_sock );
                    m_sock = IntPtr.Zero;

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

        #region Socket options

        /// <summary>
        /// Sets socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        public void SetOption( SocketOption option, string value )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == SocketOption.SocketIdentity
                            || option == SocketOption.Subscribe
                            || option == SocketOption.Unsubscribe );
            Contract.Requires( value != null );
            Contract.Requires( option != SocketOption.SocketIdentity || value.Length <= 255 );

            SetOption( option, Encoding.ASCII.GetBytes( value ) );
        }

        /// <summary>
        /// Sets socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        /// <param name="encoding">Encoding to use for the string value.</param>
        public void SetOption( SocketOption option, string value, Encoding encoding )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == SocketOption.SocketIdentity
                            || option == SocketOption.Subscribe
                            || option == SocketOption.Unsubscribe );
            Contract.Requires( value != null );
            Contract.Requires( option != SocketOption.SocketIdentity || value.Length <= 255 );

            SetOption( option, encoding.GetBytes( value ) );
        }

        /// <summary>
        /// Sets socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        public void SetOption( SocketOption option, byte[] value )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == SocketOption.SocketIdentity
                            || option == SocketOption.Subscribe
                            || option == SocketOption.Unsubscribe );
            Contract.Requires( value != null );
            Contract.Requires( option != SocketOption.SocketIdentity || ( value.Length <= 255 && value.Length > 0 ) );

            //Automatic marshalling of the byte array doesn't quite seem to work.

            IntPtr valuePtr = Marshal.AllocHGlobal( value.Length );

            try
            {
                Marshal.Copy( value, 0, valuePtr, value.Length );

                if( C.zmq_setsockopt( m_sock, (int)option, valuePtr, new C.size_t( value.Length ) ) != 0 )
                {
                    throw ZeroMQException.CurrentError();
                }
            }
            finally
            {
                Marshal.FreeHGlobal( valuePtr );
            }
        }

        /// <summary>
        /// Sets socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        public void SetOption( SocketOption option, long value )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == SocketOption.SwapSize );

            if( C.zmq_setsockopt( m_sock, (int)option, ref value, new C.size_t( Marshal.SizeOf( value ) ) ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Sets multicast socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        public void SetOption( MulticastOption option, long value )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == MulticastOption.DataRate
                            || option == MulticastOption.RecoveryInterval
                            || option == MulticastOption.RecoveryIntervalMS
                            || option == MulticastOption.Loopback );

            if( C.zmq_setsockopt( m_sock, (int)option, ref value, new C.size_t( Marshal.SizeOf( value ) ) ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Sets socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        public void SetOption( SocketOption option, ulong value )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == SocketOption.HighWaterMark
                            || option == SocketOption.ThreadAffinity
                            || option == SocketOption.SendBuffer
                            || option == SocketOption.ReceiveBuffer );

            if( C.zmq_setsockopt( m_sock, (int)option, ref value, new C.size_t( Marshal.SizeOf( value ) ) ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Sets socket options.
        /// </summary>
        /// <param name="option">Option to set.</param>
        /// <param name="value">Value to set the option to.</param>
        public void SetOption( SocketOption option, int value )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( option == SocketOption.Linger
                            || option == SocketOption.ReconnectInterval
                            || option == SocketOption.ConnectionBacklog
                            || option == SocketOption.ReconnectIntervalMax );

            if( C.zmq_setsockopt( m_sock, (int)option, ref value, new C.size_t( Marshal.SizeOf( value ) ) ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Gets socket option value.
        /// </summary>
        /// <param name="option">Option to get.</param>
        /// <returns></returns>
        public dynamic GetOption( SocketOption option )
        {
            switch( option )
            {
                case SocketOption.SocketIdentity:
                    return GetOptionString( (int)option );

                case SocketOption.HighWaterMark:
                case SocketOption.ThreadAffinity:
                case SocketOption.SendBuffer:
                case SocketOption.ReceiveBuffer:
                    return GetOptionUInt64( (int)option );

                case SocketOption.SwapSize:
                case SocketOption.MoreToFollow:
                    return GetOptionInt64( (int)option );

                case SocketOption.SocketType:
                case SocketOption.Linger:
                case SocketOption.ReconnectInterval:
                case SocketOption.ReconnectIntervalMax:
                case SocketOption.ConnectionBacklog:
                    return GetOptionInt32( (int)option );

                default:
                    throw new NotImplementedException( "Unknown option: " + option );
            }
        }

        /// <summary>
        /// Gets socket option value.
        /// </summary>
        /// <param name="option">Option to get.</param>
        /// <returns></returns>
        public dynamic GetOption( MulticastOption option )
        {
            switch( option )
            {
                case MulticastOption.DataRate:
                case MulticastOption.RecoveryInterval:
                case MulticastOption.RecoveryIntervalMS:
                case MulticastOption.Loopback:
                    return GetOptionInt64( (int)option );

                default:
                    throw new NotImplementedException( "Unknown option: " + option );
            }
        }

        private string GetOptionString( int option )
        {
            Contract.Requires( !Disposed );

            C.size_t baLen = new C.size_t( 255 );
            IntPtr baPtr = Marshal.AllocHGlobal( baLen.ToInt32() );

            //Automatic marshalling of the byte array doesn't quite seem to work.

            if( C.zmq_getsockopt( m_sock, option, baPtr, ref baLen ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }

            byte[] baValue = new byte[baLen.ToInt32()];
            Marshal.Copy( baPtr, baValue, 0, baValue.Length );

            return Encoding.ASCII.GetString( baValue );
        }

        private int GetOptionInt32( int option )
        {
            Contract.Requires( !Disposed );

            int value;
            C.size_t len = new C.size_t( Marshal.SizeOf( typeof( int ) ) );

            if( C.zmq_getsockopt( m_sock, option, out value, ref len ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }

            return value;
        }

        private long GetOptionInt64( int option )
        {
            Contract.Requires( !Disposed );

            long value;
            C.size_t len = new C.size_t( Marshal.SizeOf( typeof( long ) ) );

            if( C.zmq_getsockopt( m_sock, option, out value, ref len ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }

            return value;
        }

        private ulong GetOptionUInt64( int option )
        {
            Contract.Requires( !Disposed );

            ulong value;
            C.size_t len = new C.size_t( Marshal.SizeOf( typeof( ulong ) ) );

            if( C.zmq_getsockopt( m_sock, option, out value, ref len ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }

            return value;
        }

        #endregion

        #region Connection

        /// <summary>
        /// Binds the socket to an address to listen on.
        /// </summary>
        /// <param name="addr"></param>
        public void Bind( string addr )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( !string.IsNullOrEmpty( addr ) );

            if( C.zmq_bind( m_sock, addr ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Connects the socket to an address.
        /// </summary>
        /// <param name="addr"></param>
        public void Connect( string addr )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( !string.IsNullOrEmpty( addr ) );

            if( C.zmq_connect( m_sock, addr ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }
        }

        #endregion

        #region Receiving

        /// <summary>
        /// Receives data from the socket without flags set.
        /// </summary>
        /// <returns>The received data, or null if no data was received.</returns>
        public byte[] Receive()
        {
            Contract.Requires( !Disposed );

            return Receive( ReceiveFlags.None );
        }

        /// <summary>
        /// Receives data from the socket with flags set.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns>The received data, or null if no data was received.</returns>
        public byte[] Receive( ReceiveFlags flags )
        {
            Contract.Requires( !Disposed );

            byte[] data;

            if( TryReceive( out data, flags ) == true )
            {
                return data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Receives data from the socket as a <see cref="Message"/>.
        /// </summary>
        /// <returns></returns>
        public Message ReceiveMessage()
        {
            return Message.FromMultipart( ReceiveMultiPart() );
        }

        /// <summary>
        /// Attempts to receive data from the socket without flags set.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if data was received; otherwise, false, and <paramref name="data"/> will be an empty array.</returns>
        public bool TryReceive( out byte[] data )
        {
            Contract.Requires( !Disposed );
            Contract.Ensures( Contract.ValueAtReturn( out data ) != null );

            return TryReceive( out data, ReceiveFlags.None );
        }

        /// <summary>
        /// Attempts to receive data from the socket with flags set.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <returns>True if data was received; otherwise, false, and <paramref name="data"/> will be an empty array.</returns>
        public bool TryReceive( out byte[] data, ReceiveFlags flags )
        {
            Contract.Requires( !Disposed );
            Contract.Ensures( Contract.ValueAtReturn( out data ) != null );

            if( C.zmq_msg_init( m_msg ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }

            int rc = C.zmq_recv( m_sock, m_msg, (int)flags );

            if( rc == 0 )
            {
                data = new byte[C.zmq_msg_size( m_msg )];
                Marshal.Copy( C.zmq_msg_data( m_msg ), data, 0, data.Length );
            }
            else
            {
                data = new byte[0];
            }

            C.zmq_msg_close( m_msg );

            if( rc == 0 )
            {
                return true;
            }
            else if( C.zmq_errno() == C.EAGAIN )
            {
                return false;
            }
            else
            {
                throw ZeroMQException.CurrentError();
            }
        }

        /// <summary>
        /// Gets a multi-part message through an enumeration.
        /// </summary>
        public IEnumerable<byte[]> ReceiveMultiPart()
        {
            Contract.Ensures( Contract.Result<IEnumerable<byte[]>>() != null );

            yield return Receive();

            while( MoreToFollow )
            {
                yield return Receive();
            }
        }

        /// <summary>
        /// Gets whether more message parts of a multipart message are remaining.
        /// </summary>
        public bool MoreToFollow
        {
            get
            {
                return GetOption( SocketOption.MoreToFollow ) == 1;
            }
        }

        #endregion

        #region Sending

        /// <summary>
        /// Sends data over the socket without flags set.
        /// </summary>
        /// <param name="data">The message part(s). Sending without a value set will result in sending a zero-length message, not a null.</param>
        /// <returns>True if the data was sent successfully; otherwise, false.</returns>
        public bool Send( params byte[][] data )
        {
            Contract.Requires( !Disposed );

            if( data.Length == 0 )
            {
                return SendOne( new byte[0], SendFlags.None );
            }
            if( data.Length == 1 )
            {
                return SendOne( data[0], SendFlags.None );
            }
            else if( data.Length > 1 )
            {
                for( int i = 0; i < data.Length - 1; i++ )
                {
                    if( SendOne( data[i], SendFlags.MoreToFollow ) == false )
                    {
                        return false;
                    }
                }

                return SendOne( data.Last(), SendFlags.None );
            }

            return true;
        }

        /// <summary>
        /// Sends data over the socket with flags set.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="data">The message part(s). Sending without a value set will result in sending a zero-length message, not a null.</param>
        /// <returns>True if the data was sent successfully; otherwise, false.</returns>
        public bool Send( SendFlags flags, params byte[][] data )
        {
            Contract.Requires( !Disposed );

            if( data.Length == 0 )
            {
                return SendOne( new byte[0], flags );
            }
            if( data.Length == 1 )
            {
                return SendOne( data[0], flags );
            }
            else if( data.Length > 1 )
            {
                for( int i = 0; i < data.Length - 1; i++ )
                {
                    if( SendOne( data[i], flags | SendFlags.MoreToFollow ) == false )
                    {
                        return false;
                    }
                }

                return SendOne( data.Last(), flags );
            }

            return true;
        }

        /// <summary>
        /// Sends a (multipart) message over the socket.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Send( Message msg )
        {
            foreach( byte[] data in msg.Envelopes.Select( a => a.ToBytes() ) )
            {
                if( SendOne( data, SendFlags.MoreToFollow ) == false )
                {
                    return false;
                }

                if( SendOne( new byte[0], SendFlags.MoreToFollow ) == false )
                {
                    return false;
                }
            }

            return Send( msg.Body.ToArray() );
        }

        private bool SendOne( byte[] data, SendFlags flags )
        {
            Contract.Requires( !Disposed );
            Contract.Requires( data != null );

            if( C.zmq_msg_init_size( m_msg, data.Length ) != 0 )
            {
                throw ZeroMQException.CurrentError();
            }

            Marshal.Copy( data, 0, C.zmq_msg_data( m_msg ), data.Length );
            int rc = C.zmq_send( m_sock, m_msg, (int)flags );

            C.zmq_msg_close( m_msg );

            if( rc == 0 )
            {
                return true;
            }
            else if( C.zmq_errno() == C.EAGAIN )
            {
                return false;
            }
            else
            {
                throw ZeroMQException.CurrentError();
            }
        }

        #endregion
    }
}