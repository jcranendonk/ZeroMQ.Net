using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace ZMQ.Net
{
    /// <summary>
    /// Address-wrapped message class.
    /// </summary>
    public class Message : IEquatable<Message>
    {
        /// <summary>
        /// Stack of address envelopes.
        /// </summary>
        public Stack<Address> Envelopes
        {
            get;
            private set;
        }

        /// <summary>
        /// Message body.
        /// </summary>
        public List<byte[]> Body
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new message.
        /// </summary>
        public Message()
        {
            Envelopes = new Stack<Address>();
            Body = new List<byte[]>();
        }

        /// <summary>
        /// Creates a new message from multipart data as received from a socket.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Message FromMultipart( IEnumerable<byte[]> data )
        {
            Contract.Requires( data != null );
            Contract.Ensures( Contract.Result<Message>() != null );
            Contract.Ensures( Contract.Result<Message>().Body != null );
            Contract.Ensures( Contract.Result<Message>().Envelopes != null );

            Message m = new Message();

            List<Address> addr = new List<Address>();
            byte[] curData = null;
            bool isBody = false;

            foreach( byte[] d in data )
            {
                if( isBody )
                {
                    m.Body.Add( d );
                }
                else
                {
                    if( curData == null )
                    {
                        curData = d;
                    }
                    else
                    {
                        if( d.Length == 0 )
                        {
                            addr.Add( Address.FromBytes( curData ) );
                        }
                        else
                        {
                            isBody = true;
                            m.Body.Add( curData );
                            m.Body.Add( d );
                        }

                        curData = null;
                    }
                }
            }

            // Consume any remaining data.
            if( curData != null )
            {
                m.Body.Add( curData );
            }

            m.Envelopes = new Stack<Address>( addr.Reverse<Address>() );

            return m;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder s = Envelopes.Aggregate( new StringBuilder( "Message[" ), ( sb, a ) => sb.Append( a ).Append( "[" ) );

            s.AppendFormat( "{0} bytes in {1} parts]", Body.Aggregate( 0, ( acc, d ) => acc += d.Length ), Body.Count );

            for( int i = 0; i < Envelopes.Count; i++ )
            {
                s.Append( "]" );
            }

            return s.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            if( obj != null && obj is Message )
            {
                return Equals( (Message)obj );
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Envelopes.Aggregate( 17, ( acc, a ) => acc = acc * 13 + a.GetHashCode() )
                    + Body.Aggregate( 13, ( acc, d ) => acc = acc * 19 + d.GetHashCode() );
        }

        #region IEquatable<Message> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals( Message other )
        {
            return other != null
                && CompareEnvelopes( this, other )
                && CompareBodies( this, other );
        }

        #endregion

        private static bool CompareEnvelopes( Message a, Message b )
        {
            if( a.Envelopes.Count != b.Envelopes.Count )
            {
                return false;
            }

            var e1 = a.Envelopes.ToArray();
            var e2 = b.Envelopes.ToArray();

            for( int i = 0; i < a.Envelopes.Count; i++ )
            {
                if( e1[i].Equals( e2[i] ) == false )
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CompareBodies( Message a, Message b )
        {
            if( a.Body.Count != b.Body.Count )
            {
                return false;
            }

            if( Enumerable.Range( 0, a.Body.Count ).Any( x => a.Body[x].Length != b.Body[x].Length ) )
            {
                return false;
            }

            for( int i = 0; i < a.Body.Count; i++ )
            {
                for( int j = 0; j < a.Body[i].Length; j++ )
                {
                    if( a.Body[i][j] != b.Body[i][j] )
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Socket address.
    /// </summary>
    public class Address : IEquatable<Address>
    {
        private bool m_isUUID;
        private byte[] m_addr;

        private Address()
        {
        }

        /// <summary>
        /// Creates an address from a string. Expects ASCII.
        /// </summary>
        /// <param name="address"></param>
        public static Address FromString( string address )
        {
            Contract.Requires( address != null );
            Contract.Requires( address.Length > 0 );
            Contract.Ensures( Contract.Result<Address>() != null );

            Address addr = new Address();

            addr.m_isUUID = false;
            addr.m_addr = Encoding.ASCII.GetBytes( address );

            return addr;
        }

        /// <summary>
        /// Creates an address from a UUID in a byte array.
        /// </summary>
        /// <param name="uuid"></param>
        public static Address FromUUID( byte[] uuid )
        {
            Contract.Requires( uuid != null );
            Contract.Requires( uuid.Length == 17 );
            Contract.Requires( uuid[0] == 0 );
            Contract.Ensures( Contract.Result<Address>() != null );

            Address addr = new Address();

            addr.m_isUUID = true;
            addr.m_addr = new byte[17];
            uuid.CopyTo( addr.m_addr, 0 );

            return addr;
        }

        /// <summary>
        /// Creates an address from a byte array (be it a UUID or an encoded string).
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Address FromBytes( byte[] bytes )
        {
            Contract.Requires( bytes != null );
            Contract.Requires( bytes.Length > 0 );
            Contract.Ensures( Contract.Result<Address>() != null );

            if( bytes.Length == 17 && bytes[0] == 0 )
            {
                return FromUUID( bytes );
            }
            else
            {
                Address addr = new Address();

                addr.m_isUUID = false;
                addr.m_addr = new byte[bytes.Length];
                bytes.CopyTo( addr.m_addr, 0 );

                return addr;
            }
        }

        /// <summary>
        /// Creates an address from a newly-generated UUID.
        /// </summary>
        /// <returns></returns>
        public static Address NewUUID()
        {
            Contract.Ensures( Contract.Result<Address>() != null );

            Address addr = new Address();

            addr.m_isUUID = true;
            addr.m_addr = new byte[17];
            addr.m_addr[0] = 0;

            Guid.NewGuid().ToByteArray().CopyTo( addr.m_addr, 1 );

            return addr;
        }

        /// <summary>
        /// Returns address as byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            Contract.Ensures( Contract.Result<byte[]>() != null );

            byte[] address = new byte[m_addr.Length];
            m_addr.CopyTo( address, 0 );
            return address;
        }

        /// <summary>
        /// Returns human-readable representation of the object, with UUID addresses hex-encoded.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if( m_isUUID )
            {
                return "@" + m_addr.Skip( 1 ).Aggregate( new StringBuilder(), ( sb, b ) => sb.Append( b.ToString( "X2" ) ) ).ToString();
            }
            else
            {
                return "@" + Encoding.ASCII.GetString( m_addr );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            if( obj != null && obj is Address )
            {
                return Equals( (Address)obj );
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_addr.Aggregate( 7, ( acc, a ) => acc = acc * 11 + a );
        }

        #region IEquatable<Address> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals( Address other )
        {
            if( other != null
                && m_isUUID == other.m_isUUID
                && m_addr.Length == other.m_addr.Length )
            {
                for( int i = 0; i < m_addr.Length; i++ )
                {
                    if( m_addr[i] != other.m_addr[i] )
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
