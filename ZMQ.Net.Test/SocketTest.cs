using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace ZMQ.Net.Test
{
    /// <summary>
    ///This is a test class for SocketTest and is intended
    ///to contain all SocketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SocketTest
    {
        private static Context inprocCtx;

        private TestContext testContextInstance;
        private string inprocAddr = "inproc://test";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize( TestContext testContext )
        {
            inprocCtx = new Context();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            inprocCtx.Dispose();
            inprocCtx = null;
        }
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for Bind
        ///</summary>
        [TestMethod()]
        public void BindTest()
        {
            using( Socket sock = inprocCtx.CreateSocket( SocketType.Request ) )
            {
                sock.Bind( inprocAddr );
            }
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            using( Socket sock = inprocCtx.CreateSocket( SocketType.Request ) )
            {
                sock.Dispose();
                Assert.IsTrue( sock.Disposed );
            }
        }

        /// <summary>
        ///A test for GetOption
        ///</summary>
        [TestMethod()]
        public void GetOptionTest()
        {
            using( Socket sock = inprocCtx.CreateSocket( SocketType.Subscriber ) )
            {
                string sExpected = "";
                string sActual = sock.GetOption( SocketOption.SocketIdentity );
                Assert.AreEqual( sExpected, sActual );

                ulong ulExpected = 0;
                ulong ulActual = sock.GetOption( SocketOption.HighWaterMark );
                Assert.AreEqual( ulExpected, ulActual );

                long lExpected = 0;
                long lActual = sock.GetOption( SocketOption.SwapSize );
                Assert.AreEqual( lExpected, lActual );

                //TODO more GetOption tests
            }
        }

        /// <summary>
        ///A test for Receive
        ///</summary>
        [TestMethod()]
        public void SendReceiveTest()
        {
            byte[] data;
            byte[] rcvData;

            using( Socket rep = inprocCtx.CreateSocket( SocketType.Reply ) )
            {
                using( Socket req = inprocCtx.CreateSocket( SocketType.Request ) )
                {
                    rep.Bind( inprocAddr );
                    req.Connect( inprocAddr );

                    data = new byte[] { 1, 2, 3 };
                    req.Send( data );
                    rcvData = rep.Receive( ReceiveFlags.NonBlocking );

                    Assert.IsNotNull( rcvData );
                    Assert.AreEqual( data.Length, rcvData.Length );

                    for( int i = 0; i < data.Length; i++ )
                    {
                        Assert.AreEqual( data[i], rcvData[i] );
                    }

                    //reset rep/req state
                    rep.Send();
                    req.Receive();

                    data = new byte[0];
                    req.Send( data );
                    rcvData = rep.Receive();

                    Assert.IsNotNull( rcvData );
                    Assert.AreEqual( data.Length, rcvData.Length );

                    for( int i = 0; i < data.Length; i++ )
                    {
                        Assert.AreEqual( data[i], rcvData[i] );
                    }
                }
            }
        }

        [TestMethod]
        public void MultipartTest()
        {
            using( Socket rep = inprocCtx.CreateSocket( SocketType.Reply ) )
            {
                using( Socket req = inprocCtx.CreateSocket( SocketType.Request ) )
                {
                    rep.Bind( inprocAddr );
                    req.Connect( inprocAddr );

                    byte[] data1 = new byte[] { 1, 2, 3 };
                    byte[] data2 = new byte[] { 4, 5, 6 };
                    byte[] data3 = new byte[] { 7, 8, 9 };

                    MultipartTest( rep, req, data1, data2, data3 );

                    MultipartTest( rep, req, new byte[0] );
                }
            }
        }

        private static void MultipartTest( Socket rep, Socket req, params byte[][] data )
        {
            List<byte[]> rcv;

            req.Send( data );

            rcv = new List<byte[]>( rep.ReceiveMultiPart() );

            Assert.AreEqual( data.Length, rcv.Count );

            for( int i = 0; i < data.Length; i++ )
            {
                Assert.AreEqual( data[i].Length, rcv[i].Length );

                for( int j = 0; j < data[i].Length; j++ )
                {
                    Assert.AreEqual( data[i][j], rcv[i][j] );
                }
            }

            //reset rep/req state
            rep.Send();
            req.Receive();
        }

        /// <summary>
        ///A test for SetOption
        ///</summary>
        [TestMethod()]
        public void SetOptionTest()
        {
            using( Socket sock = inprocCtx.CreateSocket( SocketType.Subscriber ) )
            {
                string sValue = "test";
                sock.SetOption( SocketOption.SocketIdentity, sValue );
                Assert.AreEqual( sValue, sock.GetOption( SocketOption.SocketIdentity ) );

                sValue = "filter";
                sock.SetOption( SocketOption.Subscribe, sValue );
                sock.SetOption( SocketOption.Unsubscribe, sValue );

                ulong ulValue = 0;
                sock.SetOption( SocketOption.SendBuffer, ulValue );
                Assert.AreEqual( ulValue, sock.GetOption( SocketOption.SendBuffer ) );
            }
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        public void ConnectTest()
        {
            using( Socket rep = inprocCtx.CreateSocket( SocketType.Reply ) )
            {
                using( Socket req = inprocCtx.CreateSocket( SocketType.Request ) )
                {
                    rep.Bind( inprocAddr );
                    req.Connect( inprocAddr );
                }
            }
        }

        [TestMethod]
        public void MessageTest()
        {
            using( Socket rep = inprocCtx.CreateSocket( SocketType.Reply ) )
            {
                using( Socket req = inprocCtx.CreateSocket( SocketType.Request ) )
                {
                    rep.Bind( inprocAddr );
                    req.Connect( inprocAddr );

                    Message m = new Message();
                    m.Envelopes.Push( Address.FromString( "address1" ) );
                    m.Envelopes.Push( Address.FromString( "address2" ) );

                    m.Body.Add( new byte[] { 1, 2, 3 } );
                    m.Body.Add( new byte[] { 4, 5, 6 } );

                    req.Send( m );
                    Message rcv = rep.ReceiveMessage();

                    Assert.AreEqual( m, rcv );
                }
            }
        }
    }
}
