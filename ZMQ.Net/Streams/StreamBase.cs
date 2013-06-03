using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace ZMQ.Net
{
    /// <summary>
    /// Base class for Streams that use ØMQ as underlying transport.
    /// </summary>
    public abstract class ZMQStream : Stream
    {
        private static Context defaultContext;

        private Context m_context;
        private Socket m_socket;
        private byte[] m_buffer;
        private int m_bufOffset;

        #region Public properties

        /// <summary>
        /// Gets a value that indicates whether the stream supports seeking. This 
        /// property always returns false.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Gets the length of the data available on the stream. This property is 
        /// not currently supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the current position in the stream. This property is 
        /// not currently supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets whether the stream has been disposed.
        /// </summary>
        public bool Disposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether any data from a partial message from a previous <see cref="Read(byte[],int,int)"/> call is available in the buffer.
        /// </summary>
        public bool DataAvailable
        {
            get
            {
                Contract.Requires( Disposed == false, "Stream has been disposed." );

                return ( CanRead == true && m_buffer != null );
            }
        }

        #endregion

        #region Constructor

        static ZMQStream()
        {
            defaultContext = new Context();
        }

        /// <summary>
        /// Creates a new stream of the specified type for the specified context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socketType"></param>
        /// <param name="lingerTime">Linger period for socket shutdown, in ms, or <see cref="Timeout.Infinite"/> for no linger.</param>
        protected ZMQStream( Context context, SocketType socketType, int lingerTime = Context.DEFAULT_LINGER )
            : base()
        {
            Contract.Requires( context != null );
            Contract.Requires( lingerTime >= 0 || lingerTime == Timeout.Infinite );

            Disposed = false;

            m_context = context;
            m_socket = m_context.CreateSocket( socketType );

            if( lingerTime >= 0 )
            {
                m_socket.SetOption( SocketOption.Linger, lingerTime );
            }
        }

        /// <summary>
        /// Creates a new stream of the specified type for the default ØMQ context 
        /// that is shared amongst <see cref="ZMQStream"/> objects. This context 
        /// has an unspecified thread pool size, for use with the inproc transport. 
        /// <seealso cref="Context()"/>
        /// </summary>
        /// <param name="socketType"></param>
        /// <param name="lingerTime">Linger period for socket shutdown, in ms, or <see cref="Timeout.Infinite"/> for no linger.</param>
        protected ZMQStream( SocketType socketType, int lingerTime = Context.DEFAULT_LINGER )
            : this( defaultContext, socketType, lingerTime )
        {
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Flushes data from the stream.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Sets the current position of the stream to the given value. This method is 
        /// not currently supported and always throws a <see cref="NotSupportedException"/>. 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek( long offset, SeekOrigin origin )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream. This method always throws a <see cref="NotSupportedException"/>. 
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength( long value )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads data from the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read( byte[] buffer, int offset, int count )
        {
            if( Disposed == true )
            {
                throw new ObjectDisposedException( GetType().ToString() );
            }

            if( CanRead == false )
            {
                throw new NotSupportedException( "Stream does not support reading." );
            }

            if( m_buffer == null )
            {
                m_buffer = Read();
                m_bufOffset = 0;
            }

            int length = count;

            //Cap length to remaining buffered data.
            if( m_buffer.Length - m_bufOffset <= count )
            {
                length = m_buffer.Length - m_bufOffset;
            }

            Array.Copy( m_buffer, m_bufOffset, buffer, offset, length );
            m_bufOffset += length;

            if( m_bufOffset >= m_buffer.Length )
            {
                //Entire message has been read out of the buffer, clean up.
                m_buffer = null;
                m_bufOffset = 0;
            }

            return length;
        }

        /// <summary>
        /// Reads a full message from the stream.
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            Contract.Requires( Disposed == false, "Stream has been disposed." );
            Contract.Requires( CanRead == true, "Stream does not support reading." );
            Contract.Requires( DataAvailable == false, "A partial message is available in the buffer, finish reading it first." );
            Contract.Ensures( Contract.Result<byte[]>() != null );

            byte[] buf;

            try
            {
                if( m_socket.TryReceive( out buf ) == false )
                {
                    throw new IOException( "Read failed." );
                }
            }
            catch( ZeroMQException zmqex )
            {
                throw new IOException( "Read failed.", zmqex );
            }

            return buf;
        }

        /// <summary>
        /// Attempts to receive a full message from the stream for a specified amount of time.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait for a message. Use 0 to check if a message is available and return it immediately.
        /// Use <see cref="Timeout.Infinite"/> to block until a message is available.</param>
        /// <returns>A message received from the stream, or null if <paramref name="timeout"/> was 0 and no message was available.</returns>
        public byte[] Read( int timeout )
        {
            Contract.Requires( Disposed == false, "Stream has been disposed." );
            Contract.Requires( CanRead == true, "Stream does not support reading." );
            Contract.Requires( DataAvailable == false, "A partial message is available in the buffer, finish reading it first." );
            Contract.Requires( timeout >= 0 || timeout == Timeout.Infinite );

            try
            {
                if( timeout == Timeout.Infinite )
                {
                    return Read();
                }
                else if( timeout == 0 )
                {
                    return m_socket.Receive( ReceiveFlags.NonBlocking );
                }
                else
                {
                    byte[] buf;
                    Stopwatch sw = Stopwatch.StartNew();

                    do
                    {
                        if( m_socket.TryReceive( out buf ) == true )
                        {
                            return buf;
                        }

                        Thread.Sleep( 0 );
                    } while( sw.ElapsedMilliseconds < timeout );

                    sw.Stop();

                    throw new TimeoutException();
                }
            }
            catch( ZeroMQException zmqex )
            {
                throw new IOException( "Read failed.", zmqex );
            }
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write( byte[] buffer, int offset, int count )
        {
            if( Disposed == true )
            {
                throw new ObjectDisposedException( GetType().ToString() );
            }

            if( CanWrite == false )
            {
                throw new NotSupportedException( "Stream does not support writing." );
            }

            byte[] msgbuf = new byte[count];
            Array.Copy( buffer, offset, msgbuf, 0, count );

            Write( msgbuf );
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer"></param>
        public void Write( byte[] buffer )
        {
            Contract.Requires( Disposed == false, "Stream has been disposed." );
            Contract.Requires( CanWrite == true, "Stream does not support writing." );
            Contract.Requires( buffer != null );

            try
            {
                if( m_socket.Send( buffer ) == false )
                {
                    throw new IOException( "Write failed." );
                }
            }
            catch( ZeroMQException zmqex )
            {
                throw new IOException( "Write failed.", zmqex );
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources.</param>
        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

            if( Disposed == false )
            {
                if( disposing == true )
                {
                    if( m_socket != null )
                    {
                        m_socket.Dispose();
                    }
                }

                Disposed = true;
            }
        }

        #endregion
    }
}